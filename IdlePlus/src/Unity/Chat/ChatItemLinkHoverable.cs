using Databases;
using IdlePlus.Attributes;
using IdlePlus.Utilities;
using Popups;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace IdlePlus.Unity.Chat {
	
	[RegisterIl2Cpp]
	public class ChatItemLinkHoverable : MonoBehaviour {

		public static float YOffset = 0.1f;
		
		private static Camera _camera;
		private RectTransform _rect;
		private InventoryItemHoverPopup _popup;
		private TextMeshProUGUI _text;

		private string _selectedId;

		public void Setup(TextMeshProUGUI text) {
			var popup = PopupManager.Instance.GetPopup(HardPopup.InventoryItemHoverPopup);
			this._popup = new InventoryItemHoverPopup(popup.Pointer);
			this._text = text;
		}
		
		public void Awake() {
			if (!_camera) _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
			this._rect = GetComponent<RectTransform>();

			SceneManager.sceneLoaded += (UnityAction<Scene, LoadSceneMode>)delegate(Scene scene, LoadSceneMode mode) {
				if (!_camera) _camera = GameObject.Find("Main Camera").GetComponent<Camera>();
			};
		}

		public void Update() {
			if (!this._text) return;
			CheckForMouse();
		}

		private void CheckForMouse() {
			var mousePos = Vec3.Vec(Input.mousePosition.x, Input.mousePosition.y, 0);
			bool isIntersecting = TMP_TextUtilities.IsIntersectingRectTransform(this._rect, mousePos, _camera);
			IdleLog.Info($"Intersecting: {isIntersecting} ({mousePos.x} {mousePos.y})");
			if (!isIntersecting) {
				if (this._selectedId == null) return;
				this._popup.Hide();
				this._selectedId = null;
				return;
			}

			int intersectingLink = TMP_TextUtilities.FindIntersectingLink(this._text, mousePos, _camera);
			if (intersectingLink <= -1) {
				if (this._selectedId == null) return;
				this._popup.Hide();
				this._selectedId = null;
				return;
			}
			
			TMP_LinkInfo linkInfo = this._text.textInfo.linkInfo[intersectingLink];
			string linkId = linkInfo.GetLinkID();

			if (this._selectedId != null && this._selectedId != linkId) {
				this._popup.Hide();
				this._selectedId = null;
			}
			
			if (!linkId.StartsWith("ITEM:")) return;
			this._selectedId = linkId;
			
			var itemId = int.Parse(linkId.Substring(5));
			var item = ItemDatabase.ItemList[itemId];
			
			PopupManager.Instance.SetupHardPopup(HardPopup.InventoryItemHoverPopup, false, false);
			var worldPos = _camera.ScreenToWorldPoint(mousePos);
			this._popup.Setup(item, Vec2.Vec(worldPos.x, worldPos.y + YOffset));
			this._popup.Show();
		}
	}
}