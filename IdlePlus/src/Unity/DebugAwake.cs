using System;
using System.Diagnostics;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using UnityEngine;

namespace IdlePlus.Unity {
	
	[RegisterIl2Cpp]
	public class DebugAwake : MonoBehaviour {

		public static int StartFrame = 0;
		public static Stopwatch Watch;
		
		public void Awake() {
			var timeTaken = Watch.ElapsedMilliseconds;

			var _name = transform.name;
			if (transform.parent != null) _name = $"{transform.parent.name}/{_name}";

			if (transform.parent != null && transform.parent.parent != null) {
				var current = transform.parent.parent;
				while (current != null) {
					_name = $"{current.name}/{_name}";
					current = current.parent;
				}
			}
			
			var frame = Time.frameCount;
			IdleLog.Info($"{_name} took {timeTaken}ms ({frame})");
		}
	}
}