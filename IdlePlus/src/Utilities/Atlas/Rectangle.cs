using UnityEngine;

namespace IdlePlus.Utilities.Atlas {
	
	public class Rectangle {
		
		public int X { get; }
		public int Y { get; }
		public int Width { get; }
		public int Height { get; }

		public Rectangle(int x, int y, int width, int height) {
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}

		public Rect ToRect() {
			return new Rect(X, Y, Width, Height);
		}
	}
}