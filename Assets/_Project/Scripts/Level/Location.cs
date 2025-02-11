﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace BrightFish
{
	[CreateAssetMenu(fileName = "Location", menuName = "BrightFish/Location")]
	public class Location : ScriptableObject
	{
		public int ID => _id;
		public Level[] Levels => _levels;

		[SerializeField] private int _id;
		[SerializeField] private Level[] _levels;

		//----------------------------------------------------------------

		public Level GetLevel(string id)
		{
			return _levels.FirstOrDefault(lvl => lvl.ID == id);
		}

		public string GetNextLevelId(string prevLevelId)
		{
			var prevIndex = Array.IndexOf(_levels, GetLevel(prevLevelId));

			return _levels[prevIndex + 1].ID;
		}

		public int GetLevelIndex(string id)
		{
			return Array.IndexOf(_levels, GetLevel(id));
		}

		public Level GetMaxAvailableLevel()
		{
			var currentLvlId = GameProgress.GetMaxAvailableLevelId();
			return _levels.FirstOrDefault(lvl => lvl.ID == currentLvlId);
		}
	}
}