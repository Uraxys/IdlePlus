using System;
using System.Collections.Generic;
using HarmonyLib;
using IdlePlus.API.Utility;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Popups;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;
using Object = UnityEngine.Object;

namespace IdlePlus.API.Popup {
	
	using CreatePopupAction = Action<GameObject>;
	
	[HarmonyPatch]
	public static class CustomPopupManager {

		private const int StartingID = 10_000;
		private static GameObject _hardPopupContainer;
		
		private static bool _frozen;
		private static int _nextId;
		
		private static readonly HashSet<NamespacedKey> PreRegisteredKeys = new HashSet<NamespacedKey>();
		private static readonly Dictionary<PopupKey, CreatePopupAction> PreRegisteredPopups = new Dictionary<PopupKey, CreatePopupAction>();
		private static readonly Dictionary<PopupKey, CustomHardPopup> RegisteredPopups = new Dictionary<PopupKey, CustomHardPopup>();

		/// <summary><para>
		/// Registers a new custom popup with a unique key and a deferred creation action.
		/// </para><para>
		/// Instead of creating the popup immediately, the provided action will be invoked later
		/// when the game is ready. It will receive a <see cref="GameObject"/>, which should be
		/// used as the root object for this popup. This object must have a <see cref="CustomHardPopup"/> 
		/// component attached.
		/// </para><para>
		/// If the <see cref="GameObject"/> does not contain a <see cref="CustomHardPopup"/>
		/// after the action has been executed, the popup will be considered invalid, and the
		/// object will be deleted and removed from the registry.
		/// </para><para>
		/// The returned <see cref="PopupKey"/> serves as a unique identifier for the registered
		/// popup, ensuring easy access without relying on workarounds.
		/// </para></summary>
		/// <param name="key">A unique key in the <see cref="NamespacedKey"/> format, used to
		/// identifying the popup.</param>
		/// <param name="createAction">
		/// An <see cref="Action{GameObject}"/> that will be executed when the game is ready.
		/// The provided <see cref="GameObject"/> should be used as the root popup object.
		/// </param>
		/// <returns>A <see cref="PopupKey"/> that can be used to reference the registered popup.</returns>
		/// <exception cref="ArgumentException">Thrown if the given key is already registered.</exception>
		/// <exception cref="Exception">Thrown if registration occurs too late in the application lifecycle.</exception>
		/// <exception cref="AssertException">Thrown if either <paramref name="key"/> or <paramref name="createAction"/>
		/// is null.</exception>
		public static PopupKey Register(string key, CreatePopupAction createAction) {
			Asserts.NotNull(key, "key", "Key cannot be null");
			return Register(NamespacedKey.Of(key), createAction);
		}
		
		/// <summary><para>
		/// Registers a new custom popup with a unique key and a deferred creation action.
		/// </para><para>
		/// Instead of creating the popup immediately, the provided action will be invoked later
		/// when the game is ready. It will receive a <see cref="GameObject"/>, which should be
		/// used as the root object for this popup. This object must have a <see cref="CustomHardPopup"/> 
		/// component attached.
		/// </para><para>
		/// If the <see cref="GameObject"/> does not contain a <see cref="CustomHardPopup"/>
		/// after the action has been executed, the popup will be considered invalid, and the
		/// object will be deleted and removed from the registry.
		/// </para><para>
		/// The returned <see cref="PopupKey"/> serves as a unique identifier for the registered
		/// popup, ensuring easy access without relying on workarounds.
		/// </para></summary>
		/// <param name="key">A unique <see cref="NamespacedKey"/> identifying the popup.</param>
		/// <param name="createAction">
		/// An <see cref="Action{GameObject}"/> that will be executed when the game is ready.
		/// The provided <see cref="GameObject"/> should be used as the root popup object.
		/// </param>
		/// <returns>A <see cref="PopupKey"/> that can be used to reference the registered popup.</returns>
		/// <exception cref="ArgumentException">Thrown if the given key is already registered.</exception>
		/// <exception cref="Exception">Thrown if registration occurs too late in the application lifecycle.</exception>
		/// <exception cref="AssertException">Thrown if either <paramref name="key"/> or <paramref name="createAction"/>
		/// is null.</exception>
		public static PopupKey Register(NamespacedKey key, CreatePopupAction createAction) {
			Asserts.NotNull(key, "key", "Key cannot be null");
			Asserts.NotNull(createAction, "createAction", "Create action cannot be null");
			
			if (_frozen) throw new Exception("Too late to register new CustomHardPopups.");
			if (PreRegisteredKeys.Contains(key)) throw new ArgumentException("Key is already registered.", nameof(key));

			PopupKey popupKey = new PopupKey(key, StartingID + _nextId++);
			PreRegisteredKeys.Add(key);
			PreRegisteredPopups.Add(popupKey, createAction);

			return popupKey;
		}

		/// <summary>
		/// Prepares a <see cref="CustomHardPopup"/> associated with the specified <paramref name="key"/>.
		/// This method initializes the popup, optionally hides currently active popups, and can move
		/// the background behind it.
		/// </summary>
		/// <typeparam name="T">
		/// The type of <see cref="CustomHardPopup"/> that will be set up and returned.
		/// </typeparam>
		/// <param name="key">
		/// The <see cref="PopupKey"/> identifying the popup to set up.
		/// </param>
		/// <param name="hideActive">
		/// Whether any currently active popups should be closed before setting up this one.
		/// Defaults to <c>false</c>.
		/// </param>
		/// <param name="moveBackground">
		/// Whether the background should be moved behind this popup.
		/// Defaults to <c>true</c>.
		/// </param>
		/// <returns>
		/// The initialized popup of type <typeparamref name="T"/>, ready to be displayed.
		/// </returns>
		/// <exception cref="AssertException">
		/// Thrown if <paramref name="key"/> is null.
		/// </exception>
		/// <exception cref="Exception">
		/// Thrown if the setup is attempted before the registry phase is complete,
		/// or if the popup is not registered or is null.
		/// </exception>
		public static T Setup<T>(PopupKey key, bool hideActive = false, bool moveBackground = true)
			where T : CustomHardPopup {
			Asserts.NotNull(key, "key", "Key cannot be null");
			if (!_frozen) throw new Exception("Can't setup popup while in the registry phase.");
			if (!RegisteredPopups.TryGetValue(key, out var customPopup)) throw new Exception("Popup isn't registered.");
			var popup = PopupManager.Instance.SetupHardPopup(key.Type, hideActive, moveBackground).Cast<InternalPopup>();
			if (popup == null) throw new Exception("Popup is null");
			return (T) customPopup;
		}
		
		#region Patch & Internal Methods

		private static void Freeze() {
			if (_frozen) return;
			_hardPopupContainer = GameObjects.FindByPath("PopupManager/Canvas/HardPopups");
			_frozen = true;

			foreach (var entry in PreRegisteredPopups) {
				PopupKey key = entry.Key;
				CreatePopupAction action = entry.Value;

				// Create the object we're using for the popup and set the right
				// index & position.
				var obj = GameObjects.NewRect($"CustomHardPopup ({key.NamespacedKey})", _hardPopupContainer);
				obj.transform.SetSiblingIndex(_hardPopupContainer.transform.childCount - 2);
				obj.transform.localPosition = Vec3.Zero;

				// Try to create the popup.
				try {
					action.Invoke(obj);
				} catch (Exception e) {
					IdleLog.Error($"An exception occurred while creating popup for {key.NamespacedKey}.", e);
					Object.Destroy(obj);
					continue;
				}
				
				// Make sure the game object wasn't destroyed, unity overrides == and returns
				// true if the object was destroyed.
				if (obj == null) {
					IdleLog.Warn($"Root GameObject for popup {key.NamespacedKey} was destroyed after creation.");
					continue;
				}
				
				// Make sure the user added their own CustomHardPopup component.
				CustomHardPopup customPopup = obj.GetComponent<CustomHardPopup>();
				if (customPopup == null) {
					IdleLog.Error($"Root GameObject for popup {key.NamespacedKey} is missing a CustomHardPopup component, removing.");
					Object.Destroy(obj);
					continue;
				}

				// Finalize the popup.
				InternalPopup internalPopup = obj.AddComponent<InternalPopup>();
				internalPopup.Initialize(customPopup, key);
				customPopup.InternalKey = key;
				customPopup.InternalPopup = internalPopup;
				obj.name = key.NamespacedKey.Identifier;
				RegisteredPopups.Add(key, customPopup);
			}
			
			// Clean up pre-register objects.
			PreRegisteredKeys.Clear();
			PreRegisteredPopups.Clear();
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(PopupManager), nameof(PopupManager.Awake))]
		private static void PrefixAwake() {
			Freeze();
		}
		
		#endregion
	}
}