using System;

namespace IdlePlus.API.Utility {
	public static class Asserts {

		public static void NotNull(object value, string param, string message = null) {
			if (message == null) message = $"{param} cannot be null";
			if (value == null) throw new AssertException($"NotNull assertion failed.\nMessage: {message}.\nParam: {param}.");
		}
		
	}

	public class AssertException : Exception {
		public AssertException(string message) : base(message) { }
		
	}
}