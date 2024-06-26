using System;
using System.Collections.Generic;
using IdlePlus.Utilities.Extensions;

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
		public bool Enabled { get; set; }
		
		/// <summary>
		/// If the Profiler is currently running.
		/// </summary>
		private bool _currentlyEnabled;

		public Profiler(bool enabled = false) {
			//_instance = this;
			Enabled = enabled;
			_currentlyEnabled = enabled;
		}

		#region Start/Stop
		
		/// <summary>
		/// Starts the current profiling session.
		/// Must always be called before pushing or popping any sections.
		/// </summary>
		public void Start() {
			_currentlyEnabled = Enabled;
			if (!_currentlyEnabled) return;

			if (_currentSection != null) {
				IdleLog.Warn("Profiler: Tried to start, but it's already running.");
				return;
			}
			
			// "Push" the main section.
			_currentSection = RootSection;
			_sections.Add(_currentSection);
			_timestamps.Add(DateTime.Now.Ticks * 100);
		}
		
		/// <summary>
		/// Stops the current profiling session.
		/// </summary>
		public void Stop() {
			if (!_currentlyEnabled) return;
			
			if (_currentSection != RootSection) {
				IdleLog.Warn($"Profiler: Tried to disable, but not all sections were closed!\n" +
				             $"Current section: {_currentSection}");
				return;
			}
			
			// Stop the main section.
			Pop();
		}
		
		#endregion

		#region Get report
		
		/// <summary>
		/// Get the report for a given section.
		/// This will only return the children of the given section, and not
		/// all of them. If you want the full report, use #GetFullReport().
		/// </summary>
		/// <param name="section">The section to get the report of, or null for the root section.</param>
		/// <param name="includeTopLevel">If the given section should also be included in the list.</param>
		/// <returns>A sorted list of reports for the given section.</returns>
		public List<Report> GetReport(string section = null, bool includeTopLevel = true) {
			if (!_currentlyEnabled) return new List<Report>();
			
			var results = new List<Report>();
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
					var sectionPercentage = time * 100D / targetTime;
					var subSection = key.Substring(section.Length + 1);
					// Add the result
					results.Add(new Report(subSection, key.Substring(RootSection.Length + 1), time, totalPercentage,
						sectionPercentage));
				}
			}
			
			// Add untracked time.
			if (trackedTime < targetTime) {
				var time = targetTime - trackedTime;
				var totalPercentage = time * 100D / totalTime;
				var sectionPercentage = time * 100D / targetTime;
				results.Add(new Report("#untracked", null, time, totalPercentage, sectionPercentage));
			}
			
			// Decay the profiling time.
			_profiling.ReplaceAll((k, v) => (long)(v * 0.99D));
			
			// Sort the results by time then add the current section at the top.
			results.Sort((a, b) => b.Time.CompareTo(a.Time));
			if (includeTopLevel) results.Insert(0, new Report(section, section, targetTime, 
				targetTime * 100D / totalTime, 100));
			return results;
		}

		/// <summary>
		/// Get the full report for all the subsections starting from the given
		/// section.
		/// </summary>
		/// <param name="section">The section to get the full report for, or null for the root section.</param>
		/// <param name="topLevel">If the given section should also be included in the list.</param>
		/// <returns>A list of reports for the given section and all its subsections.</returns>
		public List<Report> GetFullReport(string section = null, bool topLevel = true) {
			var reports = GetReport(section, topLevel);
			
			foreach (var report in reports) {
				if (report.Path == null) continue;
				var includeTopLevel = topLevel;
				report.SubSections = GetFullReport(report.Path, false);
			}

			return reports;
		}
		
		#endregion

		#region Push/Pop
		
		public void Push(string section) {
			if (!_currentlyEnabled) return;
			
			if (_currentSection == null) {
				IdleLog.Warn("Profiler: Tried to push, but no sections are open.");
				return;
			}
			
			// Begin the section.
			_currentSection += $".{section}";
			_sections.Add(_currentSection);
			_timestamps.Add(DateTime.Now.Ticks * 100);
		}
		
		public void Pop() {
			if (!_currentlyEnabled) return;

			if (_currentSection == null) {
				IdleLog.Warn("Profiler: Tried to pop, but no sections are open.");
				return;
			}

			// End the section.
			var endTime = DateTime.Now.Ticks * 100;
			var startTime = _timestamps.RemoveAndGet(_timestamps.Count - 1);
			var time = endTime - startTime;
			_sections.RemoveAt(_sections.Count - 1);
			
			// Record the time.
			if (!_profiling.TryGetValue(_currentSection, out var current)) current = 0;
			_profiling[_currentSection] = current + time;
			
			// Reset the current section.
			_currentSection = _sections.Count > 0 ? _sections[_sections.Count - 1] : null;
		}
		
		public void PopPush(string section) {
			if (!_currentlyEnabled) return;
			
			Pop();
			Push(section);
		}
		
		#endregion
		
		public class Report {
			
			/// <summary>
			/// The name of the section.
			/// </summary>
			public string Section { get; }
			/// <summary>
			/// The path to this section.
			/// </summary>
			public string Path { get; }
			
			/// <summary>
			/// The total time spent profiling this section.
			/// </summary>
			public long Time { get; }
			/// <summary>
			/// The total percentage of the time spent in this section.
			/// </summary>
			public double TotalPercentage { get; }
			/// <summary>
			/// The percentage of the time spent in this section using the
			/// parent section as the total.
			/// </summary>
			public double SectionPercent { get; }

			/// <summary>
			/// The children of this section.
			/// Is only available if the report was created with #GetFullReport().
			/// </summary>
			public List<Report> SubSections { get; set; } = null;

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