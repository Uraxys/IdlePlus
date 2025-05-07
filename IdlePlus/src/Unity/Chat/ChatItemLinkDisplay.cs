using Databases;
using IdlePlus.API.Unity;
using IdlePlus.API.Utility;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace IdlePlus.Unity.Chat {
	
	[RegisterIl2Cpp]
	public class ChatItemLinkDisplay : MonoBehaviour, IMouseEnterHandler, IMouseExitHandler, IMouseMoveHandler {
		
		private static Camera _camera;
		
		private InventoryItemHoverPopup _popup;
		private TextMeshProUGUI _text;
		private string _selected;

		[InitializeOnce(OnSceneLoad = Scenes.Anything)]
		private static void InitializeOnce() {
			MouseEventManager.Register<ChatItemLinkDisplay>(component => component.Cast<ChatItemLinkDisplay>());
			SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)delegate(Scene scene, LoadSceneMode mode) {
				if (!_camera) _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
			};
		}
		
		public void Setup(TextMeshProUGUI text) {
			var popup = PopupManager.Instance.GetPopup(HardPopup.InventoryItemHoverPopup);
			this._popup = popup.Cast<InventoryItemHoverPopup>();
			this._text = text;
		}
		
		public void Awake() {
			if (!_camera) _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
		}

		public void HandleMouseEnter(MouseEventData data) {
			if (!this._text) return;
			this.SearchAndSetup(data);
		}

		public void HandleMouseExit(MouseEventData data) {
			if (!this._text) return;
			if (this._selected == null) return;
			
			this._selected = null;
			this._popup.Hide();
		}

		public void HandleMouseMove(MouseEventData data) {
			if (!this._text) return;
			if (this._selected == null) {
				this.SearchAndSetup(data);
				return;
			}
			
			// Make sure we're still hovering over our link, if not, display the
			// new one if we're hovering over one.
			
			string linkId = this.TryToGetLink(data);
			if (linkId == null) {
				if (this._selected == null) return;
				this._selected = null;
				this._popup.Hide();
				return;
			}

			// If it's the same link, the just update the position, if not,
			// display the other one instead.
			Vector3 worldPos;
			if (this._selected == linkId) {
				worldPos = _camera.ScreenToWorldPoint(data.MousePosition);
				this._popup.transform.position = worldPos.Add(0, 0.1f, 0).SetZ(0);
				return;
			}
			
			this._selected = linkId;
			var itemId = int.Parse(linkId.Substring(5));
			var item = ItemDatabase.ItemList[itemId];
			
			PopupManager.Instance.SetupHardPopup(HardPopup.InventoryItemHoverPopup, false, false);
			worldPos = _camera.ScreenToWorldPoint(data.MousePosition);
			this._popup.Setup(item, worldPos.Add(0, 0.1f, 0));
			this._popup.Show();
		}

		private void SearchAndSetup(MouseEventData data) {
			string linkId = this.TryToGetLink(data);
			if (linkId == null) {
				if (this._selected == null) return;
				this._selected = null;
				this._popup.Hide();
				return;
			}
			
            
			if (this._selected == linkId || !linkId.StartsWith("ITEM:")) return;
			this._selected = linkId;
			
			var itemId = int.Parse(linkId.Substring(5));
			var item = ItemDatabase.ItemList[itemId];
			
			PopupManager.Instance.SetupHardPopup(HardPopup.InventoryItemHoverPopup, false, false);
			var worldPos = _camera.ScreenToWorldPoint(data.MousePosition);
			this._popup.Setup(item, worldPos.Add(0, 0.1f, 0));
			this._popup.Show();
		}

		private string TryToGetLink(MouseEventData data) {
			int intersectingLink = TMP_TextUtilities.FindIntersectingLink(this._text, data.MousePosition, _camera);
			if (intersectingLink <= -1) return null;
			TMP_LinkInfo linkInfo = this._text.textInfo.linkInfo[intersectingLink];
			return linkInfo.GetLinkID();
		}
	}
}