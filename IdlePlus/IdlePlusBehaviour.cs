using System;
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
			/*if (Input.GetKeyDown(KeyCode.Space)) {
		
			}*/
		}
	}
}