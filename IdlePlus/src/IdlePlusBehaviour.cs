using System;
using System.Diagnostics;
using IdlePlus.Utilities;
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
			
			// I hate Unity, I really do, the pain of some things, in my other engines I have
			// implemented these easy methods, just change the window title, maybe the icon, or
			// just get the handle, but no, Unity doesn't have those.
			//
			// NOTE: Only works on windows, but that shouldn't be a problem as BepInEx only
			// supports windows when modding IL2CPP games.
			if (IdlePlus.WindowHandle == IntPtr.Zero) {
				var title = Process.GetCurrentProcess().MainWindowTitle;
				var handle = Process.GetCurrentProcess().MainWindowHandle;

				if (!title.StartsWith("BepInEx")) {
					IdlePlus.WindowHandle = handle;
					IdleLog.Info($"Found main Unity window handle! title: '{title}', handle: 0x{handle.ToInt32():X8}");
				}
			}
			
			// Used for testing.
			/*if (Input.GetKeyDown(KeyCode.Space)) {
		
			}*/
		}

		public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			IdlePlus.OnSceneLoaded(scene, mode);
		}
	}
}