using System;
using ButtonAnimations;
using Buttons;
using HarmonyLib;
using IdlePlus.Settings.Unity;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using IdlePlus.Utilities.Helpers;
using Popups;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace IdlePlus.Patches {

	[HarmonyPatch]
	public static class SettingsPopupPatch {

		private const string SettingsPopupPath = "PopupManager/Canvas/HardPopups/SettingsPopup2";
		private const string PlusButtonPath = "NavigationButtons/IdlePlusTab";
		private const string PlusPanelPath = "Panels/IdlePlusSection";
		
		private static bool _initialized;
		private static bool _genericPanelInitialized;

		[InitializeOnce(OnSceneLoad = "*")]
		private static void InitializeOnce() {
			if (_initialized) return;
			_initialized = true;
			
			var settingsPopupObj = GameObjects.FindByCachedPath(SettingsPopupPath);

			// Insert out own version text under the current build text.
			var buildTextObj = settingsPopupObj.Find("BuildText");
			var idlePlusTextObj = GameObjects.Instantiate(buildTextObj, settingsPopupObj, false);
			idlePlusTextObj.name = "IdlePlusText";
			idlePlusTextObj.transform.localPosition = new Vector3(490F, 255F, 0F);
			idlePlusTextObj.GetComponent<TextMeshProUGUI>().text = $"Idle Plus: v{IdlePlus.ModVersion}";

			// Add a new navigation button after the general button.
			var generalTabObj = settingsPopupObj.Find("NavigationButtons/GeneralTab");
			var idlePlusTabObj = Object.Instantiate(generalTabObj, generalTabObj.transform.parent, false);
			idlePlusTabObj.transform.SetSiblingIndex(1);
			idlePlusTabObj.name = "IdlePlusTab";
			ModLocalization.SetModdedKey(idlePlusTabObj.Find("Text"), "idle_plus");

			// Create a new panel after general section.
			var panelsObj = settingsPopupObj.Find("Panels");
			var idlePlusPanelObj = GameObjects.NewRect<SettingsPopupIdlePlusTab>("IdlePlusSection");
			idlePlusPanelObj.SetParent(panelsObj, false, 1);
			idlePlusPanelObj.SetActive(false);
		}

		[HarmonyPrefix]
		[HarmonyPatch(typeof(SettingsPopup), nameof(SettingsPopup.Setup), new Type[]{})]
		private static void PrefixSetup(SettingsPopup __instance) {
			//InitializeOnce();
		}
		
		[HarmonyPrefix]
		[HarmonyPatch(typeof(GenericPanelManager), nameof(GenericPanelManager.Awake))]
		private static void PrefixAwake(GenericPanelManager __instance) {
			// Make sure we're only running on the settings popup, and not any other popup.
			if (_genericPanelInitialized) return;
			if (__instance.transform.parent == null || __instance.transform.parent.name != "SettingsPopup2") return;
			_genericPanelInitialized = true;
			
			//InitializeOnce();
			
			var settingsPopupObj = GameObjects.FindByPathNonNull(SettingsPopupPath);
			var plusButton = settingsPopupObj.Find(PlusButtonPath).GetComponent<GenericHoverEdgeButton>();
			var plusPanel = settingsPopupObj.Find(PlusPanelPath);
			
			__instance._buttons = CollectionHelper.RefArrayInsert(1, plusButton, __instance._buttons);
			__instance._panels = CollectionHelper.RefArrayInsert(1, plusPanel, __instance._panels);
		}
	}
}