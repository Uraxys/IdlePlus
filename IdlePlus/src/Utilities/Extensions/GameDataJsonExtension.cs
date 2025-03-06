using System;
using Databases;
using Equipment;
using GameContent;
using Items;
using Newtonsoft.Json.Linq;
using Player;
using Skilling;

namespace IdlePlus.Utilities.Extensions {
	public static class GameDataJsonExtension {

		private static JObject CreateIfNotNull<T>(T check, Action<T, JObject> action) {
			if (check == null) return null;
			JObject entry = new JObject();
			action.Invoke(check, entry);
			return entry;
		}
		
		public static JObject ToJson(this Item item) {
			JObject entry = new JObject();

			entry.Set("id", item.ItemId);
			entry.Set("name", item.Name);
			entry.Set("description", item.DescriptionLocKey);
			
			entry.Set("localized", new JObject().Do(o => {
				o.Set("name", item.GetLocalizedNameWithoutEnchantments());
				o.Set("description", item.GetLocalizedDescription());
			}));

			entry.Set("discontinued", item.Discontinued);
			entry.Set("item_counterpart", item.ItemCounterpartId);
			entry.Set("base_value", item.BaseValue);
			entry.Set("associated_skill", GetSkillOrNull(item.AssociatedSkill));
			entry.Set("sellable", !item.CanNotBeSoldToGameShop); // Inverted
			entry.Set("tradeable", !item.CanNotBeTraded); // Inverted
			entry.Set("tradeable_with_clan", item.TradeableWithClan);
			entry.Set("flip_sprite", item.FlipSprite);
			entry.Set("tool", item.IsTool);
			entry.Set("mastery_cape_type", GetEnumOrNull(MasteryCapeType.None, item.MasteryCapeType));
			entry.Set("equipment_slot", GetEnumOrNull(EquipmentSlot.None, item.EquipmentSlot));
			
			entry.Set("melee_strength_bonus", item.StrengthBonus);
			entry.Set("melee_accuracy_bonus", item.AccuracyBonus);
			entry.Set("melee_defence_bonus", item.DefenceBonus);
			entry.Set("archery_strength_bonus", item.ArcheryStrengthBonus);
			entry.Set("archery_accuracy_bonus", item.ArcheryAccuracyBonus);
			entry.Set("archery_defence_bonus", item.ArcheryDefenceBonus);
			entry.Set("magic_strength_bonus", item.MagicStrengthBonus);
			entry.Set("magic_accuracy_bonus", item.MagicAccuracyBonus);
			entry.Set("magic_defence_bonus", item.MagicDefenceBonus);
			entry.Set("attack_interval", item.AttackInterval);
			entry.Set("two_handed", item.TwoHanded);
			
			entry.Set("weapon_monster_weakness", CreateIfNotNull(item.WeaponMonsterWeakness, (obj, e) => {
				e.Set("boost", obj.DamageBoost);
				e.Set("monsters", obj.WeakMonsters);
			}));
			
			entry.Set("weapon_raid_monster_weakness", CreateIfNotNull(item.WeaponRaidMonsterWeakness, (obj, e) => {
				e.Set("boost", obj.DamageBoost);
				e.Set("monsters", obj.WeakMonsters);
			}));
			
			entry.Set("monster_defensive_boost", CreateIfNotNull(item.WeaponRaidMonsterWeakness, (obj, e) => {
				e.Set("boost", obj.DamageBoost);
				e.Set("monsters", obj.WeakMonsters);
			}));
			
			entry.Set("weapon_style", GetEnumOrNull(AttackStyle.None, item.Style));
			entry.Set("weapon_tier", GetEnumOrNull(WeaponType.None, item.WeaponType));
			entry.Set("weapon_class", GetEnumOrNull(WeaponClassType.None, item.WeaponClass));
			
			entry.Set("activatable_type", GetEnumOrNull(ItemActivatableType.None, item.ActivatableType));
			entry.Set("potion_type", GetEnumOrNull(PotionType.None, item.PotionType));
			entry.Set("potion_effect_duration_seconds", item.PotionEffectDurationSeconds);
			entry.Set("health_applied_on_consume", item.HealthAppliedOnConsume);

			entry.Set("skill_boost", CreateSkillBoost(item.SkillBoost));
			
			entry.Set("trigger_effects", item.TriggerEffects == null ? null : new JArray().Do(array => {
				foreach (var effect in item.TriggerEffects) {
					array.Add(new JObject(
						new JProperty("type", GetEnumOrNull(ItemEffectsGlobal.ItemEffectTriggerType.None, effect.TriggerType)),
						new JProperty("chance", effect.TriggerChancePercentage),
						new JProperty("power", effect.TriggerPower)
					));
				}
			}));
			
			entry.Set("inventory_consumable_boost", CreateSkillBoost(item.InventoryConsumableBoost));
			entry.Set("level_requirement", CreateIfNotNull(item.LevelRequirement, (req, o) => {
				o.Set("skill", GetSkillOrNull(req.Skill));
				o.Set("level", req.Level);
			}));
			
			entry.Set("cosmetic_scroll_effect", GetEnumOrNull(WeaponEffectType.None, item.CosmeticScrollEffect));
			entry.Set("enchanted_version_item_id", item.EnchantedVersionItemId);
			entry.Set("enchantment_boost", item.EnchantmentBoost);
			entry.Set("enchanting_skill_type", GetSkillOrNull(item.EnchantingSkillType));
			entry.Set("scroll_type", GetEnumOrNull(EnchantmentScrollType.None, item.ScrollType));
			entry.Set("proc_chance", item.ProcChance);

			return entry;
			
			string GetEnumOrNull<T>(T def, T current) =>
				def.Equals(current) ? null : current.ToString().ToLower();

			string GetSkillOrNull(Skill current) =>
				GetEnumOrNull(Skill.None, current);

			JObject CreateSkillBoost(SkillBoost obj) =>
				CreateIfNotNull(obj, (boost, o) => {
					o.Set("skill", boost.Skill == Skill.None ? null : boost.Skill.ToString().ToLower());
					o.Set("boost_percentage", boost.BoostPercentage);
				});
		}
		
	}
}