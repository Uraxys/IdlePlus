using System;
using UnityEngine;

namespace IdlePlus.API.Popup {
	public abstract class CustomHardPopup : MonoBehaviour {

		internal PopupKey InternalKey;
		internal InternalPopup InternalPopup;
		
		/// <summary>
		/// If this popup uses the blocking background.
		/// </summary>
		public virtual bool BlockingBackground => false;

		/// <summary>
		/// If this popup can be closed by pressing the ESC keyboard button.
		/// </summary>
		public virtual bool CloseWithEsc => false;

		/// <summary>
		/// If this popup can be closed by pressing on the background. If this
		/// is true, then BlockingBackground is automatically forced on.
		/// </summary>
		public virtual bool CloseWithBackground => false;

		/// <summary>
		/// If this popup should open and close with an ease in and out effect.
		/// Can be zero or negative to disable.
		/// </summary>
		public virtual float Animation => 0.1f;
		
		/// <summary>
		/// Called when the popup is closed.
		/// </summary>
		public virtual void OnClose() {}

		protected void Display() {
			if (this.InternalPopup == null) throw new Exception("Popup isn't registered.");
			this.InternalPopup.Show();
		}

	}
}