using System;
using IdlePlus.API.Unity;
using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace IdlePlus.Unity.Chat {
	
	// <link="decorate:underline">
	// <link="decorate:italic">
	// <link="decorate:click:onTag">
	// <link="decorate:color:ff00ff">
	// <link="decorate:underline&color:ff00ff">

	[RegisterIl2Cpp]
	public class GeneralTextDecorationLink : MonoBehaviour, IMouseEnterHandler, IMouseExitHandler, IMouseMoveHandler, IMouseClickHandler {
		
		private static Camera _camera;

		private TextMeshProUGUI _text;
		private Action<string, MouseEventData> _onClick;
		
		private string _selectedId;
		private string _originalText;
		private string _clickId;

		[InitializeOnce(OnSceneLoad = Scenes.Anything)]
		private static void InitializeOnce() {
			MouseEventManager.Register<GeneralTextDecorationLink>(component => component.Cast<GeneralTextDecorationLink>());
			SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)delegate(Scene scene, LoadSceneMode mode) {
				if (!_camera) _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
			};
		}
		
		[HideFromIl2Cpp]
		public void Setup(TextMeshProUGUI text, Action<string, MouseEventData> onClick = null) {
			this._text = text;
			this._onClick = onClick;

			this._selectedId = null;
			this._originalText = null;
			this._clickId = null;
		}
		
		public void Awake() {
			if (!_camera) _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
		}

		[HideFromIl2Cpp]
		private void ActivateEffect(TMP_LinkInfo linkInfo) {
			var linkId = linkInfo.GetLinkID();
			if (!linkId.StartsWith("decorate:")) return;
			linkId = linkId.Substring("decorate:".Length);
			var parts = linkId.Split('&');
			if (parts.Length == 0) return;
			
			string openTag = "";
			string closingTag = "";
			foreach (var part in parts) {
				var arguments = part.Split(':');
				var command = arguments[0];

				switch (command) {
					case "underline":
						openTag += "<u>";
						closingTag += "</u>";
						break;
					case "italic":
						openTag += "<i>";
						closingTag += "</i>";
						break;
					case "color":
						if (arguments.Length < 2) continue;
						var color = arguments[1];
						openTag += $"<color=#{color}>";
						closingTag += "</color>";
						break;
					case "click":
						if (arguments.Length < 2) continue;
						this._clickId = arguments[1];
						break;
					default:
						continue;
				}
			}

			this._originalText = this._text.text;
			var content = $"{openTag}{linkInfo.GetLinkText()}{closingTag}";
			var start = 2 + linkInfo.GetLinkID().Length + linkInfo.linkIdFirstCharacterIndex;
			var end = linkInfo.linkTextLength + start;

			var pre = this._originalText.Substring(0, start);
			var post = this._originalText.Substring(end);
			var newText = $"{pre}{content}{post}";
			
			this._text.SetText(newText);
		}

		[HideFromIl2Cpp]
		private void DeactivateEffect() {
			if (this._originalText == null) return;
			this._text.SetText(this._originalText);
		}
		
		[HideFromIl2Cpp]
		public void HandleMouseClick(MouseEventData data) {
			if (this._selectedId == null) return;
			if (this._clickId == null) return;
			this._onClick?.Invoke(this._clickId, data);
		}
		
		[HideFromIl2Cpp]
		public void HandleMouseEnter(MouseEventData data) {
			if (!this._text) return;
			this.SearchAndSetup(data);
		}

		[HideFromIl2Cpp]
		public void HandleMouseExit(MouseEventData data) {
			if (!this._text) return;
			if (this._selectedId == null) return;
			this._selectedId = null;
			this.DeactivateEffect();
		}
		
		[HideFromIl2Cpp]
		public void HandleMouseMove(MouseEventData data) {
			if (!this._text) return;
			if (this._selectedId == null) {
				this.SearchAndSetup(data);
				return;
			}
			
			// Make sure we're still hovering over our link, if not, display the
			// new one if we're hovering over one.
			
			var linkInfo = this.TryToGetLink(data);
			string linkId = linkInfo?.GetLinkID();
			if (linkId == null) {
				if (this._selectedId == null) return;
				this._selectedId = null;
				this.DeactivateEffect();
				return;
			}

			// If it's the same link, then don't do anything.
			if (this._selectedId == linkId) return;
			this._selectedId = null;
			this.DeactivateEffect();
			this._text.ForceMeshUpdate();
			
			// Fetch the new link as we've updated the text.
			linkInfo = this.TryToGetLink(data);
			linkId = linkInfo?.GetLinkID();
			if (linkId == null) return;
			
			this._selectedId = linkId;
			this.ActivateEffect(linkInfo);
		}

		[HideFromIl2Cpp]
		private void SearchAndSetup(MouseEventData data) {
			var linkInfo = this.TryToGetLink(data);
			string linkId = linkInfo?.GetLinkID();
			if (linkId == null) {
				if (this._selectedId == null) return;
				this._selectedId = null;
				this.DeactivateEffect();
				return;
			}
			
            
			if (this._selectedId == linkId) return;
			this._selectedId = null;
			this.DeactivateEffect();
			this._text.ForceMeshUpdate();
			
			// Fetch the new link as we've updated the text.
			linkInfo = this.TryToGetLink(data);
			linkId = linkInfo?.GetLinkID();
			if (linkId == null) return;
			
			this._selectedId = linkId;
			this.ActivateEffect(linkInfo);
		}

		[HideFromIl2Cpp]
		private TMP_LinkInfo TryToGetLink(MouseEventData data) {
			int intersectingLink = TMP_TextUtilities.FindIntersectingLink(this._text, data.MousePosition, _camera);
			return intersectingLink <= -1 ? null : this._text.textInfo.linkInfo[intersectingLink];
		}
	}
}