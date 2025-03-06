using System;
using System.Collections.Generic;
using System.Diagnostics;
using IdlePlus.API.Event;
using IdlePlus.API.Unity;
using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using IdlePlus.Unity;
using IdlePlus.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace IdlePlus {
	
	[RegisterIl2Cpp]
	public class IdlePlusBehaviour : MonoBehaviour {
		
		public static IdlePlusBehaviour Instance;
		private static bool _stopSearchingForWindow = false;

		public IdlePlusBehaviour(IntPtr pointer) : base(pointer) { }

		internal static void Create() {
			if (Instance != null) return;
			var obj = new GameObject("IdlePlus");
			DontDestroyOnLoad(obj);
			obj.hideFlags = HideFlags.HideAndDontSave;
			Instance = obj.AddComponent<IdlePlusBehaviour>();
			
			SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>) Instance.OnSceneLoaded;
		}

		private static readonly HashSet<int> StartingObjects = new HashSet<int>();

		public void Update() {
			// Tick tasks.
			IdleTasks.Tick();
			MouseEventManager.Tick();
			
			// I hate Unity, I really do, the pain of some things, in my other engines I have
			// implemented these easy methods, just change the window title, maybe the icon, or
			// just get the handle, but no, Unity doesn't have those.
			//
			// NOTE: Only works on windows, but that shouldn't be a problem as BepInEx only
			// supports windows when modding IL2CPP games.
			if (IdlePlus.WindowHandle == IntPtr.Zero && !_stopSearchingForWindow) {
				var title = Process.GetCurrentProcess().MainWindowTitle;
				var handle = Process.GetCurrentProcess().MainWindowHandle;

				if (title.StartsWith("Idle Clans") && !title.Contains("BepInEx")) {
					IdlePlus.WindowHandle = handle;
				}
			}
			
			// Used for testing.
			/*if (Input.GetKeyDown(KeyCode.F11)) {
				var chatbox = PopupManager.Instance.SetupHardPopup<ChatboxPopup>();
				chatbox.Show();
			}*/
			
			/*if (Input.GetKeyDown(KeyCode.F12)) {
				var popup = CustomPopupManager.Setup<TestPopupTwo>(TestPopupTwo.PopupKey);
				popup.Setup();
			}*/

#pragma warning disable CS0162 // Unreachable code detected
			if (IdlePlus.PerformanceTest) {
				if (Time.frameCount < DebugAwake.StartFrame + 25) {
					var time = DebugAwake.Watch?.ElapsedMilliseconds ?? -1;
					IdleLog.Info($"Frame: {Time.frameCount} / {time}ms");
				}
			}
#pragma warning restore CS0162 // Unreachable code detected
		}

		public void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
			GameObjects.InitializeCache();
			InitializeAttributeHandler.Initialize(scene.name);
			InitializeOnceAttributeHandler.InitializeOnce(scene.name);

			// Safeguard in case we don't find the window handle, then
			// we don't want to keep checking every frame.
			if (scene.name == Scenes.Game && !_stopSearchingForWindow) {
				_stopSearchingForWindow = true;
			}

			// Call the scene loaded event.
			switch (scene.name) {
				case Scenes.MainMenu: Events.Scene.OnLobby.Call(); break;
				case Scenes.Game: 
					Events.Scene.OnGame.Call();

					// TODO: Remove later.
					
					if (!IdlePlus.PerformanceTest) break;
#pragma warning disable CS0162 // Unreachable code detected
					if (DebugAwake.Watch != null) DebugAwake.Watch.Stop();
					DebugAwake.Watch = new Stopwatch();
					DebugAwake.Watch.Start();
					
					Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();
					foreach (var entry in objs) {
						
						var _name = entry.name;
						if (entry.parent != null) _name = $"{entry.parent.name}/{_name}";
						if (_name != "ChatButton/ChatNotification") continue;
						
						if (entry.gameObject.GetComponent<DebugAwake>() != null) continue;
						entry.gameObject.AddComponent<DebugAwake>();
					}
					
					break;
#pragma warning restore CS0162 // Unreachable code detected
			}
		}
	}
}