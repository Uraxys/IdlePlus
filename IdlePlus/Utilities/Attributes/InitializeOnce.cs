using System;

namespace IdlePlus.Utilities.Attributes {
	[AttributeUsage(AttributeTargets.Method)]
	public class InitializeOnce : Attribute {
		public int Priority = 0;
		public string OnSceneLoad = null;
	}
}