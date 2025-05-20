using System;
using System.Linq;
using Databases;

namespace IdlePlus.API.Utility.Game {
	
	/*
	 * Should be the vanilla implementation of the experience table.
	 * Provided by Temsei in Discord DMs, thanks! :D
	 *
	 * We're using this instead of SkillManager, as it's unreliable in IL2CPP
	 * modding land.
	 */
	
	public static class SkillUtils {
		
		private static int[] _expTable;

		private static void SetLevelExperienceRequirements() {
			double points = 0;
			double output = 0;
    
			_expTable = new int[SettingsDatabase.SharedSettings.MaxSkillLevel + 1];
			_expTable[0] = (int)output;

			for (int level = 1; level <= SettingsDatabase.SharedSettings.MaxSkillLevel; level++) {
				if (level == 1) {
					_expTable[level] = 0;
					continue;
				}

				points += Math.Floor(level + 300 * Math.Pow(2, level / 7));
				output = Math.Floor(points / 4);

				_expTable[level] = (int)output;
			}
		}

		public static int GetLevelForExperience(int experience) {
			if (_expTable == null) SetLevelExperienceRequirements();
			
			for (int i = _expTable.Length - 1; i >= 0; i--) {
				if (_expTable[i] <= experience) {
					return Math.Min(i, SettingsDatabase.SharedSettings.MaxSkillLevel);
				}
			}

			return 1;
		}

		public static int GetExperienceForLevel(int level) {
			if (_expTable == null) SetLevelExperienceRequirements();
			
			if (level >= SettingsDatabase.SharedSettings.MaxSkillLevel) {
				return _expTable.Last();
			}
    
			return _expTable[level];
		}
	}
}