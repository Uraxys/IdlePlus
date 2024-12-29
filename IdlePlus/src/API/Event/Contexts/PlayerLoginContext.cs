using Player;

namespace IdlePlus.API.Event.Contexts {
	public class PlayerLoginContext : EventContext {
		public PlayerData PlayerData { get; }
		public PlayerLoginContext(PlayerData playerData) {
			this.PlayerData = playerData;
		}
	}
}