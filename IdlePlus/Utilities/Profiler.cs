using System;
using System.Collections.Generic;

namespace IdlePlus.Utilities {
	public class Profiler {
		
		private const string RootSection = "main";
		
		private readonly List<string> _sections = new List<string>();
		private readonly List<long> _timestamps = new List<long>();
		private readonly Dictionary<string, long> _profiling = new Dictionary<string, long>();
		private string _currentSection;
		
		/// <summary>
		/// If the Profiler should be enabled the next time it's started.
		/// </summary>
		private bool _enabledState;
		
		/// <summary>
		/// If the Profiler is currently running.
		/// </summary>
		private bool _enabled;

		public Profiler(bool enabled = false) {
			//_instance = this;
			_enabled = enabled;
		}

		public void Start() {
			_enabled = _enabledState;
			if (!_enabled) return;

			if (_currentSection != null) {
				IdleLog.Warn("Profiler: Tried to start, but it's already running.");
				return;
			}
			
			_currentSection = RootSection;
			_sections.Add(_currentSection);
			_timestamps.Add(GetNanos());
		}
		
		public void Stop() {
			if (!_enabled) return;
			
			if (_currentSection != RootSection) {
				IdleLog.Warn($"Profiler: Tried to disable, but not all sections were closed!\n" +
				             $"Current section: {_currentSection}");
				return;
			}
			
			// Stop the main section.
			Pop();
		}
		
		public List<Report> GetReport(string section = null, bool includeTopLevel = true) {
			if (!IsEnabled()) return new List<Report>();
			
			List<Report> results = new List<Report>();
			section = section == null ? RootSection : $"{RootSection}.{section}";
			
			// Get the total time for the target and the root section.
			if (!_profiling.TryGetValue(RootSection, out var totalTime)) totalTime = 0;
			if (!_profiling.TryGetValue(section, out var targetTime)) targetTime = 0;
			var trackedTime = 0L;
			
			// Get the time of all subsections from the profiling.
			foreach (var pair in _profiling) {
				var key = pair.Key;
				// Only get subsections, and not sub subsections.
				if (key.Length > section.Length && key.StartsWith(section) &&
				    key.IndexOf('.', section.Length + 1) == -1) {
					trackedTime += pair.Value;
				}
			}
			
			// Add the result.
			foreach (var pair in _profiling) {
				var key = pair.Key;
				if (key.Length > section.Length && key.StartsWith(section) &&
				    key.IndexOf('.', section.Length + 1) == -1) {
					// Get the time and calculate the percentage.
					var time = _profiling[key];
					var totalPercentage = time * 100D / totalTime;
					var sectionPercentage = time * 100D / trackedTime;
					var subSection = key.Substring(section.Length + 1);
					// Add the result
					results.Add(new Report(subSection, key.Substring(RootSection.Length + 1), time, totalPercentage,
						sectionPercentage));
				}
			}
			
			// Add untracked time.
			if (trackedTime < targetTime) {
				var time = targetTime - trackedTime;
				var sectionPercentage = time * 100D / trackedTime;
				var totalPercentage = time * 100D / totalTime;
				results.Add(new Report("#untracked", null, time, totalPercentage, sectionPercentage));
			}
			
			// Decay the profiling time.
			foreach (var pair in _profiling) {
				_profiling[pair.Key] = (long) (pair.Value * 0.999D);
			}
			
			// Sort the results by time then add the current section at the top.
			results.Sort((a, b) => b.Time.CompareTo(a.Time));
			if (includeTopLevel) results.Insert(0, new Report(section, section, targetTime, 
				targetTime * 100D / totalTime, 100));
			return results;
		}

		public void Enable() {
			_enabledState = true;
		}

		public void Disable() {
			_enabledState = false;
		}
		
		public bool IsEnabled() {
			return _enabled;
		}

		public void Push(string section) {
			if (!IsEnabled()) return;
			
			if (_currentSection == null) {
				IdleLog.Warn("Profiler: Tried to push, but no sections are open.");
				return;
			}
			
			// Begin the section.
			_currentSection += $".{section}";
			_sections.Add(_currentSection);
			_timestamps.Add(GetNanos());
		}
		
		public void Pop() {
			if (!IsEnabled()) return;

			if (_currentSection == null) {
				IdleLog.Warn("Profiler: Tried to pop, but no sections are open.");
				return;
			}

			// End the section.
			var endTime = GetNanos();
			var startTime = RemoveAndGet(_timestamps, _timestamps.Count - 1);
			var time = endTime - startTime;
			_sections.RemoveAt(_sections.Count - 1);
			
			// Record the time.
			if (!_profiling.TryGetValue(_currentSection, out var current)) current = 0;
			_profiling[_currentSection] = current + time;
			
			// Reset the current section.
			_currentSection = _sections.Count > 0 ? _sections[_sections.Count - 1] : null;
		}
		
		public void PopPush(string section) {
			if (!IsEnabled()) return;
			
			Pop();
			Push(section);
		}
		
		private static long GetNanos() {
			return DateTime.Now.Ticks * 100;
		}
		
		private static long RemoveAndGet(List<long> list, int index) {
			var value = list[index];
			list.RemoveAt(index);
			return value;
		}
		
		public class Report {
			
			public string Section { get; }
			public string Path { get; }
			public long Time { get; }
			public double TotalPercentage { get; }
			public double SectionPercent { get; }

			public Report(string section, string path, long time, double totalPercentage, double sectionPercent) {
				Section = section;
				Path = path;
				Time = time;
				TotalPercentage = totalPercentage;
				SectionPercent = sectionPercent;
			}
		}
	}
}