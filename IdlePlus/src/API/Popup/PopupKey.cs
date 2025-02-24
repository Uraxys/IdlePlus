using IdlePlus.API.Utility;
using Popups;

namespace IdlePlus.API.Popup {
	
	/// <summary>
	/// Represents a unique identifier for a <see cref="CustomHardPopup"/>,
	/// which can be used to easily identify the popup without relying on
	/// workarounds.
	/// </summary>
	public class PopupKey {

		/// <summary>
		/// The <see cref="NamespacedKey"/> of this <see cref="PopupKey"/>.
		/// </summary>
		public readonly NamespacedKey NamespacedKey;
		
		/// <summary>
		/// The internal number id of this <see cref="PopupKey"/>.
		/// </summary>
		public readonly int InternalId;
		
		/// <summary>
		/// The <see cref="HardPopup"/> type matching the internal number id.
		/// </summary>
		public HardPopup Type => (HardPopup) InternalId;

		internal PopupKey(NamespacedKey key, int internalId) {
			this.NamespacedKey = key;
			this.InternalId = internalId;
		}
	}
}