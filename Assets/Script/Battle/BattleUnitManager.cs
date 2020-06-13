using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Battle {
	public class BattleUnitManager : MonoBehaviour {
		
		public static BattleUnitManager CurrentManager;

		public List<BattleUnit> AllyList = new List<BattleUnit>();
		public List<BattleUnit> EnemyList = new List<BattleUnit>();
		public List<BattleUnit> NeutralList = new List<BattleUnit>();

		void OnDestroy() {
			MemoryUtil.SetNull(ref AllyList);
			MemoryUtil.SetNull(ref EnemyList);
			MemoryUtil.SetNull(ref NeutralList);
		}

		void Start() {
			CurrentManager = this;
		}
		void Update() {
			if(CurrentManager != this) {
				CurrentManager = this;
			}
		}
	}
}
