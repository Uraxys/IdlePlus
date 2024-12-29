using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Il2CppInterop.Runtime.Injection;

namespace IdlePlus.Attributes {

	[AttributeUsage(AttributeTargets.Class)]
	public class RegisterIl2CppAttribute : Attribute {

		public Type[] Interfaces { get; set; }
		
		public RegisterIl2CppAttribute(params Type[] interfaces) {
			Interfaces = interfaces;
		}
		
	}

	public static class RegisterIl2CppAttributeHandler {

		private static bool _registered;

		public static void Register() {
			if (_registered) throw new InvalidOperationException("RegisterIl2Cpp has already been called!");
			_registered = true;
			
			var il2CPPTypes = 
				from t in Assembly.GetExecutingAssembly().GetTypes()
				let attributes = t.GetCustomAttributes(typeof(RegisterIl2CppAttribute), true)
				where attributes != null && attributes.Length > 0
				select new { Type = t, Attributes = attributes.Cast<RegisterIl2CppAttribute>() };
			
			foreach (var type in il2CPPTypes) {
				var attribute = type.Attributes.First();
				var registerOptions = RegisterTypeOptions.Default;
				
				if (attribute.Interfaces != null && attribute.Interfaces.Length > 0) {
					registerOptions = new RegisterTypeOptions();
					var interfaceCollection = (Il2CppInterfaceCollection) attribute.Interfaces;

					// I.. I... just... ...
					Traverse.Create(registerOptions)
						.Property("Interfaces").SetValue(interfaceCollection)
						.Property("LogSuccess").SetValue(true);
				}
				
				ClassInjector.RegisterTypeInIl2Cpp(type.Type, registerOptions);
			}
		}

	}
}