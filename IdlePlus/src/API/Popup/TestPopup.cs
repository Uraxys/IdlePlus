using System;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Popups;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.API.Popup {
	
	[RegisterIl2Cpp]
	public class TestPopup : BaseHardPopup {

		public const int TEST_POPUP = 10_000;
		
		internal static void InitializeOnce() {
			var container = GameObjects.FindByPath("PopupManager/Canvas/HardPopups");
			var popup = GameObjects.NewRect<UniformModifier, ProceduralImage>("idle_plus:TestPopup", container.gameObject);
			popup.transform.SetSiblingIndex(0);
			popup.Use<RectTransform>().sizeDelta = Vec2.Vec(300, 100);
			popup.Use<UniformModifier>().radius = 10;
			popup.With<TestPopup>();
			popup.SetActive(false);
		}
		
		
		
		public TestPopup(IntPtr pointer) : base(pointer) {
			_popup = (HardPopup) TEST_POPUP;
			_useBlockingBackground = true;
			CanCloseWithEsc = true;
			CloseFromBackgroundClick = true;
		}

		public void Awake() {
			IdleLog.Warn("TestPopup awake!");
		}

		public void Setup() {
			base.Show();
		}

		/*public override void Show() {
			IdleLog.Warn("Show()");
			//base.Show();
		}*/

		/*public override void Hide() {
			IdleLog.Warn("Hide()");
			//base.Hide();
		}

		public override void Close() {
			IdleLog.Warn("Close()");
			//base.Close();
		}*/
	}
}