using System;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Popups;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.Settings.Unity {
	
	[RegisterIl2Cpp(typeof(ISettingsPopupTab))]
	public class SettingsPopupIdlePlusTab : MonoBehaviour {

		public static SettingsPopupIdlePlusTab Instance { get; private set; }
		private static readonly Color BackgroundColor = new Color(0, 0, 0, 0.25f);
		
		private GameObject _scroll;
		private GameObject _container;
		
		private bool _initialized;
		
		public SettingsTab Tab => SettingsTab.Chat;
		public bool RequireConnection => false;

		public void Initialize() {
			if (_initialized) return;
			_initialized = true;

			try {
				// Copy the scroll view from the chat tab.
				_scroll = Instantiate(transform.parent.Find("ChatSection/ContentScrollView"), transform, false).gameObject;
				_scroll.name = "ScrollView";
			
				// Set up the size and position of the scroll.
				var scrollRect = _scroll.GetComponent<RectTransform>();
				scrollRect.localPosition = Vec3.Zero;
				scrollRect.sizeDelta = Vec2.Vec(850, 360);
			
				// Add a background to the scroll.
				_scroll.DestroyComponent<Image>(true);
				_scroll.With<ProceduralImage>().color = BackgroundColor;
				_scroll.With<UniformModifier>().radius = 5;
			
				// Fix the scroll wheel.
				var wheelObj = _scroll.FindNonNull("Scrollbar Vertical");
				wheelObj.Use<RectTransform>().sizeDelta = Vec2.Vec(10, 0);
				wheelObj.Use<UniformModifier>().radius = 0;
				wheelObj.FindAndUse<UniformModifier>("Sliding Area/Handle").radius = 2;

				// Remove the entries from the scroll.
				_container = _scroll.FindAndUse("Viewport/ChatChannelConfigurationContainer", o => o.name = "Container");
				_container.DestroyComponent<ProceduralImage, UniformModifier>();
				_container.DestroyChildren();
				_container.Use<VerticalLayoutGroup>(group => {
					group.padding.left = 0;
					group.childAlignment = TextAnchor.UpperCenter;
				});
			
				// Create the sections.
				foreach (var category in ModSettings.Categories) {
					var section = SettingsSection.GetPrefab();
					section.SetParent(_container, false, 1000);
					section.Use<SettingsSection>().Initialize(category);
				}
			} catch (Exception e) {
				IdleLog.Error("Failed to initialize Idle Plus settings tab.", e);
			}
		}

		public void Awake() {
			Instance = this;
		}

		public void Setup() {
			Initialize();
		}
	}
}