using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

namespace GameSystem.Battle {
	/// <summary>
	/// 攻击判定实体。近战攻击与投射物通用
	/// </summary>
	public class HitEntity : MonoBehaviour {
		[SerializeField] bool _Static = true;

		[SerializeField] float _Speed = 0.0f;
		[SerializeField] float _LifeTime = 20.0f;
		[SerializeField] AnimationCurve _SpeedModifier = AnimationCurve.Linear(0.0f, 1.0f, 1.0f, 1.0f);

		[SerializeField] bool _ImmediateEffect = true;
		[SerializeField] float _ImmdiateEffectLag = 0.0f;

		//影响对象
		BattleUnit Owner;
		[SerializeField] HitAreaType _HitAreaType = HitAreaType.None;
		public List<BattleUnit> Targets = new List<BattleUnit>();//仅当范围效果为None时使用
		[SerializeField] bool _AffectOwner = false;
		[SerializeField] bool _AffectAllies = false;
		[SerializeField] bool _AffectEnemies = true;
		[SerializeField] bool _AffectNeutrals = false;
		[SerializeField] bool _AreaNotAffectOwner = false;

		BattleUnitManager CurrentManager;
		Matrix4x4 WorldToLocalMatrix;
		Vector2 SelfPositionXZ;

		///////////////////////
		////    打击区域    ///
		/////////////////////
		#region [ Hit Area ]
		enum HitAreaType {
			None,
			Circle,
			Rectangle,
			Fan,
			Line,
		}
		//本地XZ平面
		//Circle
		[SerializeField] float _HACircleRadius = 1.0f;
		//Rectangle-半边长XZ
		[SerializeField] Vector2 _HARecSize = Vector2.one;
		//Fan
		[SerializeField] float _HAFanInnerRadius = 0.2f;
		[SerializeField] float _HAFanOuterRadius = 1.0f;
		[SerializeField] [Range(1f, 180f)] float _HAFanAngle = 30.0f;
		float CosRadian = 0f;
		float SinRadian = 1f;
		//Line
		[SerializeField] float _HALineStart = 0.0f;
		[SerializeField] float _HALineEnd = 2.0f;
		#endregion

		void Start() {
			CurrentManager = BattleUnitManager.CurrentManager;
			Vector3 SelfPosition = transform.position;
			SelfPositionXZ = new Vector2(SelfPosition.x, SelfPosition.z);
		}

		void FixedUpdate() {
			WorldToLocalMatrix = transform.worldToLocalMatrix;
			if (_HitAreaType == HitAreaType.Fan) {
				float radian = _HAFanAngle / 180f * Mathf.PI;
				CosRadian = Mathf.Cos(radian);
				SinRadian = Mathf.Sin(radian);
			}
		}

		void SetEffectOnTargets() {
			if (_AffectOwner) {
				SetEffectOntoTarget(Owner);
			}
			if (_HitAreaType == HitAreaType.None) {
				foreach (BattleUnit battleUnit in Targets) {
					SetEffectOntoTarget(battleUnit);
				}
			}
			else {
				if (CurrentManager != null) {
					if (_AffectAllies) {
						foreach (BattleUnit battleUnit in CurrentManager.AllyList) {
							if (CheckAreaCollision(battleUnit)) {
								SetEffectOntoTarget(battleUnit);
							}
						}
					}
					if (_AffectEnemies) {
						foreach (BattleUnit battleUnit in CurrentManager.EnemyList) {
							if (CheckAreaCollision(battleUnit)) {
								SetEffectOntoTarget(battleUnit);
							}
						}
					}
					if (_AffectNeutrals) {
						foreach (BattleUnit battleUnit in CurrentManager.EnemyList) {
							if (CheckAreaCollision(battleUnit)) {
								SetEffectOntoTarget(battleUnit);
							}
						}
					}
				}
			}
		}

		bool CheckAreaCollision(BattleUnit battleUnit) {
			if (_AreaNotAffectOwner && battleUnit == Owner) {
				return false;
			}
			float targetSize = battleUnit.Size;
			Vector3 targetPosition = battleUnit.transform.position;
			//------------------------------------------------------------------------------
			if (_HitAreaType == HitAreaType.Circle) {
				float distanceSqr = (new Vector2(targetPosition.x, targetPosition.z) - SelfPositionXZ).sqrMagnitude;
				float collisionDistanceSqr = _HACircleRadius + targetSize;
				collisionDistanceSqr *= collisionDistanceSqr;
				return distanceSqr <= collisionDistanceSqr;
			}
			//------------------------------------------------------------------------------
			else {
				Vector3 localTargetPosition = WorldToLocalMatrix.MultiplyPoint3x4(targetPosition);
				Vector2 localTargetPositionXZ = new Vector2(localTargetPosition.x, localTargetPosition.z);
				//------------------------------------------------------------------------------
				if (_HitAreaType == HitAreaType.Rectangle) {
					Vector2 localTargetPositionXZAbsMinusSize = new Vector2(Mathf.Abs(localTargetPositionXZ.x), Mathf.Abs(localTargetPositionXZ.y)) - _HARecSize;
					bool xInside = localTargetPositionXZAbsMinusSize.x <= 0f;
					bool yInside = localTargetPositionXZAbsMinusSize.y <= 0f;
					if (xInside && yInside) {
						return true;
					}
					else if (!xInside && !yInside) {
						float distanceSqr = localTargetPositionXZAbsMinusSize.sqrMagnitude;
						float collisionDistanceSqr = targetSize * targetSize;
						return distanceSqr <= collisionDistanceSqr;
					}
					else {
						float distance = xInside ? localTargetPositionXZAbsMinusSize.y : localTargetPositionXZAbsMinusSize.x;
						return distance <= targetSize;
					}
				}
				//------------------------------------------------------------------------------
				else if (_HitAreaType == HitAreaType.Fan) {
					float targetDistanceFromO = localTargetPositionXZ.magnitude;
					float rDistance = Mathf.Max(targetDistanceFromO - _HAFanOuterRadius, _HAFanInnerRadius - targetDistanceFromO);
					bool rInside = rDistance <= 0f;
					if (Mathf.Approximately(targetDistanceFromO, 0.0f)) {
						return rInside;
					}
					else {
						float cosTargetDir = localTargetPositionXZ.y / targetDistanceFromO;
						bool fInside = cosTargetDir >= CosRadian;
						if (fInside) {
							return rDistance <= targetSize;
						}
						else {
							Vector2 edgeDirection = new Vector2(localTargetPositionXZ.x >= 0 ? SinRadian : -SinRadian, CosRadian);
							float distanceToEdgeSqr = SqrDistanceToLine(edgeDirection, _HAFanInnerRadius, _HAFanOuterRadius, localTargetPositionXZ);
							return distanceToEdgeSqr <= targetSize * targetSize;
						}
					}
				}
				//------------------------------------------------------------------------------
				else if (_HitAreaType == HitAreaType.Line) {
					float distanceToLineSqr = SqrDistanceToLine(new Vector2(0.0f, 1.0f), _HALineStart, _HALineEnd, localTargetPositionXZ);
					return distanceToLineSqr <= targetSize * targetSize;
				}
				//------------------------------------------------------------------------------
			}
			return false;
		}

		void SetEffectOntoTarget(BattleUnit battleUnit) {
			if (battleUnit == null) {
				return;
			}
		}


		//////////////////////////////
		////    Help Functions    ///
		////////////////////////////
		#region [ Help Functions ]
		/// <summary>
		/// 点到经过原点的直线距离的平方
		/// </summary>
		/// <param name="direction">normalize化方向</param>
		/// <param name="start">起点位置(Min)</param>
		/// <param name="end">终点位置(Max)</param>
		/// <param name="position">测量对象坐标</param>
		/// <returns></returns>
		float SqrDistanceToLine(Vector2 direction, float start, float end, Vector2 position) {
			float DdotP = Vector2.Dot(direction, position);
			Vector2 nearestPosition = Mathf.Clamp(DdotP, start, end) * direction;
			return (position - nearestPosition).sqrMagnitude;
		}
		#endregion

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
			Gizmos.color = Color.yellow;
			Vector3 o = transform.position;
			Gizmos.DrawLine(new Vector3(o.x - 0.1f, o.y, o.z), new Vector3(o.x + 0.1f, o.y, o.z));
			Gizmos.DrawLine(new Vector3(o.x, o.y - 0.1f, o.z), new Vector3(o.x, o.y + 0.1f, o.z));
			Gizmos.DrawLine(new Vector3(o.x, o.y, o.z - 0.1f), new Vector3(o.x, o.y, o.z + 0.1f));
			if (_HitAreaType != HitAreaType.None) {
				Gizmos.color = Color.red;
				float y = 0f;
				Matrix4x4 localToWorldMatrix = transform.localToWorldMatrix;
				if (_HitAreaType == HitAreaType.Line) {
					Gizmos.DrawLine(localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, y, _HALineStart)), localToWorldMatrix.MultiplyPoint3x4(new Vector3(0f, y, _HALineEnd)));
				}
				else if (_HitAreaType == HitAreaType.Rectangle) {
					Vector3 v0 = localToWorldMatrix.MultiplyPoint3x4(new Vector3(-_HARecSize.x, y, -_HARecSize.y));
					Vector3 v1 = localToWorldMatrix.MultiplyPoint3x4(new Vector3(-_HARecSize.x, y, _HARecSize.y));
					Vector3 v2 = localToWorldMatrix.MultiplyPoint3x4(new Vector3(_HARecSize.x, y, _HARecSize.y));
					Vector3 v3 = localToWorldMatrix.MultiplyPoint3x4(new Vector3(_HARecSize.x, y, -_HARecSize.y));
					Gizmos.DrawLine(v0, v1);
					Gizmos.DrawLine(v1, v2);
					Gizmos.DrawLine(v2, v3);
					Gizmos.DrawLine(v3, v0);
				}
				else if (_HitAreaType == HitAreaType.Circle) {
					float radius = _HACircleRadius;
					Vector3[] points = new Vector3[GizmosCirclePointNum];
					for (int i = 0; i < GizmosCirclePointNum; i++) {
						points[i] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(GizmosCirclePointsCache[i].x * radius, y, GizmosCirclePointsCache[i].y * radius));
					}
					for (int i = 0; i < GizmosCirclePointNum - 1; i++) {
						Gizmos.DrawLine(points[i], points[i + 1]);
					}
					Gizmos.DrawLine(points[GizmosCirclePointNum - 1], points[0]);
				}
				else if (_HitAreaType == HitAreaType.Fan) {
					//edge
					float radian = _HAFanAngle / 180f * Mathf.PI;
					Vector2 dir = new Vector2(Mathf.Sin(radian), Mathf.Cos(radian));
					Vector3 edgePositiveStart = localToWorldMatrix.MultiplyPoint3x4(new Vector3(dir.x * _HAFanInnerRadius, y, dir.y * _HAFanInnerRadius));
					Vector3 edgePositiveEnd = localToWorldMatrix.MultiplyPoint3x4(new Vector3(dir.x * _HAFanOuterRadius, y, dir.y * _HAFanOuterRadius));
					Vector3 edgeNegativeStart = localToWorldMatrix.MultiplyPoint3x4(new Vector3(-dir.x * _HAFanInnerRadius, y, dir.y * _HAFanInnerRadius));
					Vector3 edgeNegativeEnd = localToWorldMatrix.MultiplyPoint3x4(new Vector3(-dir.x * _HAFanOuterRadius, y, dir.y * _HAFanOuterRadius));
					Gizmos.DrawLine(edgePositiveStart, edgePositiveEnd);
					Gizmos.DrawLine(edgeNegativeStart, edgeNegativeEnd);
					//arc
					int arcSegmentCount = Mathf.FloorToInt(_HAFanAngle / 180f * GizmosCirclePointNum / 2f) + 1;
					if (arcSegmentCount > 0) {
						Vector3[] arcInnerPositive = new Vector3[arcSegmentCount + 1];
						Vector3[] arcOuterPositive = new Vector3[arcSegmentCount + 1];
						Vector3[] arcInnerNegative = new Vector3[arcSegmentCount + 1];
						Vector3[] arcOuterNegative = new Vector3[arcSegmentCount + 1];
						for (int i = 0; i < arcSegmentCount; i++) {
							arcInnerPositive[i] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(GizmosCirclePointsCache[i].x * _HAFanInnerRadius, y, GizmosCirclePointsCache[i].y * _HAFanInnerRadius));
							arcOuterPositive[i] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(GizmosCirclePointsCache[i].x * _HAFanOuterRadius, y, GizmosCirclePointsCache[i].y * _HAFanOuterRadius));
							arcInnerNegative[i] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(-GizmosCirclePointsCache[i].x * _HAFanInnerRadius, y, GizmosCirclePointsCache[i].y * _HAFanInnerRadius));
							arcOuterNegative[i] = localToWorldMatrix.MultiplyPoint3x4(new Vector3(-GizmosCirclePointsCache[i].x * _HAFanOuterRadius, y, GizmosCirclePointsCache[i].y * _HAFanOuterRadius));
						}
						arcInnerPositive[arcSegmentCount] = edgePositiveStart;
						arcOuterPositive[arcSegmentCount] = edgePositiveEnd;
						arcInnerNegative[arcSegmentCount] = edgeNegativeStart;
						arcOuterNegative[arcSegmentCount] = edgeNegativeEnd;
						for (int i = 0; i < arcSegmentCount; i++) {
							Gizmos.DrawLine(arcInnerPositive[i], arcInnerPositive[i + 1]);
							Gizmos.DrawLine(arcOuterPositive[i], arcOuterPositive[i + 1]);
							Gizmos.DrawLine(arcInnerNegative[i], arcInnerNegative[i + 1]);
							Gizmos.DrawLine(arcOuterNegative[i], arcOuterNegative[i + 1]);
						}
					}
				}
			}
			Gizmos.color = colorCache;
		}
#endif
		#endregion
	}
}