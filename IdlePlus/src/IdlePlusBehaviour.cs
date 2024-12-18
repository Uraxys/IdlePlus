using System;
using IdlePlus.Utilities.Attributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace IdlePlus {
	
	[RegisterIl2Cpp]
	public class IdlePlusBehaviour : MonoBehaviour {
		
		public static IdlePlusBehaviour Instance;

		public IdlePlusBehaviour(IntPtr pointer) : base(pointer) { }

		internal static void Create() {
			if (Instance != null) return;
			var obj = new GameObject("IdlePlus");
			DontDestroyOnLoad(obj);
			obj.hideFlags = HideFlags.HideAndDontSave;
			Instance = obj.AddComponent<IdlePlusBehaviour>();
			
			SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>) Instance.OnSceneLoaded;
		}

		public void Update() {
			IdlePlus.Update();
			
			// Used for testing.
			/*if (Input.GetKeyDown(KeyCode.Space)) {
		
			}*/
		}

		public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			IdlePlus.OnSceneLoaded(scene, mode);
		}
	}
}