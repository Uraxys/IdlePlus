using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using UnityEngine;

namespace IdlePlus.Unity {
	
	[RegisterIl2Cpp]
	public class MatchParentSize : MonoBehaviour {

		private RectTransform _parentRect;
		private RectTransform _rect;
		
		public void Awake() {
			if (transform.parent == null) {
				_parentRect = null;
				return;
			}

			_rect = gameObject.Use<RectTransform>();
			_parentRect = transform.parent.Use<RectTransform>();
		}
		
		public void Update() {
			if (_parentRect == null) return;
			if (_rect == null) _rect = gameObject.Use<RectTransform>();
			if (_parentRect.sizeDelta == _rect.sizeDelta) return;
			_rect.sizeDelta = _parentRect.sizeDelta;
		}

		public void OnTransformParentChanged() {
			if (transform.parent == null) {
				_parentRect = null;
				return;
			}
			_parentRect = transform.Use<RectTransform>();
		}
	}
}