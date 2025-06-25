using System;
using Combat;
using Databases;
using HarmonyLib;
using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using Player;

namespace IdlePlus.Patches.Combat {
	
	// A fix for the memory leak in CombatSoloPlayerPanelEntry.
	// Commented out as I'm hoping we'll get an official fix soon.
	
	/*[HarmonyPatch(typeof(CombatSoloPlayerPanelEntry))]
	public class CombatSoloPlayerPanelEntryPatch {
		
		private static Il2CppSystem.Action<Item> _equipAction;
		private static Il2CppSystem.Action<Item> _unequipAction;

		[Initialize(OnSceneLoad = Scenes.Game)]
		private static void Initialize() {
			_equipAction = null;
			_unequipAction = null;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(CombatSoloPlayerPanelEntry.Setup))]
		private static void PostfixSetup(CombatSoloPlayerPanelEntry __instance, out Tuple<Il2CppSystem.Action<Item>, Il2CppSystem.Action<Item>> __state) {
			var equipment = PlayerData.Instance.Equipment;
			IdleLog.Info("Copying OnEquipItem and OnUnequipItem");
			__state = new Tuple<Il2CppSystem.Action<Item>, Il2CppSystem.Action<Item>>(equipment.OnEquipItem, equipment.OnUnequipItem);
		}

		[HarmonyPostfix]
		[HarmonyPatch(nameof(CombatSoloPlayerPanelEntry.Setup))]
		public static void PostfixSetup(CombatSoloPlayerPanelEntry __instance, Tuple<Il2CppSystem.Action<Item>, Il2CppSystem.Action<Item>> __state) {
			// Revert the changes made in the original method.
			var onEquipItem = __state.Item1;
			var onUnequipItem = __state.Item2;
			PlayerData.Instance.Equipment.OnEquipItem = onEquipItem;
			PlayerData.Instance.Equipment.OnUnequipItem = onUnequipItem;
			
			// Unsubscribe the original actions.
			if (_unequipAction != null) {
				IdleLog.Info("Unsubscribing from OnEquipItem and OnUnequipItem");
				PlayerData.Instance.Equipment.OnEquipItem -= _equipAction;
				PlayerData.Instance.Equipment.OnUnequipItem -= _unequipAction;
			}

			// Subscribe to the new actions.
			var onEquip = (Il2CppSystem.Action<Item>)delegate(Item item) {
				IdleLog.Info("OnEquipItem");
				__instance.OnEquipmentChanged(item);
			};
			var onUnequip = (Il2CppSystem.Action<Item>)delegate(Item item) {
				IdleLog.Info("OnUnequipItem");
				__instance.OnEquipmentChanged(item);
			};
			IdleLog.Info("Subscribing to OnEquipItem and OnUnequipItem");
			PlayerData.Instance.Equipment.OnEquipItem += onEquip;
			PlayerData.Instance.Equipment.OnUnequipItem += onUnequip;
			_equipAction = onEquip;
			_unequipAction = onUnequip;
		}
	}*/
}