using System;

namespace IdlePlus.API.Utility {
	public class GeneralStringReader {
		
		public int Index { get; private set; }
		public string String { get; }

		/// <summary>
		/// The remaining length of the string.
		/// </summary>
		public int Remaining => String.Length - Index;
		/// <summary>
		/// The total length of the string.
		/// </summary>
		public int Length => String.Length;

		public GeneralStringReader(string s) {
			String = s;
		}

		public bool CanRead(int lenght) => Index + lenght <= String.Length;
		public bool CanRead() => CanRead(1);

		public char Peek(int offset) => String[Index + offset];
		public char Peek() => Peek(0);
		
		public void Skip(int length) => Index += length;
		public void Skip() => Skip(1);

		public int IndexOf(char c) => String.IndexOf(c, Index);

		public char Next(int skip) {
			Index += skip;
			return String[Index++];
		}

		public char Next() => Next(0);

		public string ReadStr(int size) {
			size = Math.Min(size, String.Length - Index);
			string result = String.Substring(Index, size);
			Index += size;
			return result;
		}

		public string ReadStr() => ReadStr(String.Length - Index);
	}
}