using Player;

namespace IdlePlus.API.Event.Contexts {
	public class PlayerLoginEventContext : EventContext {
		public PlayerData PlayerData { get; }
		public PlayerLoginEventContext(PlayerData playerData) {
			this.PlayerData = playerData;
		}
	}
}