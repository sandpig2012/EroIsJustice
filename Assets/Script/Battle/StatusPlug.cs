using System.Collections;
using System.Collections.Generic;
using UnityEditor.Compilation;
using UnityEngine;

namespace GameSystem.Battle {
	/// <summary>
	/// 单位附加状态
	/// </summary>
	public class StatusPlug : ScriptableObject {

		[SerializeField] AttributeModifier _AttributeModifier = null;
		[SerializeField] public ActionOnHit _EffectOnHit = null;
		[SerializeField] public Orb _Orb = null;

		public void ApplyStatusPlugToCache(ref BattleUnit battleUnit, ref StatusPlugModifierCache modifierCache) {
			if (_AttributeModifier != null) {
				_AttributeModifier.ApplyModifierToCache(ref battleUnit, ref modifierCache);
			}
			if (_EffectOnHit != null) {
				modifierCache.AddEffectOnHit(_EffectOnHit);
			}
			if (_Orb != null) {
				modifierCache.AddOrb(_Orb);
			}
		}
	}

	//////////////////////////////////
	////    Attribute Modifier    ///
	////////////////////////////////
	#region [ Attribute Modifier ]
	/// <summary>
	/// 属性修正
	/// </summary>
	public abstract class AttributeModifier {
		/// <summary>
		/// 属性修正函数
		/// </summary>
		public abstract void ApplyModifierToCache(ref BattleUnit battleUnit, ref StatusPlugModifierCache modifierCache);
	}
	#endregion

	/////////////////////////////
	////    Action On Hit    ///
	///////////////////////////
	#region [ Action On Hit ]
	/// <summary>
	/// 击中效果
	/// </summary>
	public abstract class ActionOnHit {
		/// <summary>
		/// 进行攻击时的状态数据预存处理
		/// </summary>
		public abstract void CollectDataOnCast(ref BattleUnit battleUnit, ref StatusPlugModifierCache modifierCache);
		/// <summary>
		/// 创建一个存有状态快照的EffectOnHit用于赋予Projector
		/// </summary>
		public abstract ActionOnHit CreateSnapshot();
		/// <summary>
		/// 击中目标时产生的效果
		/// </summary>
		public abstract void ActiveEffectOnHit(ref BattleUnit target);
	}
	#endregion

	///////////////////
	////    Orb    ///
	/////////////////
	#region [ Orb ]
	/// <summary>
	/// 法球效果
	/// </summary>
	public abstract class Orb {
		/// <summary>
		/// 近战模式法球效果
		/// </summary>
		public abstract void MeleeAttackOrb();
		/// <summary>
		/// 远程模式法球效果
		/// </summary>
		public abstract void RangedAttackOrb();
	}
	#endregion
}

