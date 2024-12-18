using System;

namespace IdlePlus.Utilities.Attributes {

	[AttributeUsage(AttributeTargets.Class)]
	public class RegisterIl2Cpp : Attribute {

		public Type[] Interfaces { get; set; }
		
		public RegisterIl2Cpp(params Type[] interfaces) {
			Interfaces = interfaces;
		}
		
	}
}