using System;
using System.Collections.Generic;
using System.Linq;
using IdlePlus.Utilities;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.InteropTypes;
using UnityEngine;
using UnityEngine.EventSystems;
using Type = Il2CppSystem.Type;

namespace IdlePlus.API.Unity {
	
	/// <summary>
	/// <para>A basic version of Unity's <see cref="EventSystem"/> for detecting
	/// mouse events. This custom implementation is necessary because the original
	/// may break in modded environments where the game uses IL2CPP, such as
	/// Idle Clan.
	/// </para>
	/// <para>To use, implement one of the <see cref="IMouseEvent"/> interfaces,
	/// for example, <see cref="IMouseEnterHandler"/> to detect when the mouse
	/// enters the <see cref="GameObject"/>.
	/// </para>
	/// </summary>
	public static class MouseEventManager {

		// The currently selected game object.
		private static readonly CachedSelectedObject Selected = new CachedSelectedObject();
		// The types we've registered.
		private static readonly Dictionary<Type, RegisteredType> RegisteredTypes = new Dictionary<Type, RegisteredType>();
		
		// Public API

		public delegate MonoBehaviour RegisteredTypeFactory(Component component);
		
		public static void Register<T>(RegisteredTypeFactory factory) {
			RegisteredTypes[Il2CppType.From(typeof(T), true)] = new RegisteredType(typeof(T), factory);
			IdleLog.Info($"Registered type {Il2CppType.From(typeof(T), true).Name}");
		}
		
		// Internal
		
		internal static void Tick() {
			if (!EventSystem.current) return;
			Vector3 mousePosition = Input.mousePosition;
			
			PointerEventData data = new PointerEventData(null) { position = mousePosition };
			Il2CppSystem.Collections.Generic.List<RaycastResult> hits = new Il2CppSystem.Collections.Generic.List<RaycastResult>();
			EventSystem.current.RaycastAll(data, hits);

			RaycastResult result = null;
			foreach (var hit in hits) {
				if (!hit.gameObject) continue;
				result = hit;
				break;
			}

			MouseEventData eventData = new MouseEventData(Input.mousePosition);
			ProcessEnterExit(eventData, result?.gameObject);
			ProcessMove(eventData);
		}

		private static void ProcessEnterExit(MouseEventData data, GameObject current) {
			Selected.JustSelected = false;
			
			// Make sure we're selecting something and have something to update.
			if (current == null && !Selected.GameObject) return;
			if (Selected.GameObject == current) return;
			
			// Call exit on the previously selected object.
			// We're having force set to true to always send an exit event, even
			// if the object is disabled.
			ExecuteEvent(Selected.GameObject, data, MouseEventFunctions.MouseExitFunc, true);

			// Check if we have a new game object.
			if (current == null) {
				// We don't, so just clear the current one.
				Selected.Clear();
				return;
			}
			
			// Set the selected object to the new current object and call the
			// enter event.
			// We do this before actually calling the event in case it throws an
			// exception, resulting in the exit event being called more than once.
			Selected.Set(current, data);
			ExecuteEvent(Selected.GameObject, data, MouseEventFunctions.MouseEnterFunc);
		}

		private static void ProcessMove(MouseEventData data) {
			if (!Selected.GameObject || Selected.JustSelected) return;
			if (Selected.MoveEvent == null) return;
			if (Selected.MoveLastPosition == data.MousePosition) return;

			Selected.MoveLastPosition = data.MousePosition;
			Selected.MoveEvent?.Invoke(data);
		}
		
		private static void ExecuteEvent<T>(GameObject obj, MouseEventData data,
			MouseEventFunctions.MouseEventFunc<T> func, bool force = false) where T : class {

			if (obj == null || (!obj.activeInHierarchy && !force)) return;
			var components = obj.GetComponents<Component>();
			
			foreach (var component in components) {
				if (!RegisteredTypes.TryGetValue(component.GetIl2CppType(), out var type)) continue;
				var casted = type.TryCastTo<T>(component);
				if (casted == null) continue;
				
				Behaviour behaviour = casted as Behaviour;
				if (!behaviour || (!behaviour.isActiveAndEnabled && !force)) continue;
				
				try {
					func.Invoke(casted, data);
				} catch (Exception e) {
					IdleLog.Error("Exception occurred while handling mouse event.", e);
					throw;
				}
			}
		}

		private class CachedSelectedObject {
			
			public GameObject GameObject { get; private set; }
			public bool JustSelected { get; set; }
			
			// Events
			
			public MouseEventFunctions.MouseEventSimpleFunc MoveEvent { get; set; }
			public Vector3 MoveLastPosition { get; set; }


			public void Set(GameObject obj, MouseEventData data) {
				this.GameObject = obj;
				this.JustSelected = true;
				
				this.MoveEvent = null;
				this.MoveLastPosition = data.MousePosition;
				
				var components = obj.GetComponents<Component>();
				var moveHandlers = new List<IMouseMoveHandler>();
				foreach (var component in components) {
					if (!RegisteredTypes.TryGetValue(component.GetIl2CppType(), out var type)) continue;
					var casted = type.TryCastTo<IMouseMoveHandler>(component);
					if (casted == null) continue;
					moveHandlers.Add(casted);
				}
				
				this.MoveEvent = d => {
					foreach (var handler in moveHandlers) {
						try {
							handler.HandleMouseMove(d);
						} catch (Exception e) {
							IdleLog.Error("Exception occurred while handling mouse move event.", e);
							throw;
						}
					}
				};
			}

			public void Clear() {
				this.GameObject = null;
				this.MoveEvent = null;
			}
		}

		private class RegisteredType {

			private readonly RegisteredTypeFactory _factory;
			private readonly HashSet<System.Type> _supports = new HashSet<System.Type>();

			public RegisteredType(System.Type type, RegisteredTypeFactory factory) {
				this._factory = factory;
				if (typeof(IMouseEnterHandler).IsAssignableFrom(type)) this._supports.Add(typeof(IMouseEnterHandler));
				if (typeof(IMouseExitHandler).IsAssignableFrom(type)) this._supports.Add(typeof(IMouseExitHandler));
				if (typeof(IMouseMoveHandler).IsAssignableFrom(type)) this._supports.Add(typeof(IMouseMoveHandler));
			}

			public T TryCastTo<T>(Component component) where T : class {
				if (!this._supports.Contains(typeof(T))) return null;
				return this._factory.Invoke(component) as T;
			}
		}
	}

	public class MouseEventData {
		
		public readonly Vector3 MousePosition;
		
		public MouseEventData(Vector3 mousePosition) {
			this.MousePosition = mousePosition;
		}
	}

	internal static class MouseEventFunctions {

		internal delegate void MouseEventFunc<in T>(T handler, MouseEventData data);
		internal delegate void MouseEventSimpleFunc(MouseEventData data);
		
		internal static readonly MouseEventFunc<IMouseEnterHandler> MouseEnterFunc =
			(handler, data) => handler.HandleMouseEnter(data);

		internal static readonly MouseEventFunc<IMouseExitHandler> MouseExitFunc =
			(handler, data) => handler.HandleMouseExit(data);
	}

	public interface IMouseEvent {}
	
	public interface IMouseEnterHandler : IMouseEvent {
		void HandleMouseEnter(MouseEventData data);
	}
	
	public interface IMouseExitHandler : IMouseEvent {
		void HandleMouseExit(MouseEventData data);
	}
	
	public interface IMouseMoveHandler : IMouseEvent {
		void HandleMouseMove(MouseEventData data);
	}
}