using Databases;
using Equipment;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Player;
using Scripts.Shared.Data.Content.Skills;
using Skilling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePlus.Unity.Items {
	
	[RegisterIl2Cpp]
	public class EquipmentStatsInfo : MonoBehaviour {

		private static readonly Color GrayColor = new Color(0.9f, 0.9f, 0.9f, 1f);
		
		private GameObject _container;
		private StatEntry _meleeEntry;
		private StatEntry _rangeEntry;
		private StatEntry _magicEntry;

		private bool _initialized;

		public void Awake() {
			Initialize();
		}

		public bool Setup(Item item) {
			if (item.EquipmentSlot == EquipmentSlot.None) return false;
			Initialize();

			int meleeStrength;
			int meleeAccuracy;
			int meleeDefence;

			int rangeStrength;
			int rangeAccuracy;
			int rangeDefence;

			int magicStrength;
			int magicAccuracy;
			int magicDefence;
			
			var equipped = PlayerData.Instance.Equipment._equippedItems[item.EquipmentSlot];
			if (equipped != null) {
				meleeStrength = CalculateDifference(item.StrengthBonus, equipped.StrengthBonus);
				meleeAccuracy = CalculateDifference(item.AccuracyBonus, equipped.AccuracyBonus);
				meleeDefence = CalculateDifference(item.DefenceBonus, equipped.DefenceBonus);

				rangeStrength = CalculateDifference(item.ArcheryStrengthBonus, equipped.ArcheryStrengthBonus);
				rangeAccuracy = CalculateDifference(item.ArcheryAccuracyBonus, equipped.ArcheryAccuracyBonus);
				rangeDefence = CalculateDifference(item.ArcheryDefenceBonus, equipped.ArcheryDefenceBonus);
				
				magicStrength = CalculateDifference(item.MagicStrengthBonus, equipped.MagicStrengthBonus);
				magicAccuracy = CalculateDifference(item.MagicAccuracyBonus, equipped.MagicAccuracyBonus);
				magicDefence = CalculateDifference(item.MagicDefenceBonus, equipped.MagicDefenceBonus);
			} else {
				meleeStrength = item.StrengthBonus;
				meleeAccuracy = item.AccuracyBonus;
				meleeDefence = item.DefenceBonus;

				rangeStrength = item.ArcheryStrengthBonus;
				rangeAccuracy = item.ArcheryAccuracyBonus;
				rangeDefence = item.ArcheryDefenceBonus;

				magicStrength = item.MagicStrengthBonus;
				magicAccuracy = item.MagicAccuracyBonus;
				magicDefence = item.MagicDefenceBonus;
			}
			
			var hasMelee = meleeStrength != 0 || meleeAccuracy != 0 || meleeDefence != 0;
			var hasRange = rangeStrength != 0 || rangeAccuracy != 0 || rangeDefence != 0;
			var hasMagic = magicStrength != 0 || magicAccuracy != 0 || magicDefence != 0;
			
			if (hasMelee) _meleeEntry.Setup(meleeStrength, meleeAccuracy, meleeDefence);
			else _meleeEntry.Enabled(false);
			
			if (hasRange) _rangeEntry.Setup(rangeStrength, rangeAccuracy, rangeDefence);
			else _rangeEntry.Enabled(false);
			
			if (hasMagic) _magicEntry.Setup(magicStrength, magicAccuracy, magicDefence);
			else _magicEntry.Enabled(false);

			return hasMelee || hasRange || hasMagic;
		}

		private int CalculateDifference(int selected, int equipped) {
			if (selected == equipped) return 0;
			if (selected > equipped) return selected - equipped;
			
			// We're going to lose stats, find out how much.
			var diff = equipped - selected;
			return diff * -1;
		}

		private void Initialize() {
			if (_initialized) return;
			_initialized = true;
			
			_container = gameObject;
			
			// Title text.
			var title = GameObjects.NewRect("Title", _container);
			title.With<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			title.With<TextMeshProUGUI>(text => {
				text.text = "When Equipped";
				text.fontSize = 16;
				text.fontSizeMax = 16;
				text.color = GrayColor;
			});

			// Container for the stats.
			var statsContainer = GameObjects.NewRect("StatsContainer", _container);
			statsContainer.With<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			statsContainer.With<VerticalLayoutGroup>().SetChildStates(true, false, true).spacing = 2;

			// The stat lines.
			_meleeEntry = new StatEntry("Melee", Skill.Rigour, statsContainer);
			_meleeEntry.Setup(128, 10, 44);
			_rangeEntry = new StatEntry("Range", Skill.Archery, statsContainer);
			_rangeEntry.Setup(-27, -55, 0);
			_magicEntry = new StatEntry("Magic", Skill.Magic, statsContainer);
			_magicEntry.Setup(0, 0, -10);
		}

		private class StatEntry {
			private const string PositiveColor = "bfb";
			private const string NeutralColor = "bbb";
			private const string NegativeColor = "fbb";

			private readonly GameObject _container;
			
			private readonly TextMeshProUGUI _strengthText;
			private readonly TextMeshProUGUI _accuracyText;
			private readonly TextMeshProUGUI _defenceText;

			public StatEntry(string name, Skill skill, GameObject parent) {
				_container = GameObjects.NewRect(name, parent);

				_container.With<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
				_container.With<HorizontalLayoutGroup>(group => {
					group.DisableChildStates();
					group.childAlignment = TextAnchor.MiddleLeft;
				});

				var icon = GameObjects.NewRect("Icon", _container);
				icon.Use<RectTransform>().sizeDelta = Vec2.Vec(22);
				icon.With<Image>().sprite = IdleAssets.GetSkillSprite(skill);

				var statsContainer = GameObjects.NewRect("Stats", _container);
				statsContainer.With<ContentSizeFitter>().SetFit(ContentSizeFitter.FitMode.PreferredSize);
				statsContainer.With<HorizontalLayoutGroup>(group => {
					//group.spacing = 2f;
					group.DisableChildStates();
				});

				var strength = GameObjects.NewRect("Strength", statsContainer);
				var accuracy = GameObjects.NewRect("Accuracy", statsContainer);
				var defence = GameObjects.NewRect("Defence", statsContainer);

				strength.Use<RectTransform>().sizeDelta = Vec2.Vec(70f, 19.72f);
				_strengthText = strength.With<TextMeshProUGUI>(text => {
					text.text = "+100 STR";
					text.fontSize = 16;
					text.fontSizeMax = 16;
					text.color = GrayColor;
					text.alignment = TextAlignmentOptions.TopRight;
				});
			
				accuracy.Use<RectTransform>().sizeDelta = Vec2.Vec(70f, 19.72f);
				_accuracyText = accuracy.With<TextMeshProUGUI>(text => {
					text.text = "+100 ACC";
					text.fontSize = 16;
					text.fontSizeMax = 16;
					text.color = GrayColor;
					text.alignment = TextAlignmentOptions.TopRight;
				});

				defence.Use<RectTransform>().sizeDelta = Vec2.Vec(70f, 19.72f);
				_defenceText = defence.With<TextMeshProUGUI>(text => {
					text.text = "+100 DEF";
					text.fontSize = 16;
					text.fontSizeMax = 16;
					text.color = GrayColor;
					text.alignment = TextAlignmentOptions.TopRight;
				});
			}

			public void Setup(int strength, int accuracy, int defence) {
				Enabled(true);
				var strClr = strength == 0 ? NeutralColor : strength < 0 ? NegativeColor : PositiveColor;
				var accClr = accuracy == 0 ? NeutralColor : accuracy < 0 ? NegativeColor : PositiveColor;
				var defClr = defence == 0 ? NeutralColor : defence < 0 ? NegativeColor : PositiveColor;

				var strNum = strength > 0 ? $"+{strength}" : strength.ToString();
				var accNum = accuracy > 0 ? $"+{accuracy}" : accuracy.ToString();
				var defNum = defence > 0 ? $"+{defence}" : defence.ToString();

				var strText = strength == 0 ? $"<color=#{NeutralColor}>STR</color>" : "<color=#ffffff>STR</color>";
				var accText = accuracy == 0 ? $"<color=#{NeutralColor}>ACC</color>" : "<color=#ffffff>ACC</color>";
				var defText = defence == 0 ? $"<color=#{NeutralColor}>DEF</color>" : "<color=#ffffff>DEF</color>";

				_strengthText.text = $"<color=#{strClr}>{strNum}</color> {strText}";
				_accuracyText.text = $"<color=#{accClr}>{accNum}</color> {accText}";
				_defenceText.text = $"<color=#{defClr}>{defNum}</color> {defText}";
			}

			public void Enabled(bool value) {
				this._container.SetActive(value);
			}
		}
		
	}
}