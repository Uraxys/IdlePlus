using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdlePlus.Utilities
{
    /// <summary>
    /// Generic queue for asynchronous processing of items with parallel execution.
    /// </summary>
    /// <typeparam name="T">The type of items in the queue.</typeparam>
    public abstract class WebhookQueue<T>
    {
        private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
        private readonly SemaphoreSlim _processingLock = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _parallelismLimiter;
        private volatile bool _isProcessing;
        private readonly int _maxConsecutiveErrors;
        private int _consecutiveErrors;
        private CancellationTokenSource _cancellationTokenSource;
        private readonly List<Task> _runningTasks = new List<Task>();
        private readonly object _tasksLock = new object();

        /// <summary>
        /// Gets the current number of active parallel tasks.
        /// </summary>
        public int ActiveTasksCount
        {
            get
            {
                lock (_tasksLock)
                {
                    // Zähle nur nicht abgeschlossene Tasks
                    return _runningTasks.Count;
                }
            }
        }

        /// <summary>
        /// Gets the number of items in the queue.
        /// </summary>
        public int Count => _queue.Count;

        /// <summary>
        /// Initializes a new instance of the WebhookQueue class.
        /// </summary>
        /// <param name="maxParallelTasks">Maximum number of parallel tasks to run.</param>
        /// <param name="maxConsecutiveErrors">Maximum number of consecutive errors before pausing processing.</param>
        protected WebhookQueue(int maxParallelTasks = 10, int maxConsecutiveErrors = 5)
        {
            _parallelismLimiter = new SemaphoreSlim(maxParallelTasks, maxParallelTasks);
            _maxConsecutiveErrors = maxConsecutiveErrors;
            _cancellationTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// Adds an item to the queue for processing.
        /// </summary>
        /// <param name="item">The item to enqueue.</param>
        public void Enqueue(T item)
        {
            if (item == null)
            {
                IdleLog.Error("[WebhookQueue] Cannot enqueue null item");
                return;
            }

            _queue.Enqueue(item);
            IdleLog.Debug($"[WebhookQueue] Item added to queue. Queue size: {_queue.Count}");
            StartProcessingIfNotRunning();
        }

        /// <summary>
        /// Starts the queue processor if it is not already running.
        /// </summary>
        private async void StartProcessingIfNotRunning()
        {
            bool shouldProcess = false;
            
            try
            {
                await _processingLock.WaitAsync();
                
                if (_isProcessing) return;
                _isProcessing = true;
                shouldProcess = true;
                
                // Reset cancellation token if it was cancelled
                if (_cancellationTokenSource.IsCancellationRequested)
                {
                    _cancellationTokenSource.Dispose();
                    _cancellationTokenSource = new CancellationTokenSource();
                }
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[WebhookQueue] Error in StartProcessingIfNotRunning: {ex.Message}");
                _isProcessing = false;
            }
            finally
            {
                _processingLock.Release();
            }

            if (shouldProcess)
            {
                try
                {
                    await ProcessQueueAsync(_cancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    IdleLog.Debug("[WebhookQueue] Queue processing was cancelled");
                }
                catch (Exception ex)
                {
                    IdleLog.Error($"[WebhookQueue] Unhandled error in queue processing: {ex.Message}");
                }
                
                await _processingLock.WaitAsync();
                try
                {
                    _isProcessing = false;
                }
                finally
                {
                    _processingLock.Release();
                }
            }
        }

        /// <summary>
        /// Continuously processes the queue asynchronously with parallel execution.
        /// </summary>
        private async Task ProcessQueueAsync(CancellationToken cancellationToken)
        {
            IdleLog.Debug("[WebhookQueue] Started processing queue in parallel mode");
            
            while (!cancellationToken.IsCancellationRequested)
            {
                // Prüfe, ob die Queue leer ist
                if (_queue.IsEmpty)
                {
                    // Warten, bis alle laufenden Tasks abgeschlossen sind
                    if (ActiveTasksCount > 0)
                    {
                        IdleLog.Debug($"[WebhookQueue] Queue empty, waiting for {ActiveTasksCount} tasks to complete");
                        await Task.Delay(100, cancellationToken);
                        continue;
                    }
                    else
                    {
                        // Keine Items und keine laufenden Tasks - wir sind fertig
                        IdleLog.Debug("[WebhookQueue] Queue processing complete");
                        break;
                    }
                }

                // Zu viele aufeinanderfolgende Fehler - Pause machen
                if (_consecutiveErrors >= _maxConsecutiveErrors)
                {
                    IdleLog.Error($"[WebhookQueue] Pausing processing after {_consecutiveErrors} consecutive errors. Will retry in 30 seconds.");
                    try
                    {
                        await Task.Delay(30000, cancellationToken); // Wait 30 seconds
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    _consecutiveErrors = 0;  // Reset the counter and try again
                }

                // Versuche, ein Element aus der Queue zu nehmen
                if (_queue.TryDequeue(out var item))
                {
                    // Warte, bis ein Slot für eine neue parallele Task verfügbar ist
                    await _parallelismLimiter.WaitAsync(cancellationToken);
                    
                    // Starte die Verarbeitung als separate Task
                    var processingTask = Task.Run(async () =>
                    {
                        try
                        {
                            await ProcessItemAsync(item);
                            Interlocked.Exchange(ref _consecutiveErrors, 0); // Thread-safe Reset
                            IdleLog.Debug($"[WebhookQueue] Task completed successfully, active tasks: {ActiveTasksCount}");
                        }
                        catch (Exception ex)
                        {
                            Interlocked.Increment(ref _consecutiveErrors);
                            IdleLog.Error($"[WebhookQueue] Error processing item: {ex.Message}");
                        }
                        finally
                        {
                            // Freigabe des parallelen Slots
                            _parallelismLimiter.Release();
                        }
                    }, cancellationToken);
                    
                    // Fügen Sie eine Continuation hinzu, um die Task aus der Liste zu entfernen
                    var task = processingTask.ContinueWith(t => 
                    {
                        lock (_tasksLock)
                        {
                            _runningTasks.Remove(processingTask);
                        }
                    }, TaskContinuationOptions.ExecuteSynchronously);
                    
                    // Füge die Task zur Liste der aktiven Tasks hinzu
                    lock (_tasksLock)
                    {
                        _runningTasks.Add(processingTask);
                        IdleLog.Debug($"[WebhookQueue] Started new task, active tasks: {_runningTasks.Count}");
                    }
                }
                else
                {
                    // Warte kurz, wenn wir kein Element entnehmen konnten
                    await Task.Delay(50, cancellationToken);
                }
            }
            
            // Warte auf Abschluss aller laufenden Tasks
            Task[] tasksToWaitFor;
            lock (_tasksLock)
            {
                tasksToWaitFor = _runningTasks.ToArray();
            }
            
            if (tasksToWaitFor.Length > 0)
            {
                IdleLog.Debug($"[WebhookQueue] Waiting for {tasksToWaitFor.Length} tasks to complete");
                await Task.WhenAll(tasksToWaitFor);
            }
            
            IdleLog.Debug("[WebhookQueue] Queue processing finished");
        }

        /// <summary>
        /// Cancels all ongoing queue processing.
        /// </summary>
        public void CancelProcessing()
        {
            try
            {
                _cancellationTokenSource.Cancel();
                IdleLog.Debug("[WebhookQueue] Processing cancelled");
            }
            catch (Exception ex)
            {
                IdleLog.Error($"[WebhookQueue] Error cancelling processing: {ex.Message}");
            }
        }

        /// <summary>
        /// Processes a single item from the queue.
        /// </summary>
        /// <param name="item">The item to process.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        protected abstract Task ProcessItemAsync(T item);
    }
}