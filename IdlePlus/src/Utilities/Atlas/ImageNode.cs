using UnityEngine;

namespace IdlePlus.Utilities.Atlas {
	public class ImageNode {

		public readonly Rectangle Rect;
		private readonly ImageNode[] _child = new ImageNode[2];
		private Texture2D _texture;

		public ImageNode(int x, int y, int width, int height) {
			Rect = new Rectangle(x, y, width, height);
			_child[0] = null;
			_child[1] = null;
		}
		
		private bool IsLeaf() {
			return _child[0] == null && _child[1] == null;
		}

		public ImageNode Insert(Texture2D texture, int padding) {
			if (!IsLeaf()) {
				var node = _child[0].Insert(texture, padding);
				return node ?? _child[1].Insert(texture, padding);
			}
			
			// Check if we're occupied.
			if (_texture != null) return null;
			// Check if the texture fits.
			if (texture.width > Rect.Width || texture.height > Rect.Height) return null;
			
			// Check if the texture fits perfectly.
			if (texture.width == Rect.Width && texture.height == Rect.Height) {
				_texture = texture;
				return this;
			}
			
			var dw = Rect.Width - texture.width;
			var dh = Rect.Height - texture.height;

			if (dw > dh) {
				_child[0] = new ImageNode(Rect.X, Rect.Y, texture.width, Rect.Height);
				_child[1] = new ImageNode(padding + Rect.X + texture.width, Rect.Y, 
					Rect.Width - texture.width - padding, Rect.Height);
			} else {
				_child[0] = new ImageNode(Rect.X, Rect.Y, Rect.Width, texture.height);
				_child[1] = new ImageNode(Rect.X, padding + Rect.Y + texture.height, 
					Rect.Width, Rect.Height - texture.height - padding);
			}
			return _child[0].Insert(texture, padding);
		}
	}
}