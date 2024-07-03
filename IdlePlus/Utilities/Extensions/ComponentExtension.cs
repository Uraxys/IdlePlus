using UnityEngine;

namespace IdlePlus.Utilities.Extensions {
	public static class ComponentExtension {
		
		// RectTransform
		
		public static void SetAnchors(this RectTransform rect, float minX, float minY, float maxX, float maxY) {
			rect.anchorMin = Vec2.Vec(minX, minY);
			rect.anchorMax = Vec2.Vec(maxX, maxY);
		}
	}
}