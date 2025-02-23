using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.API.Popup.Popups {
	
	[RegisterIl2Cpp]
	public class TestPopupTwo : CustomHardPopup {

		public static PopupKey Popup { get; private set; }
		
		public static void CreateTestPopupTwo() {
			var obj = CustomPopupManager.CreateTemplatePopup(true);
			var popup = obj.With<TestPopupTwo>();
			obj.With<ProceduralImage>().color = IdleColors.PopupBlue;

			var text = GameObjects.NewRect<TextMeshProUGUI>("text", obj).Use<TextMeshProUGUI>();
			text.text = "Helloooo!!!";
			text.color = Color.white;

			Popup = CustomPopupManager.Register(popup);
		}
		
		public override bool CloseWithEsc => true;
		public override bool CloseWithBackground => true;

		public void Setup() {
			Display();
		}
	}
}