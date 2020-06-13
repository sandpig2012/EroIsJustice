using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Battle {
	/// <summary>
	/// 用语存放和处理BattleEffect的类
	/// </summary>
	[RequireComponent(typeof(BattleUnit))]
	public class StatusPlugHolder : MonoBehaviour {

		public List<StatusPlug> StatusPlugList = new List<StatusPlug>();

		BattleUnit AttachedBattleUnit = null;

		void Start() {
			AttachedBattleUnit = GetComponent<BattleUnit>();
#if UNITY_EDITOR
			if (AttachedBattleUnit == null) {
				Debug.LogError("找不到StatusPlugHolder关联的BattleUnit！");
			}
#endif
		}

		void OnDestroy() {
			MemoryUtil.SetNull(ref StatusPlugList);
		}
	}

	/// <summary>
	/// 属性更新缓存
	/// </summary>
	public class StatusPlugModifierCache {

		public bool? ModIsTimeLimited = null;
		public float? ModTimeLeft = null;

		public bool? ModIsUndamageable = null;

		public int? ModMaxHP = null;
		public int? ModCurrentHP = null;
		public int? ModPhysicalAttack = null;
		public int? ModMagicalAttack = null;
		public float? ModBaseAttackLoopTime = null;
		public float? ModAttackSpeed = null;

		public int? ModPhysicalDefense = null;
		public int? ModMagicalDefense = null;

		List<ActionOnHit> EffectOnHitList = new List<ActionOnHit>();
		List<Orb> OrbList = new List<Orb>();

		public void AddEffectOnHit(ActionOnHit effectOnHit) {
			EffectOnHitList.Add(effectOnHit);
		}
		public void AddOrb(Orb orb) {
			OrbList.Add(orb);
		}

		/// <summary>
		/// 重置缓存（清空）
		/// </summary>
		public void ResetCache() {
			ModIsTimeLimited = null;
			ModTimeLeft = null;
			ModIsUndamageable = null;
			ModMaxHP = null;
			ModCurrentHP = null;
			ModPhysicalAttack = null;
			ModMagicalAttack = null;
			ModBaseAttackLoopTime = null;
			ModAttackSpeed = null;
			ModPhysicalDefense = null;
			ModMagicalDefense = null;
			EffectOnHitList.Clear();
			OrbList.Clear();
		}
	}
}
