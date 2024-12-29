using System;
using IdlePlus.API.Event.Contexts;
using IdlePlus.Utilities;

namespace IdlePlus.API.Event {
	
	/*
	 * WORK IN PROGRESS
	 */
	
	public static class Events {
		/// <summary>
		/// Scene events.
		/// </summary>
		public static class Scene {
			/// <summary>
			/// Called when the user enters the game scene after logging in.
			/// </summary>
			public static readonly SimpleEventAction OnGame = new SimpleEventAction("OnGame");
			/// <summary>
			/// Called when the user enters the lobby scene.
			/// </summary>
			public static readonly SimpleEventAction OnLobby = new SimpleEventAction("OnLobby");
		}

		/// <summary>
		/// Player events.
		/// </summary>
		public static class Player {
			/// <summary>
			/// Called after the player has logged in and is in the game scene.
			/// </summary>
			public static readonly EventAction<PlayerLoginContext> OnLogin = new EventAction<PlayerLoginContext>("OnLogin");
		}
	}
	
	#region Event Action

	public class EventContext {}
	public class CancellableEventContext : EventContext {
		public bool IsCancelled { get; private set; }
		public void Cancel() { this.IsCancelled = true; }
	}
	
	public class SimpleEventAction {
		
		private Action _action;
		private readonly string _name;
		
		public SimpleEventAction(string name) {
			this._name = name;
		}

		public void Call() {
			try {
				this._action?.Invoke();
			} catch (Exception e) {
				IdleLog.Error($"Error while invoking simple event action {this._name}!", e);
			}
		}
		
		public void Register(Action callback) {
			this._action += callback;
		}
	}
	
	public class EventAction<T> where T : EventContext {
		
		private Action<T> _action;
		private readonly string _name;
		
		public EventAction(string name) {
			this._name = name;
		}

		public T Call(T context) {
			try {
				this._action?.Invoke(context);
			} catch (Exception e) {
				IdleLog.Error($"Error while invoking event action {this._name}!", e);
			}
			return context;
		}
		
		public void Register(Action<T> callback) {
			this._action += callback;
		}
	}
	
	#endregion
}