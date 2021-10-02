using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace CharSlotsTools
{	
	[Serializable]
	public class Character
	{
		private CharacterName characterName;
		private CharacterName[] likedCharacters;
		private CharacterName[] dislikedCharacters;
		private string serialPreference;
		private string[] likedModules;
		private string[] dislikedModules;
		private string likedEdgework;
		private string dislikedEdgework;

		public CharacterName CharacterName { get { return characterName; } }
		public CharacterName[] LikedCharacters { get { return likedCharacters; } }
		public CharacterName[] DislikedCharacters { get { return dislikedCharacters; } }
		public string SerialPreference { get { return serialPreference; } }
		public string[] LikedModules { get { return likedModules; } }
		public string[] DislikedModules { get { return dislikedModules; } }
		public string LikedEdgework { get { return likedEdgework; } }
		public string DislikedEdgework { get { return dislikedEdgework; } }

		public Character(CharacterName charName)
		{
			characterName = charName;
			Data.charToLikedCharacters.TryGetValue(charName, out likedCharacters);
			Data.charToDislikedCharacters.TryGetValue(charName, out dislikedCharacters);
			Data.charToSerial.TryGetValue(charName, out serialPreference);
			Data.charToLikedModules.TryGetValue(charName, out likedModules);
			Data.charToDislikedModules.TryGetValue(charName, out dislikedModules);
			Data.charToLikedEdgework.TryGetValue(charName, out likedEdgework);
			Data.charToDislikedEdgework.TryGetValue(charName, out dislikedEdgework);
		}

	}

}

