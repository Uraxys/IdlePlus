using Popups;

namespace IdlePlus.Utilities {
	public static class PopupHelper {

		public static GenericPopup Generic(bool hideActive = false, bool moveBackground = true) {
			var popup = PopupManager.Instance.SetupHardPopup(HardPopup.GenericPopup, hideActive, moveBackground);
			// ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
			return new GenericPopup(popup.Pointer);
		}
		
		public static GenericPopup GenericLocalized(string key, bool hideActive = false, bool moveBackground = true) {
			var popup = PopupManager.Instance.SetupHardPopup(HardPopup.GenericPopup, hideActive, moveBackground);
			// ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
			var genericPopup = new GenericPopup(popup.Pointer);
			genericPopup.SetupAndLocalize(key);
			return genericPopup;
		}
		
		public static GenericPopup GenericText(string text, bool hideActive = false, bool moveBackground = true) {
			var popup = PopupManager.Instance.SetupHardPopup(HardPopup.GenericPopup, hideActive, moveBackground);
			// ReSharper disable once Unity.IncorrectMonoBehaviourInstantiation
			var genericPopup = new GenericPopup(popup.Pointer);
			genericPopup.Setup(text);
			return genericPopup;
		}
	}
}