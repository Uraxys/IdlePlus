using Equipment;
using HarmonyLib;
using IdlePlus.Settings;
using IdlePlus.Unity;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePlus.Patches.Equipment {
	
	[HarmonyPatch(typeof(UIEquipmentBonuses))]
	public class UIEquipmentBonusesPatch {

		private static bool _initialized;
		private static StatsEntry _meleeStats;
		private static StatsEntry _archeryStats;
		private static StatsEntry _magicStats;
		
		[Initialize(OnSceneLoad = Scenes.MainMenu)]
		private static void Initialize() {
			_initialized = false;
			_meleeStats = null;
			_archeryStats = null;
			_magicStats = null;
		}

		[HarmonyPrefix]
		[HarmonyPatch(nameof(UIEquipmentBonuses.OnEnable))]
		private static void PrefixOnEnable(UIEquipmentBonuses __instance) {
			if (!ModSettings.UI.EnhancedInventoryStats.Value) return;
			if (_initialized) return;
			_initialized = true;

			// Get the parents of the entries and disable the text component.
			var meleeEntry = __instance._meleeBonusesText.transform.parent;
			var archeryEntry = __instance._archeryBonusesText.transform.parent;
			var magicEntry = __instance._magicBonusesText.transform.parent;
			__instance._meleeBonusesText.enabled = false;
			__instance._archeryBonusesText.enabled = false;
			__instance._magicBonusesText.enabled = false;
			
			// Start creating our custom entries.
			for (byte i = 0; i < 3; i++) {
				var parent = i == 0 ? meleeEntry : i == 1 ? archeryEntry : magicEntry;
				var stats = GameObjects.NewRect<HorizontalLayoutGroup>($"Stats{i}", parent.gameObject);
				var rect = stats.Use<RectTransform>();
				var layout = stats.Use<HorizontalLayoutGroup>();
				rect.SetAnchors(0.122f, 0, 1, 1);
				rect.sizeDelta = Vec2.One;
				layout.childControlWidth = true;
				layout.childControlHeight = true;
				layout.childForceExpandWidth = true;
				layout.childForceExpandHeight = true;

				var objects = new GameObject[3];

				for (var j = 0; j < 3; j++) {
					var entry = GameObjects.NewRect($"Stat{j}", stats.gameObject);
					var textObj = GameObjects.NewRect<MatchParentSize, TextMeshProUGUI>("Text", entry);
					var textRect = textObj.Use<RectTransform>();
					textRect.SetAnchors(0, 0.5f, 0, 0.5f);
					textRect.pivot = Vec2.Vec(0, 0.5f);
					textRect.sizeDelta = Vec2.Vec(120, 40);
					objects[j] = textObj;

					var text = textObj.Use<TextMeshProUGUI>();
					text.alignment = TextAlignmentOptions.Left;
					text.text = "Key: Value";
				}
				
				switch (i) {
					case 0: _meleeStats = new StatsEntry(i, objects[0], objects[1], objects[2]); break;
					case 1: _archeryStats = new StatsEntry(i, objects[0], objects[1], objects[2]); break;
					default: _magicStats = new StatsEntry(i, objects[0], objects[1], objects[2]); break;
				}
			}
		}
		
		[HarmonyPostfix]
		[HarmonyPatch(nameof(UIEquipmentBonuses.OnUpdateBonuses))]
		private static void PostfixOnUpdateBonuses() {
			if (!ModSettings.UI.EnhancedInventoryStats.Value) return;
			_meleeStats?.Update();
			_archeryStats?.Update();
			_magicStats?.Update();
		}
		
		private class StatsEntry {
			private readonly byte _index;
			private readonly GameObject _strength;
			private readonly GameObject _accuracy;
			private readonly GameObject _defence;

			public StatsEntry(byte index, GameObject strength, GameObject accuracy, GameObject defence) {
				_index = index;
				_strength = strength;
				_accuracy = accuracy;
				_defence = defence;
			}

			public void Update() {
				var strength = _strength.Use<TextMeshProUGUI>();
				var accuracy = _accuracy.Use<TextMeshProUGUI>();
				var defence = _defence.Use<TextMeshProUGUI>();
				
				int str;
				int acc;
				int def;

				var bonuses = PlayerData.Instance.EquipmentBonuses;
				switch (_index) {
					case 0:
						str = bonuses.TotalStrengthBonus;
						acc = bonuses.TotalAccuracyBonus;
						def = bonuses.TotalDefenceBonus;
						break;
					case 1:
						str = bonuses.TotalArcheryStrengthBonus;
						acc = bonuses.TotalArcheryAccuracyBonus;
						def = bonuses.TotalArcheryDefenceBonus;
						break;
					default:
						str = bonuses.TotalMagicStrengthBonus;
						acc = bonuses.TotalMagicAccuracyBonus;
						def = bonuses.TotalMagicDefenceBonus;
						break;
				}
				
				var strClr = "fff";
				var accClr = "fff";
				var defClr = "fff";
				if (str < 0) strClr = "fbb";
				if (acc < 0) accClr = "fbb";
				if (def < 0) defClr = "fbb";
				
				strength.text = $"<color=#ddd>Strength:</color> <b><color=#{strClr}>{str}</color></b>";
				accuracy.text = $"<color=#ddd>Accuracy:</color> <b><color=#{accClr}>{acc}</color></b>";
				defence.text = $"<color=#ddd>Defence:</color> <b><color=#{defClr}>{def}</color></b>";
			}
		}
	}
}