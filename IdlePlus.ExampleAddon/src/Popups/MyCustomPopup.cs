using IdlePlus.API.Popup;

namespace IdlePlus.ExampleAddon.Popups {
	public class MyCustomPopup : CustomHardPopup {
		
		// Popup settings.
		
		public override bool CloseWithEsc => true;
		public override bool CloseWithBackground => true;

		// Method to call when we want to display the popup.
		public void Setup() {
			base.Display();
		}
	}
}