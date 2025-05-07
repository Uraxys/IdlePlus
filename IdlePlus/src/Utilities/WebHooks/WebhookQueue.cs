using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace IdlePlus.Utilities {
	/// <summary>
	/// Generic queue for asynchronous processing of items with parallel execution.
	/// </summary>
	/// <typeparam name="T">The type of items in the queue.</typeparam>
	public abstract class WebhookQueue<T> {
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
		public int ActiveTasksCount {
			get {
				lock (_tasksLock) {
					// Count only non-completed tasks
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
		protected WebhookQueue(int maxParallelTasks = 10, int maxConsecutiveErrors = 5) {
			_parallelismLimiter = new SemaphoreSlim(maxParallelTasks, maxParallelTasks);
			_maxConsecutiveErrors = maxConsecutiveErrors;
			_cancellationTokenSource = new CancellationTokenSource();
		}

		/// <summary>
		/// Adds an item to the queue for processing.
		/// </summary>
		/// <param name="item">The item to enqueue.</param>
		public void Enqueue(T item) {
			if (item == null) {
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
		private async void StartProcessingIfNotRunning() {
			bool shouldProcess = false;

			try {
				await _processingLock.WaitAsync();

				if (_isProcessing) return;
				_isProcessing = true;
				shouldProcess = true;

				// Reset cancellation token if it was cancelled
				if (_cancellationTokenSource.IsCancellationRequested) {
					_cancellationTokenSource.Dispose();
					_cancellationTokenSource = new CancellationTokenSource();
				}
			} catch (Exception ex) {
				IdleLog.Error($"[WebhookQueue] Error in StartProcessingIfNotRunning: {ex.Message}");
				_isProcessing = false;
			} finally {
				_processingLock.Release();
			}

			if (shouldProcess) {
				try {
					await ProcessQueueAsync(_cancellationTokenSource.Token);
				} catch (TaskCanceledException) {
					IdleLog.Debug("[WebhookQueue] Queue processing was cancelled");
				} catch (Exception ex) {
					IdleLog.Error($"[WebhookQueue] Unhandled error in queue processing: {ex.Message}");
				}

				await _processingLock.WaitAsync();
				try {
					_isProcessing = false;
				} finally {
					_processingLock.Release();
				}
			}
		}

		/// <summary>
		/// Continuously processes the queue asynchronously with parallel execution.
		/// </summary>
		/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
		private async Task ProcessQueueAsync(CancellationToken cancellationToken) {
			IdleLog.Debug("[WebhookQueue] Started processing queue in parallel mode");

			try {
				while (!cancellationToken.IsCancellationRequested) {
					// Check if the queue is empty
					if (_queue.IsEmpty) {
						// Wait until all running tasks are completed
						if (ActiveTasksCount > 0) {
							IdleLog.Debug($"[WebhookQueue] Queue empty, waiting for {ActiveTasksCount} tasks to complete");
							await Task.Delay(100, cancellationToken);
							continue;
						} else {
							// No items and no running tasks - we're done
							IdleLog.Debug("[WebhookQueue] Queue processing complete");
							break;
						}
					}

					// Too many consecutive errors - take a pause
					if (_consecutiveErrors >= _maxConsecutiveErrors) {
						IdleLog.Error($"[WebhookQueue] Pausing processing after {_consecutiveErrors} consecutive errors. Will retry in 30 seconds.");
						try {
							await Task.Delay(30000, cancellationToken); // Wait 30 seconds
						} catch (TaskCanceledException) {
							break;
						}
						_consecutiveErrors = 0;  // Reset the counter and try again
					}

					// Try to take an element from the queue
					if (_queue.TryDequeue(out var item)) {
						// Wait until a slot for a new parallel task is available
						await _parallelismLimiter.WaitAsync(cancellationToken);

						// Start processing as a separate task
						var processingTask = Task.Run(async () => {
							try {
								await ProcessItemAsync(item);
								Interlocked.Exchange(ref _consecutiveErrors, 0); // Thread-safe reset
								IdleLog.Debug($"[WebhookQueue] Task completed successfully, active tasks: {ActiveTasksCount}");
							} catch (Exception ex) {
								Interlocked.Increment(ref _consecutiveErrors);
								IdleLog.Error($"[WebhookQueue] Error processing item: {ex.Message}");
							} finally {
								// Release the parallel slot
								_parallelismLimiter.Release();
							}
						}, cancellationToken);

						// Add a continuation to remove the task from the list
						var task = processingTask.ContinueWith(t => {
							lock (_tasksLock) {
								_runningTasks.Remove(processingTask);
							}
						}, TaskContinuationOptions.ExecuteSynchronously);

						// Add the task to the list of active tasks
						lock (_tasksLock) {
							_runningTasks.Add(processingTask);
							IdleLog.Debug($"[WebhookQueue] Started new task, active tasks: {_runningTasks.Count}");
						}
					} else {
						// Wait briefly if we couldn't get an item
						await Task.Delay(50, cancellationToken);
					}
				}
			} finally {
				// Wait for completion of all running tasks
				Task[] tasksToWaitFor;
				lock (_tasksLock) {
					tasksToWaitFor = _runningTasks.ToArray();
					_runningTasks.Clear();  // Clear the list to prevent memory leaks
				}

				if (tasksToWaitFor.Length > 0) {
					IdleLog.Debug($"[WebhookQueue] Waiting for {tasksToWaitFor.Length} tasks to complete during cleanup");
					try {
						// Use a timeout to avoid hanging
						var waitTask = Task.WhenAll(tasksToWaitFor);
						var completedTask = await Task.WhenAny(waitTask, Task.Delay(2000));

						if (completedTask != waitTask) {
							IdleLog.Warn("[WebhookQueue] Some tasks did not complete within timeout period during cleanup");
						}
					} catch (Exception ex) {
						IdleLog.Error($"[WebhookQueue] Error waiting for tasks during cleanup: {ex.Message}");
					}
				}

				IdleLog.Debug("[WebhookQueue] Queue processing finished and resources cleaned up");
			}
		}

		/// <summary>
		/// Cancels all ongoing queue processing.
		/// </summary>
		public void CancelProcessing() {
			try {
				_cancellationTokenSource.Cancel();
				IdleLog.Debug("[WebhookQueue] Processing cancelled");
			} catch (Exception ex) {
				IdleLog.Error($"[WebhookQueue] Error cancelling processing: {ex.Message}");
			}
		}

		/// <summary>
		/// Cancels all ongoing queue processing and waits until all tasks are completed.
		/// </summary>
		/// <param name="timeoutMs">Timeout in milliseconds for waiting on task completion.</param>
		/// <returns>A Task representing the asynchronous operation.</returns>
		public async Task CancelAndWaitAsync(int timeoutMs = 5000) {
			try {
				// Cancel the processing
				_cancellationTokenSource?.Cancel();
				IdleLog.Debug("[WebhookQueue] Processing cancelled, waiting for tasks to complete");

				// Wait for all active tasks to complete
				Task[] tasksToWaitFor;
				lock (_tasksLock) {
					tasksToWaitFor = _runningTasks.ToArray();
				}

				if (tasksToWaitFor.Length > 0) {
					// Use a timeout in case tasks get stuck
					var waitTask = Task.WhenAll(tasksToWaitFor);
					var completedTask = await Task.WhenAny(waitTask, Task.Delay(timeoutMs));

					if (completedTask != waitTask) {
						IdleLog.Warn($"[WebhookQueue] Timeout waiting for {tasksToWaitFor.Length} tasks to complete");
					} else {
						IdleLog.Debug($"[WebhookQueue] All {tasksToWaitFor.Length} tasks completed successfully");
					}
				}
			} catch (Exception ex) {
				IdleLog.Error($"[WebhookQueue] Error during cancel and wait: {ex.Message}");
			}
		}

		/// <summary>
		/// Releases all resources used by the WebhookQueue.
		/// </summary>
		public void Dispose() {
			try {
				CancelProcessing();

				// Wait briefly to allow running tasks to terminate
				Task.Delay(100).Wait();

				// Dispose of semaphores
				_processingLock?.Dispose();
				_parallelismLimiter?.Dispose();

				// Dispose of CancellationTokenSource
				_cancellationTokenSource?.Dispose();

				IdleLog.Debug("[WebhookQueue] Resources disposed properly");
			} catch (Exception ex) {
				IdleLog.Error($"[WebhookQueue] Error during disposal: {ex.Message}");
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