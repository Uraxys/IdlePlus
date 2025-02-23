using System;
using System.Collections.Generic;
using HarmonyLib;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Popups;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.API.Popup {
	
	[HarmonyPatch]
	public static class CustomPopupManager {

		private const int StartingID = 10_000;

		private static GameObject _hardPopupContainer;
		private static bool _frozen;
		private static Action _onRegisterPopups;
		
		private static int _nextId;
		private static Dictionary<CustomHardPopup, PopupKey> _registeredPopups = new Dictionary<CustomHardPopup, PopupKey>();

		// TODO: Refactor, I don't like the way this is done.
		public static void OnRegister(Action action) {
			_onRegisterPopups += action;
		}

		/// <summary>
		/// Register a new CustomHardPopup that can be displayed using the
		/// returned PopupKey.
		/// </summary>
		/// <param name="popup">The popup to register.</param>
		/// <returns>A PopupKey that is linked to the CustomHardPopup.</returns>
		/// <exception cref="Exception">If it's too late to register popups, or
		/// if it's already registered.</exception>
		public static PopupKey Register(CustomHardPopup popup) {
			if (_frozen) throw new Exception("Too late to register CustomHardPopup, registration should happen before scene load.");
			if (_registeredPopups.ContainsKey(popup)) throw new Exception("CustomHardPopup already registered.");
			
			var key = new PopupKey(StartingID + _nextId++);
			_registeredPopups.Add(popup, key);
			return key;
		}

		/// <summary>
		/// Set up the CustomHardPopup linked to the given PopupKey, hiding and
		/// moving the background if specified.
		/// </summary>
		/// <param name="key">The key for the CustomHardPopup we want to set up.</param>
		/// <param name="hideActive">If any currently active popups should be closed.</param>
		/// <param name="moveBackground">If the background should be moved behind
		/// this popup.</param>
		/// <typeparam name="T">The CustomHardPopup that was just setup and ready
		/// to be displayed.</typeparam>
		/// <returns></returns>
		public static T Setup<T>(PopupKey key, bool hideActive = false, bool moveBackground = true)
			where T : CustomHardPopup {
			var popup = PopupManager.Instance.SetupHardPopup(key.Type, hideActive, moveBackground).Cast<InternalPopup>();
			if (popup == null) throw new Exception("Popup is null");
			return (T) popup.CustomPopup;
		}
		
		// Helpers

		// TODO: Refactor, I don't like the way this is done.
		public static GameObject CreateTemplatePopup(bool withBackground = false) {
			if (_frozen) throw new Exception("Too late to create empty template popups.");
			var obj = GameObjects.NewRect("template_popup");
			obj.SetActive(false);

			if (withBackground) {
				obj.With<UniformModifier, ProceduralImage>().radius = 10;
			}
			
			return obj;
		}

		#region Internal
		
		private static void EnsureInitialized() {
			if (_hardPopupContainer != null) return;
			_hardPopupContainer = GameObjects.FindByPath("PopupManager/Canvas/HardPopups");
		}

		private static void Freeze() {
			EnsureInitialized();
			_frozen = true;

			// TODO: OnRegister might throw an exception, breaking every other popup in the registry.
			foreach (var pair in _registeredPopups) {
				var popup = pair.Key;
				var key = pair.Value;
				
				popup.transform.SetParent(_hardPopupContainer.transform);
				popup.transform.SetSiblingIndex(_hardPopupContainer.transform.childCount - 2);
				popup.transform.localPosition = Vec3.Zero;
				popup.transform.name = $"idleplus:custom_popup:{key.Key}";
				
				var internalPopup = popup.gameObject.With<InternalPopup>();
				internalPopup.Initialize(popup, key);
				popup.OnRegister();
			}
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(PopupManager), nameof(PopupManager.Awake))]
		private static void PrefixAwake(PopupManager __instance) {
			_onRegisterPopups.Invoke();
			Freeze();
		}
		
		#endregion
	}
}