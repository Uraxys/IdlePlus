using System;
using System.Collections.Generic;
using IdlePlus.Utilities;
using Il2CppInterop.Runtime.Injection;
using UnityEngine;

namespace IdlePlus {
	
	public class IdlePlusBehaviour : MonoBehaviour {
		
		public static IdlePlusBehaviour Instance;

		public IdlePlusBehaviour(IntPtr pointer) : base(pointer) { }

		internal static void Create() {
			if (Instance != null) return;
			// Inject the type into Il2Cpp.
			ClassInjector.RegisterTypeInIl2Cpp<IdlePlusBehaviour>();
			// Create the game object.
			var obj = new GameObject("IdlePlus");
			DontDestroyOnLoad(obj);
			obj.hideFlags = HideFlags.HideAndDontSave;
			Instance = obj.AddComponent<IdlePlusBehaviour>();
		}

		public void Update() {
			IdlePlus.Update();
			
			// Used for testing.
			if (Input.GetKeyDown(KeyCode.Space)) {
				if (!IdlePlus._profiler.Enabled) return;
				IdleLog.Info("Showing report for toplevel sections");
				List<Profiler.Report> report = IdlePlus._profiler.GetReport();
				foreach (var entry in report) {
					// time is in nanoseconds
					var milliseconds = entry.Time / 1_000_000D;
					IdleLog.Info($"{entry.Section}: section {entry.SectionPercent:0.00}%, " +
					             $"total {entry.TotalPercentage:0.00}%, time {milliseconds:0.00}ms");
				}
			}

			if (Input.GetKeyDown(KeyCode.B)) {
				if (!IdlePlus._profiler.Enabled) return;
				IdleLog.Info("Showing full report");
				LogAllSections(null, 0);
			}

			if (Input.GetKeyDown(KeyCode.P)) {
				var profilerEnabled = IdlePlus._profiler.Enabled;
				IdlePlus._profiler.Enabled = !profilerEnabled;
				IdleLog.Info($"Profiler is now {(!profilerEnabled ? "disabled" : "enabled")}");
			}
		}

		private void LogAllSections(string section, int depth) {
			var reports = IdlePlus._profiler.GetReport(section, false);
			foreach (var entry in reports) {
				var text = new string(' ', depth * 2) + $"L {entry.Section}: " +
				           $"section {entry.SectionPercent:0.00}%, total {entry.TotalPercentage:0.00}%, " +
				           $"time {entry.Time / 1_000_000D:0.00}ms";
				IdleLog.Info(text);
				if (entry.Path != null) LogAllSections(entry.Path, depth + 1);
			}
		}
	}
}