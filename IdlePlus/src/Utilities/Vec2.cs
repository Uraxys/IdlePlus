using UnityEngine;

namespace IdlePlus.Utilities {
	public static class Vec2 {
		
		public static Vector2 Zero => Vec(0);
		public static Vector2 One => Vec(1);
		
		public static Vector2 Vec(float value) => new Vector2(value, value); 
		public static Vector2 Vec(float x, float y) => new Vector2(x, y);
		
		// Extensions
		
		public static Vector2 SetX(this Vector2 vector, float x) => Vec(x, vector.y);
		public static Vector2 SetY(this Vector2 vector, float y) => Vec(vector.x, y);
		
		public static Vector2 Add(this Vector2 vector, Vector2 other) => vector + other;
		public static Vector2 Add(this Vector2 vector, float value) => vector + Vec(value);
		public static Vector2 Add(this Vector2 vector, float x, float y) => vector + Vec(x, y);
		
		public static Vector2 Sub(this Vector2 vector, Vector2 other) => vector - other;
		public static Vector2 Sub(this Vector2 vector, float value) => vector - Vec(value);
		public static Vector2 Sub(this Vector2 vector, float x, float y) => vector - Vec(x, y);
	}
}