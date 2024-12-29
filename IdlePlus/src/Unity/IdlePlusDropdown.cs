using IdlePlus.Attributes;
using IdlePlus.Utilities;
using IdlePlus.Utilities.Extensions;
using Michsky.MUIP;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.Unity {
	
	// TODO: I hate this dropdown so much... refactor at a later date.
	[RegisterIl2Cpp]
	public class IdlePlusDropdown : MonoBehaviour {

		private static readonly Color SelectedColor = new Color(0f, 0f, 0f, 0.3f);
		private static readonly Color UnselectedColor = new Color(0f, 0f, 0f, 0.15f);

		public bool Localize { get; set; }

		public int SelectedIndex {
			get => _selectedIndex;
			set {
				_selectedIndex = value;
				UpdateEntries(_selectedIndex);
			}
		}

		public string[] Items {
			get => _items;
			set {
				_items = value;
				Setup();
			}
		}
		
		public CustomDropdown.DropdownEvent OnValueChanged { get; private set; }

		private string[] _items = { "no", "value", "set" };
		private int _selectedIndex;
		private CustomDropdown _dropdown;
		
		private void Awake() {
			_dropdown = GetComponent<CustomDropdown>();
			
			Setup();
			
			OnValueChanged = _dropdown.onValueChanged;
			OnValueChanged.Listen(OnSelectedChanged);
		}

		public void OnEnable() {
			UpdateEntries(SelectedIndex);
		}

		private void Setup() {
			if (_dropdown == null) return;
			_dropdown.items.Clear();
			for (var index = 0; index < Items.Length; index++) {
				var itemName = Items[index];
				var item = new CustomDropdown.Item {
					itemName = Localize ? ModLocalization.GetModdedValue(itemName) : itemName,
					interactable = true,
					buttonColor = index == SelectedIndex ? SelectedColor : UnselectedColor
				};
				_dropdown.items.Add(item);
			}
			_dropdown.selectedItemIndex = SelectedIndex;
			_dropdown.SetupDropdown(true);
			UpdateEntries(SelectedIndex);
		}

		private void OnSelectedChanged(int index) {
			SelectedIndex = index;
			UpdateEntries(index);
		}

		public void UpdateEntries(int index) {
			var items = _dropdown.itemList;
			_dropdown.selectedItemIndex = SelectedIndex;
			
			for (var i = 0; i < Items.Length; i++) {
				var entry = items.transform.GetChild(i);
				
				var button = entry.GetComponent<Button>();
				button.interactable = i != index;
				
				var proceduralImage = entry.GetComponent<ProceduralImage>();
				proceduralImage.color = i == index ? SelectedColor : UnselectedColor;
			}
		}
	}
}