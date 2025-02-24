using BepInEx;
using BepInEx.Unity.IL2CPP;
using IdlePlus.API.Popup;
using IdlePlus.API.Utility;
using IdlePlus.ExampleAddon.Popups;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Il2CppInterop.Runtime.Injection;
using TMPro;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.ExampleAddon {
	
	// Basic BepInEx plugin information.
	[BepInPlugin(ModGuid, ModName, ModVersion)]
	// Add Idle Plus as hard a dependency.
	[BepInDependency("dev.uraxys.idleplus")]
	public class ExampleAddon : BasePlugin {

		private const string
			ModName = "Example Mod using Idle Plus",
			ModGuid = "dev.uraxys.example_mod_for_idleplus",
			ModVersion = "1.0";
		
		// The PopupKey, which is used when we want to set up and display
		// this popup.
		internal static PopupKey MyCustomPopup { get; private set; }
		
		// Load is called when our BepInEx mod loads up.
		public override void Load() {
			// Register IL2CPP types.
			// TODO: Make an easier way for addons to register IL2CPP types.
			ClassInjector.RegisterTypeInIl2Cpp(typeof(MyCustomPopup));
			
			// Register our custom popups.
			// The popup creation could be moved to another method or file, which
			// is recommended if you're working with large or complex popups.
			MyCustomPopup = CustomPopupManager.Register("ExampleAddon:MyCustomPopup", obj => {
				obj.Use<RectTransform>().sizeDelta = Vec2.Vec(400, 200);
				obj.With<MyCustomPopup>();
				obj.With<ProceduralImage, UniformModifier>().color = IdleColors.PopupBlue;

				var text = GameObjects.NewRect<TextMeshProUGUI>("text", obj).Use<TextMeshProUGUI>();
				text.text = "My Awesome Popup";
				text.color = Color.white;
			});
		}
	}
}