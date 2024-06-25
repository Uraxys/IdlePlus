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
		
		public static void Update() {
			for (var i = 0; i < Tasks.Count; i++) {
				if (Tasks[i].Tick()) continue;
				Tasks.RemoveAt(i);
				i--;
			}
		}
		
		/// <summary>
		/// Create a new task that runs every frame until it's cancelled.
		/// </summary>
		/// <param name="action">The action to run every frame.</param>
		/// <returns>The created task.</returns>
		public static IdleTask Run(Action<IdleTask> action) {
			var task = new IdleTask(action, -1F);
			Tasks.Add(task);
			return task;
		}
		
		/// <summary>
		/// Create a new task that runs every interval seconds until it's
		/// cancelled.
		/// </summary>
		/// <param name="interval">The interval in seconds.</param>
		/// <param name="action">The action to run every interval.</param>
		/// <returns>The created task.</returns>
		/// <exception cref="ArgumentException">If the interval is less than or equal to 0.</exception>
		public static IdleTask Interval(float interval, Action<IdleTask> action) {
			if (interval <= 0) throw new ArgumentException("Interval must be greater than 0");
			var task = new IdleTask(action, interval);
			Tasks.Add(task);
			return task;
		}
		
		public class IdleTask {
			private readonly Action<IdleTask> _taskAction;
			private readonly Action _action;
			private readonly float _interval;
			
			private float _time;
			private bool _cancelled;

			public IdleTask(Action<IdleTask> taskAction, float interval) {
				_taskAction = taskAction;
				_action = null;
				_interval = interval;
			}

			public IdleTask(Action action, float interval) {
				_taskAction = null;
				_action = action;
				_interval = interval;
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
				
				// If the interval is less than 0, then run the task once per
				// frame.
				if (_interval < 0) {
					Run();
					return !_cancelled;
				}
				
				// Interval is greater than 0, increment the time and check if
				// it's time to run the task.
				_time += Time.deltaTime;
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