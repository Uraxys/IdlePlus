using System.Collections.Generic;
using Databases;
using Equipment;
using GameContent;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace IdlePlus.Unity.Items {
	
	[RegisterIl2Cpp]
	public class ScrollInfo : MonoBehaviour {

		private static bool _initialized;
		private static readonly List<Item> AvailableJewellery = new List<Item>();

		/// <summary>
		/// The time between each tick in seconds.
		/// </summary>
		private static readonly long TimeBetweenTick = 1;
		
		private readonly List<Sprite> _earrings = new List<Sprite>();
		private readonly List<Sprite> _amulets = new List<Sprite>();
		private readonly List<Sprite> _rings = new List<Sprite>();
		private readonly List<Sprite> _bracelets = new List<Sprite>();
		
		private Image _earringsImage;
		private Image _amuletsImage;
		private Image _ringsImage;
		private Image _braceletsImage;

		private long _ticks;
		private float _nextTick;

		public void Awake() {
			_earringsImage = transform.GetChild(1).Use<Image>();
			_amuletsImage = transform.GetChild(2).Use<Image>();
			_ringsImage = transform.GetChild(3).Use<Image>();
			_braceletsImage = transform.GetChild(4).Use<Image>();
		}

		public void Update() {
			if (Time.time < _nextTick) return;
			_nextTick = Time.time + TimeBetweenTick;
			_ticks++;
			
			UpdateSprites();
		}

		public bool Setup(Item item) {
			Initialize();
			
			var skill = item.EnchantingSkillType;
			var scrollType = item.ScrollType;
			if (scrollType == EnchantmentScrollType.None) {
				IdleLog.Warn($"Couldn't get scroll type for item: {item.Name}");
				return false;
			}
			
			_earrings.Clear();
			_amulets.Clear();
			_rings.Clear();
			_bracelets.Clear();
			_nextTick = Time.time;
			
			var enchantments = PlayerData.Instance.Enchantments.GetEnchantments();
			foreach (var entry in AvailableJewellery) {
				if (entry.UsableEnchantmentScroll != scrollType) continue;
				var enchantedId = entry.EnchantedVersionItemId;
				
				// If we don't have the item in the enchantments list, then it's not enchanted,
				// meaning we can enchant it with this scroll.
				if (!enchantments.ContainsKey(enchantedId)) {
					AddItemSprite(entry);
					continue;
				}
				
				var skills = enchantments[enchantedId];
				
				// Check if we already have this skill enchanted.
				if (skills.Contains(skill)) continue;
				
				// We can enchant this item with this scroll.
				AddItemSprite(entry);
			}

			// Check if we have any items to display, if not, then we don't enable the indicator.
			if (_earrings.Count == 0 && _amulets.Count == 0 && _rings.Count == 0 && _bracelets.Count == 0) return false;
			
			UpdateSprites(true);
			return true;
		}

		private void UpdateSprites(bool setup = false) {
			if (setup) {
				_earringsImage.gameObject.SetActive(_earrings.Count > 0);
				_amuletsImage.gameObject.SetActive(_amulets.Count > 0);
				_ringsImage.gameObject.SetActive(_rings.Count > 0);
				_braceletsImage.gameObject.SetActive(_bracelets.Count > 0);
			}

			if (_earrings.Count > 0) _earringsImage.sprite = _earrings[(int) _ticks % _earrings.Count];
			if (_amulets.Count > 0) _amuletsImage.sprite = _amulets[(int) _ticks % _amulets.Count];
			if (_rings.Count > 0) _ringsImage.sprite = _rings[(int) _ticks % _rings.Count];
			if (_bracelets.Count > 0) _braceletsImage.sprite = _bracelets[(int) _ticks % _bracelets.Count];
		}

		private void AddItemSprite(Item item) {
			List<Sprite> sprites;
			switch (item.EquipmentSlot) {
				case EquipmentSlot.Earrings: sprites = _earrings; break;
				case EquipmentSlot.Amulet: sprites = _amulets; break;
				case EquipmentSlot.Jewellery: sprites = _rings; break;
				case EquipmentSlot.Bracelet: sprites = _bracelets; break;
				default: IdleLog.Warn($"Unknown equipment slot for item while getting sprites: {item.Name}"); return;
			}
			sprites.Add(item.LoadSpriteFromResources());
		}

		/// <summary>
		/// Find the items we're interested in.
		/// </summary>
		private static void Initialize() {
			if (_initialized) return;
			_initialized = true;
			
			foreach (var entry in ItemDatabase.ItemList) {
				var item = entry.value;
				// We want every item that can be enchanted.
				if (item.EnchantedVersionItemId == 0) continue;
				if (item.CosmeticScrollEffect != WeaponEffectType.None) continue;
				if (item.UsableEnchantmentScroll == EnchantmentScrollType.None) continue;
				if (item.EquipmentSlot != EquipmentSlot.Earrings &&
				    item.EquipmentSlot != EquipmentSlot.Amulet &&
				    item.EquipmentSlot != EquipmentSlot.Jewellery &&
				    item.EquipmentSlot != EquipmentSlot.Bracelet) continue;
				AvailableJewellery.Add(item);
			}
			
			// Sort by GetItemSortId.
			AvailableJewellery.Sort((a, b) => GetItemSortId(a).CompareTo(GetItemSortId(b)));
		}

		/// <summary>
		/// Get the sort id for the given item.
		/// A bit of a hacky solution to get the item colors to line up.
		/// </summary>
		private static int GetItemSortId(Item item) {
			if (item.Name.Contains("sorcerer") || item.Name.Contains("arcane")) return 1;
			if (item.Name.Contains("marksman") || item.Name.Contains("precision")) return 2;
			if (item.Name.Contains("brute") || item.Name.Contains("berserker")) return 3;
			return 100;
		}
	}
}