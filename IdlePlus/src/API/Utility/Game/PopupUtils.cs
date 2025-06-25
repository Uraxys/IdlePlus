using System;
using JetBrains.Annotations;
using Popups;

namespace IdlePlus.API.Utility.Game {
	public static class PopupUtils {

		public static void ShowLoadingPopup(bool showBackground = true) {
			var popup = PopupManager.Instance.SetupHardPopup<LoadingPopup>();
			var text = LocalizationManager.GetLocalizedValue("loading");
			popup.Initialize(text, showBackground);
		}
		
		public static void ShowSoftPopup(string title, string message, float delay = 0f, float time = 3.0f,
			[CanBeNull] Action onClick = null) {
			// We need to create a nullable float... because if not we'll crash... fun.
			var stay = new Il2CppSystem.Nullable<float>(time);
			Il2CppSystem.Action action = onClick;

			SoftPopupManager.Instance.ShowSoftMessagePopup(
				title: title,
				message: message,
				delaySeconds: delay,
				customAnimationTime: stay,
				onClickAction: action
			);
		}
	}
}