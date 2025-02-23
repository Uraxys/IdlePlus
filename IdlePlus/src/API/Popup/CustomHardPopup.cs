using System;
using IdlePlus.Attributes;
using Il2CppInterop.Runtime.Attributes;
using Popups;
using UnityEngine;

namespace IdlePlus.API.Popup {
	public abstract class CustomHardPopup : MonoBehaviour {
		
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
		/// Called when this CustomHardPopup is being registered and moved to
		/// the correct position in the hierarchy.
		/// </summary>
		public virtual void OnRegister() {}
		
		/// <summary>
		/// Called when the popup is closed.
		/// </summary>
		public virtual void OnClose() {}

		public void Display() {
			this.InternalPopup.Show();
		}

	}
	
	[RegisterIl2Cpp]
	internal class InternalPopup : BaseHardPopup {

		internal CustomHardPopup CustomPopup;

		public InternalPopup(IntPtr pointer) : base(pointer) { }

		[HideFromIl2Cpp]
		internal void Initialize(CustomHardPopup popup, PopupKey key) {
			this.CustomPopup = popup;
			this.CustomPopup.InternalPopup = this;
			// BaseHardPopup settings.
			_popup = (HardPopup) key.Key;
			_useBlockingBackground = this.CustomPopup.BlockingBackground || this.CustomPopup.CloseWithBackground;
			
			CanCloseWithEsc = this.CustomPopup.CloseWithEsc;
			CloseFromBackgroundClick = this.CustomPopup.CloseWithBackground;
			CanCloseAtAll = this.CustomPopup.CloseWithEsc || this.CustomPopup.CloseWithBackground;

			_animationTime = this.CustomPopup.Animation;
			_doHideAnimation = this.CustomPopup.Animation > 0f;
			OnClose += (Action) delegate { this.CustomPopup.OnClose(); };
		}
	}
}