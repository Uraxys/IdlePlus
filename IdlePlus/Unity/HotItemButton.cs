using ButtonAnimations;
using Databases;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using PlayerMarket;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace IdlePlus.Unity {
	
	[RegisterIl2Cpp]
	public class HotItemButton : MonoBehaviour {

		private bool _initialized;

		private PlayerMarketPage _playerMarket;
		private HotItemEntry _hotItemEntry;
		private Button _button;
		private ButtonScale _buttonScale;

		private int _itemId;
		
		private void Awake() {
			if (_initialized) return;
			_playerMarket = GameObjects.FindByPathNonNull<PlayerMarketPage>("GameCanvas/PageCanvas/PlayerMarket");
			_hotItemEntry = GetComponent<HotItemEntry>();
			_button = transform.gameObject.AddComponent<Button>();
			_buttonScale = transform.gameObject.AddComponent<ButtonScale>();

			_button.onClick.AddListener((UnityAction) OnButtonPressed);
			_buttonScale._scaleAmount = 1.025F;
			_initialized = true;
		}

		public void Setup(int itemId) {
			if (!_initialized) Awake();
			
			_itemId = itemId;
			if (itemId < 0) {
				_button.enabled = false;
				_buttonScale.enabled = false;
				return;
			}
			
			_button.enabled = true;
			_buttonScale.enabled = true;
		}

		private void OnButtonPressed() {
			if (_itemId < 0) return;
			_playerMarket._frontPageContainer.SetActive(false);
			_playerMarket._searchPage.Setup(_playerMarket._frontPageContainer, ItemDatabase.ItemList[_itemId]);
		}
	}
}