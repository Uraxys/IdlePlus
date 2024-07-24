using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using UnityEngine;

namespace IdlePlus.Unity {
	
	[RegisterIl2Cpp]
	public class MatchParentSize : MonoBehaviour {

		private RectTransform _rect;
		
		public void Awake() {
			_rect = gameObject.Use<RectTransform>();
		}
		
		public void Update() {
			if (transform.parent == null) return;
			var parentRect = transform.parent.Use<RectTransform>();
			if (parentRect.sizeDelta == _rect.sizeDelta) return;
			_rect.sizeDelta = parentRect.sizeDelta;
		}
	}
}