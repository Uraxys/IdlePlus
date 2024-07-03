using System;

namespace IdlePlus.Utilities.Attributes {
	public class Initialize : Attribute {
		public int Priority = 0;
		public string OnSceneLoad = null;
	}
}