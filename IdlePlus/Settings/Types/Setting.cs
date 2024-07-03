using UnityEngine;

namespace IdlePlus.Settings.Types {
	public abstract class Setting {
		public const string DescriptionField = "Description";
		public const string RestartField = "Restart";

		/// <summary>
		/// The unique identifier of the setting.
		/// </summary>
		public string Id { get; protected set; }
		
		/// <summary>
		/// If the setting requires a restart to take effect.
		/// </summary>
		public bool RequireRestart { get; protected set; }
		
		/// <summary>
		/// The description of the setting.
		/// </summary>
		public string Description { get; protected set; }
		
		/// <summary>
		/// Used to determine if the value has been changed and requires a restart.
		/// </summary>
		public bool Dirty { get; protected set; }
		
		public abstract byte[] Serialize();
		public abstract void Deserialize(byte[] data);
		
		public abstract void Initialize(GameObject obj);
		public abstract GameObject GetPrefab();

	}
}