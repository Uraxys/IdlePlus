using System;
using IdlePlus.Attributes;
using Il2CppInterop.Runtime.Attributes;
using Popups;

namespace IdlePlus.API.Popup {
	
	[RegisterIl2Cpp]
	internal class InternalPopup : BaseHardPopup {

		public InternalPopup(IntPtr pointer) : base(pointer) { }

		[HideFromIl2Cpp]
		internal void Initialize(CustomHardPopup popup, PopupKey key) {
			// BaseHardPopup settings.
			_popup = (HardPopup) key.InternalId;
			_useBlockingBackground = popup.BlockingBackground || popup.CloseWithBackground;
			
			CanCloseWithEsc = popup.CloseWithEsc;
			CloseFromBackgroundClick = popup.CloseWithBackground;
			CanCloseAtAll = popup.CloseWithEsc || popup.CloseWithBackground;

			_animationTime = popup.Animation;
			_doHideAnimation = popup.Animation > 0f;
			OnClose += (Action) popup.OnClose;
		}
	}
}