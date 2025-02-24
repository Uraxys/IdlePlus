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
		
		// The "key" that is used to reference this popup.
		public static PopupKey PopupKey { get; set; }
		
		// Method called when the popup should be created, happens before the 
		// PopupManager#Awake() method is called.
		public static void Create(GameObject obj) {
			// obj is a newly created GameObject, which is placed inside
			// the hard popups "container".
			
			// Create the look.
			obj.Use<RectTransform>().sizeDelta = Vec2.Vec(400, 200);
			obj.With<TestPopupTwo>();
			obj.With<ProceduralImage, UniformModifier>().color = IdleColors.PopupBlue;
			// Add some cool text.
			var text = GameObjects.NewRect<TextMeshProUGUI>("text", obj).Use<TextMeshProUGUI>();
			text.text = "Custom Popup";
			text.color = Color.white;
		}
		
		// Popup settings.
		
		public override bool CloseWithEsc => true;
		public override bool CloseWithBackground => true;

		// Method to call when we want to display the popup.
		public void Setup() {
			base.Display();
		}
	}
}