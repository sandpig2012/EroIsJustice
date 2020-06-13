using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GameSystem.Battle {
	public class BattleUnit : MonoBehaviour {
		[SerializeField, Tooltip("半径")] float _Size = 0.2f;
		public float Size { get { return _Size; } }

		//分类
		[SerializeField] BattleUnitType _Type = BattleUnitType.Normal;
		[HideInInspector] public BattleUnit OwnerIfSummoned = null;

		//是否为虚拟实体
		[SerializeField] bool _IsVirtual = false;
		[HideInInspector] public BattleUnit ProxySource = null; //代理对象

		//是否为限时单位
		[SerializeField] bool _IsTimeLimited = false;
		[SerializeField] float _TimeLeft = 0;

		//是否为不可破坏
		[SerializeField] bool _IsUndamageable = false;

		//属性(Attributes)
		[SerializeField] int _MaxHP = 100;
		[SerializeField] int _CurrentHP = 100;

		[SerializeField] int _PhysicalAttack = 10;
		[SerializeField] int _MagicalAttack = 10;
		[SerializeField] float _BaseAttackLoopTime = 2.0f;
		[SerializeField] float _AttackSpeed = 1.0f;

		[SerializeField] int _PhysicalDefense = 0;
		[SerializeField] int _MagicalDefense = 0;


		void FixedUpdate() {
		}


		void Update() {

		}

		//////////////////////
		////    Gizmos    ///
		////////////////////
		#region [ Gizmos ]
#if UNITY_EDITOR
		static int GizmosCirclePointNum = 64;
		static Vector2[] GizmosCirclePointsCache = GetGizmosCirclePointsCache();

		static Vector2[] GetGizmosCirclePointsCache() {
			Vector2[] positions = new Vector2[GizmosCirclePointNum];
			float a = 2f * Mathf.PI / GizmosCirclePointNum;
			for (int i = 0; i < GizmosCirclePointNum; i++) {
				float radian = a * i;
				positions[i] = new Vector2(Mathf.Sin(radian), Mathf.Cos(radian));
			}
			return positions;
		}

		void OnDrawGizmos() {
			Color colorCache = Gizmos.color;
			Gizmos.color = new Color(1.0f, 0.5f, 0.0f, 1.0f);
			Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
			float y = 0f;
			float radius = _Size;
			Vector3[] points = new Vector3[GizmosCirclePointNum];
			for (int i = 0; i < GizmosCirclePointNum; i++) {
				points[i] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(GizmosCirclePointsCache[i].x * radius, y, GizmosCirclePointsCache[i].y * radius));
			}
			for (int i = 0; i < GizmosCirclePointNum - 1; i++) {
				Gizmos.DrawLine(points[i], points[i + 1]);
			}
			Gizmos.DrawLine(points[GizmosCirclePointNum - 1], points[0]);
			Gizmos.color = colorCache;
		}
#endif
		#endregion
	}

	enum BattleUnitType {
		Normal,
		Summon,
	}
}
