using System;
using UnityEngine.Events;

namespace IdlePlus.Utilities.Extensions {
	public static class UnityEventExtension {

		public static void Listen(this UnityEvent @event, Action action) {
			@event.AddListener((UnityAction) delegate { action(); });
		}
		
		public static void Listen<T0>(this UnityEvent<T0> @event, Action<T0> action) {
			@event.AddListener((UnityAction<T0>) delegate(T0 a) { action(a); });
		}
		
		public static void Listen<T0, T1>(this UnityEvent<T0, T1> @event, Action<T0, T1> action) {
			@event.AddListener((UnityAction<T0, T1>) delegate(T0 a, T1 b) { action(a, b); });
		}
	}
}