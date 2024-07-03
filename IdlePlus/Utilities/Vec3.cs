using UnityEngine;

namespace IdlePlus.Utilities {
	public static class Vec3 {

		public static Vector3 Zero => Vec(0);
		
		public static Vector3 Vec(float value) => new Vector3(value, value, value);
		public static Vector3 Vec(float x, float y, float z) => new Vector3(x, y, z);
		
		// Extensions
		
		public static Vector3 SetX(this Vector3 vector, float x) => Vec(x, vector.y, vector.z);
		public static Vector3 SetY(this Vector3 vector, float y) => Vec(vector.x, y, vector.z);
		public static Vector3 SetZ(this Vector3 vector, float z) => Vec(vector.x, vector.y, z);
		
		public static Vector3 Add(this Vector3 vector, Vector2 other) => vector + Vec(other.x, other.y, 0);
		public static Vector3 Add(this Vector3 vector, Vector3 other) => vector + other;
		public static Vector3 Add(this Vector3 vector, float value) => vector + Vec(value);
		public static Vector3 Add(this Vector3 vector, float x, float y, float z) => vector + Vec(x, y, z);
		
		public static Vector3 Sub(this Vector3 vector, Vector2 other) => vector - Vec(other.x, other.y, 0);
		public static Vector3 Sub(this Vector3 vector, Vector3 other) => vector - other;
		public static Vector3 Sub(this Vector3 vector, float value) => vector - Vec(value);
		public static Vector3 Sub(this Vector3 vector, float x, float y, float z) => vector - Vec(x, y, z);
	}
}