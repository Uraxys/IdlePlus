using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using IdlePlus.Utilities.Helpers;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.Settings.Unity {
	
	[RegisterIl2Cpp]
	public class SettingsSection : MonoBehaviour {

		private static readonly Color SectionColor = new Color(0.046f, 0.1575f, 0.2075f, 1f);
		private static GameObject _prefab;

		private SettingCategory _category;
		private TextMeshProUGUI _title;
		private GameObject _container;
		private bool _initialized;

		[HideFromIl2Cpp]
		public void Initialize(SettingCategory category) {
			if (_initialized) return;
			_initialized = true;

			_category = category;
			_title = gameObject.Find("Title").Use<TextMeshProUGUI>();
			_container = gameObject.Find("Container");
			
			// TODO: Localization?
			_title.SetText($"<b>{category.Title}</b>");

			// Add the settings.
			foreach (var setting in _category.Settings) {
				var obj = setting.GetPrefab();
				obj.SetParent(_container);
				setting.Initialize(obj);
			}
		}

		#region Prefab
		
		public static GameObject GetPrefab() {
			if (_prefab == null) CreatePrefab();
			return Instantiate(_prefab);
		}
		
		private static void CreatePrefab() {
			var section = GameObjects.NewRect<SettingsSection>("SettingsSectionPrefab", UiHelper.PrefabContainer);

			// Main object
			section.With<RectTransform>().sizeDelta = Vec2.Vec(930, 350);
			section.With<ProceduralImage>().color = SectionColor;
			section.With<UniformModifier>().radius = 5;
			section.With<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			var layout = section.With<VerticalLayoutGroup>();
			layout.childAlignment = TextAnchor.UpperCenter;
			layout.childControlHeight = false;
			layout.childControlWidth = false;
			layout.childScaleHeight = false;
			layout.childScaleWidth = false;
			layout.childForceExpandHeight = false;
			layout.childForceExpandWidth = false;
			layout.padding.top = 5;
			
			// Title
			var title = GameObjects.NewRect<TextMeshProUGUI>("Title", section).Use<TextMeshProUGUI>();
			//title.SetText("<b>Market Value</b>");
			title.fontSize = 25;
			title.alignment = TextAlignmentOptions.Top;
			title.transform.localPosition = Vec3.Vec(0, 160, 0);
			var rect = title.gameObject.Use<RectTransform>();
			rect.sizeDelta = new Vector2(750, 40);
			rect.SetAnchors(0.5f, 1f, 0.5f, 1f);
			
			// Create the container for the settings.
			var container = GameObjects.NewRect("Container", section);
			container.Use<RectTransform>().sizeDelta = Vec2.Vec(850, 100);
			container.With<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
			var grid = container.With<GridLayoutGroup>();
			grid.cellSize = Vec2.Vec(350, 100);
			grid.childAlignment = TextAnchor.UpperCenter;
			grid.spacing = Vec2.Vec(25, 10);
			grid.padding.bottom = 40;
			
			// Set the prefab.
			_prefab = section;
		}
		
		#endregion
	}
}