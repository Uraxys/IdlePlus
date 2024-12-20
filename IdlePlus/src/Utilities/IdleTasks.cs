using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdlePlus.Utilities {
	
	/// <summary>
	/// A simple task utility that allows you to run a task either every frame
	/// or every specified interval.
	/// </summary>
	public static class IdleTasks {

		private static readonly List<IdleTask> Tasks = new List<IdleTask>();
		
		public static void Tick() {
			for (var i = 0; i < Tasks.Count; i++) {
				if (Tasks[i].Tick()) continue;
				Tasks.RemoveAt(i);
				i--;
			}
		}
		
		/// <summary>
		/// Create a new task that runs next frame
		/// </summary>
		/// <param name="action">The action to run every frame.</param>
		/// <returns>The created task.</returns>
		public static IdleTask Run(Action action) {
			var task = new IdleTask(idleTask => {
				try {
					action.Invoke();
				} catch (Exception e) {
					IdleLog.Error("Error while running task!", e);
				}
				idleTask.Cancel();
			}, -1F, -1F);
			Tasks.Add(task);
			return task;
		}
		
		public static IdleTask Delay(float delay, Action action) {
			if (delay < 0) throw new ArgumentException("Delay must be greater than 0.");
			var task = new IdleTask(idleTask => {
				try {
					action.Invoke();
				} catch (Exception e) {
					IdleLog.Error("Error while running task!", e);
				}
				idleTask.Cancel();
			}, delay, -1F);
			Tasks.Add(task);
			return task;
		}
		
		/// <summary>
		/// Create a new task that runs every interval seconds until it's
		/// cancelled.
		/// </summary>
		/// <param name="delay">The delay in seconds before the task starts.</param>
		/// <param name="interval">The interval in seconds.</param>
		/// <param name="action">The action to run every interval.</param>
		/// <returns>The created task.</returns>
		/// <exception cref="ArgumentException">If the interval is less than 0.</exception>
		public static IdleTask Repeat(float delay, float interval, Action<IdleTask> action) {
			if (interval < 0) throw new ArgumentException("Interval must be greater than 0.");
			var task = new IdleTask(action, delay, interval);
			Tasks.Add(task);
			return task;
		}

		public static IdleTask Update(GameObject obj, Action action) {
			var task = new IdleTask(idleTask => {
				// ReSharper disable once ConditionIsAlwaysTrueOrFalse
				if (obj == null) {
					IdleLog.Info("Object was destroyed, cancelling task.");
					idleTask.Cancel();
					return;
				}
				
				if (!obj.activeInHierarchy) return;
				
				action.Invoke();
			}, -1, -1);
			Tasks.Add(task);
			return task;
		}
		
		public class IdleTask {
			private readonly Action<IdleTask> _taskAction;
			private readonly Action _action;
			private readonly float _interval;
			private readonly int _frameStart;

			private float _time;
			private float _delay;
			private bool _cancelled;

			public IdleTask(Action<IdleTask> taskAction, float delay, float interval) {
				_taskAction = taskAction;
				_action = null;
				_delay = delay;
				_interval = interval;
				_frameStart = Time.frameCount;
			}

			/// <summary>
			/// Ticks the task, returns false if the task has been stopped.
			/// </summary>
			/// <returns>True if the task should continue ticking, false if it
			/// should be removed.</returns>
			public bool Tick() {
				// Check if the task has been cancelled before ticking, this
				// might happen if it's been cancelled from the outside.
				if (_cancelled) return false;
				var dt = Time.deltaTime;
				
				// Tasks always start the next frame, so if the frame count is
				// still the same, return true.
				if (_frameStart == Time.frameCount) return true;
				
				// Check for delay.
				if (_delay > 0) {
					if (_delay > dt) {
						_delay -= dt;
						return true;
					}

					// Delay is less than the delta time, modify the delta time
					// and remove the delay.
					_delay = -1;
					dt -= _delay;
				}
				
				// If the interval is less than 0, then run the task once per
				// frame.
				if (_interval < 0) {
					Run();
					return !_cancelled;
				}
				
				// Interval is greater than 0, increment the time and check if
				// it's time to run the task.
				_time += dt;
				if (_time < _interval) return true;

				Run();
				_time -= _interval;
				return !_cancelled;
			}
			
			/// <summary>
			/// Cancels the current task.
			/// </summary>
			public void Cancel() {
				_cancelled = true;
			}
			
			private void Run() {
				try {
					if (_taskAction != null) _taskAction(this);
					else _action();
				} catch (Exception e) {
					IdleLog.Error("Error while running task!", e);
				}
			}
		}
		
	}
}