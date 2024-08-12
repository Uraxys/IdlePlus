using IdlePlus.Utilities;
using IdlePlus.Utilities.Attributes;
using IdlePlus.Utilities.Extensions;
using UnityEngine;
using UnityEngine.UI.ProceduralImage;

namespace IdlePlus.Unity {
	
	[RegisterIl2Cpp]
	public class DebugRender : MonoBehaviour {

		private GameObject _dummy;
		private RectTransform _rectTransform;
		
		public Color Color {
			get => _dummy.Use<ProceduralImage>().color;
			set => _dummy.Use<ProceduralImage>().color = value;
		}
		
		public void Awake() {
			_dummy = GameObjects.NewRect<ProceduralImage, UniformModifier>("DebugRender", gameObject);
			_rectTransform = _dummy.Use<RectTransform>();
			_rectTransform.sizeDelta = gameObject.Use<RectTransform>().sizeDelta;
			
			_dummy.Use<ProceduralImage>(image => {
				image.borderWidth = 1;
				image.color = Color.magenta;
			});
		}

		public void OnDestroy() {
			if (_dummy == null) return;
			Destroy(_dummy);
		}

		public void OnEnable() {
			if (_dummy == null) return;
			_dummy.SetActive(true);
		}
		
		public void OnDisable() {
			if (_dummy == null) return;
			_dummy.SetActive(false);
		}

		public void Update() {
			if (_dummy == null) return;
			var rect = gameObject.Use<RectTransform>();
			if (rect.sizeDelta == _rectTransform.sizeDelta) return;
			IdleLog.Info("Updating DebugRender size.");
			_rectTransform.sizeDelta = rect.sizeDelta;
		}
	}
}