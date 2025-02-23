using Popups;

namespace IdlePlus.API.Popup {
	
	/// <summary>
	/// An ID that is obtained after registering a CustomHardPopup, which can
	/// then be used to display the popup.
	/// </summary>
	public class PopupKey {
		
		public readonly int Key;
		public HardPopup Type => (HardPopup) Key;

		public PopupKey(int key) {
			Key = key;
		}
	}
}