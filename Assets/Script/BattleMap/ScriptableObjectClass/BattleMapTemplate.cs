using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EIJ.BattleMap {

#if UNITY_EDITOR
	[Serializable]
	public class TemplateAreaEditorCache {
		[SerializeField] private List<Int2> _AreaCellLocationsCache = new List<Int2>();
		public List<Int2> AreaCellLocationsCache { get { return _AreaCellLocationsCache; } }
		[SerializeField] private List<Int2> _AreaEntryLocationsCache = new List<Int2>();
		public List<Int2> AreaEntryLocationsCache { get { return _AreaEntryLocationsCache; } }
		[SerializeField] private List<Vector3> _AreaOutlineGUICache = new List<Vector3>();
		public List<Vector3> AreaOutlineGUICache { get { return _AreaOutlineGUICache; } }
		/// <summary>
		/// 新建编辑器缓存
		/// </summary>
		/// <param name="areaCellLocationsCache">所有单元格位置</param>
		/// <param name="areaEntryLocationsCache">入口位置</param>
		/// <param name="areaOutlineGUICache">轮廓线缓存</param>
		public TemplateAreaEditorCache(List<Int2> areaCellLocationsCache, List<Int2> areaEntryLocationsCache, List<Vector3> areaOutlineGUICache) {
			_AreaCellLocationsCache = areaCellLocationsCache;
			_AreaEntryLocationsCache = areaEntryLocationsCache;
			_AreaOutlineGUICache = areaOutlineGUICache;
		}
		/// <summary>
		/// 应用平移
		/// </summary>
		/// <param name="offset">平移量</param>
		public void UpdateByLocationOffset(Int2 offset) {
			for (int i = 0; i < _AreaCellLocationsCache.Count; i++) {
				_AreaCellLocationsCache[i] += offset;
			}
			for (int i = 0; i < _AreaEntryLocationsCache.Count; i++) {
				_AreaEntryLocationsCache[i] += offset;
			}
			for (int i = 0; i < _AreaOutlineGUICache.Count; i++) {
				_AreaOutlineGUICache[i] += new Vector3(offset.x, offset.y, 0f);
			}
		}
		/// <summary>
		/// 应用90°旋转
		/// </summary>
		/// <param name="center">中心点</param>
		/// <param name="clockwise">是否为顺时针</param>
		public void UpdateByRotation(Int2 center, bool clockwise) {
			for (int i = 0; i < _AreaCellLocationsCache.Count; i++) {
				_AreaCellLocationsCache[i] = RotateAround(_AreaCellLocationsCache[i], center, clockwise);
			}
			for (int i = 0; i < _AreaEntryLocationsCache.Count; i++) {
				_AreaEntryLocationsCache[i] = RotateAround(_AreaEntryLocationsCache[i], center, clockwise);
			}
			for (int i = 0; i < _AreaOutlineGUICache.Count; i++) {
				_AreaOutlineGUICache[i] = RotateAround(_AreaOutlineGUICache[i], center, clockwise);
			}
		}
		static Int2 RotateAround(Int2 location, Int2 center, bool clockwise) {
			Int2 offset = location - center;
			if (clockwise) {
				return new Int2(offset.y, -offset.x - 1) + center;
			}
			else {
				return new Int2(-offset.y - 1, offset.x) + center;
			}
		}
		static Vector3 RotateAround(Vector3 location, Int2 center, bool clockwise) {
			Vector3 offset = new Vector3(location.x - center.x, location.y - center.y, 0f);
			if (clockwise) {
				return new Vector3(offset.y + center.x, -offset.x + center.y, 0f);
			}
			else {
				return new Vector3(-offset.y + center.x, offset.x + center.y, 0f);
			}
		}
	}
#endif


	/// <summary>
	/// 战斗地图模板ScriptableObject
	/// </summary>
	[CreateAssetMenu(fileName = "BattleMapTemplate", menuName = "Battle Map/Template", order = 551)]
	public class BattleMapTemplate : ScriptableObject {
		[SerializeField] private int[] _OverlapCache = new int[10000]; //100 x 100
		public int[] OverlapCache { get { return _OverlapCache; } }
		[SerializeField] private Int2 _TemplateSize = new Int2(40, 40); //min_x, min_y, max_x, max_y
		public Int2 TemplateSize { get { return _TemplateSize; } }
		[SerializeField] private List<BattleMapArea> _Areas = new List<BattleMapArea>();
		public List<BattleMapArea> Areas { get { return _Areas; } }
		[SerializeField] private List<Int2> _AreaLocations = new List<Int2>();
		public List<Int2> AreaLocations { get { return _AreaLocations; } }
		[SerializeField] private List<AreaRotation> _AreaRotations = new List<AreaRotation>();
		public List<AreaRotation> AreaRotations { get { return _AreaRotations; } }

		//////////////////////
		////    Editor    ///
		////////////////////
		#region [ Editor ]
#if UNITY_EDITOR

		[SerializeField] private List<TemplateAreaEditorCache> _AreaEditorCaches = new List<TemplateAreaEditorCache>();
		public List<TemplateAreaEditorCache> AreaEditorCaches { get { return _AreaEditorCaches; } }
		[SerializeField] private List<Int4> _AreaBounds = new List<Int4>(); //x, y, width, height
		[SerializeField] private List<string> _UniqueIDs = new List<string>();
		const float GUIOutlineThickness = 0.3f;
		[SerializeField] int _Selecting = -1;
		public int Selecting { get { return _Selecting; } set { _Selecting = value; } }

		private Int2 LastSelectedLocation = Int2.Zero;
		private List<int> LastSelectedList = new List<int>();

		/// <summary>
		/// 设置模板尺寸
		/// </summary>
		/// <param name="range">尺寸Int4(min_x, min_y, max_x, max_y)</param>
		public void SetTemplateSize(Int2 Size) {
			_TemplateSize = Size;
		}
		///////////////////////////////
		////    Import Control    ////
		/////////////////////////////
		#region [ Import Control ]
		/// <summary>
		/// 拖动添加新的模块
		/// </summary>
		/// <param name="battleMapArea">模块</param>
		/// <param name="location">位置</param>
		public void DragAddNewArea(BattleMapArea battleMapArea, Int2 location) {
			if (battleMapArea == null) {
				return;
			}
			_Areas.Add(battleMapArea);
			_AreaLocations.Add(location);
			_AreaRotations.Add(AreaRotation._0);
			string uniqueID = battleMapArea.UniqueID;
			if (uniqueID.Length <= 0) { //not completed
				_UniqueIDs.Add(string.Empty);
				List<Int2> areaCellLocationsCache = new List<Int2>() {
					new Int2(0, 0) + location,
					new Int2(-1, 0) + location,
					new Int2(-1, -1) + location,
					new Int2(0, -1) + location
				};
				List<Int2> areaEntryLocationsCache = new List<Int2>();
				_AreaBounds.Add(new Int4(location.x - 1, location.y - 1, 2, 2));
				List<Vector3> areaOutlineGUICaches = new List<Vector3>();
				///////////////////////////
				////    GUI Outline    ///
				/////////////////////////
				#region [ GUI Outline ]
				//left
				areaOutlineGUICaches.Add(new Vector3(location.x - 1, location.y + 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1, location.y - 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1 + GUIOutlineThickness, location.y - 1 + GUIOutlineThickness, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1 + GUIOutlineThickness, location.y + 1 - GUIOutlineThickness, 0f));
				//right
				areaOutlineGUICaches.Add(new Vector3(location.x + 1, location.y - 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1, location.y + 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1 - GUIOutlineThickness, location.y + 1 - GUIOutlineThickness, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1 - GUIOutlineThickness, location.y - 1 + GUIOutlineThickness, 0f));
				//up
				areaOutlineGUICaches.Add(new Vector3(location.x - 1, location.y + 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1, location.y + 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1 - GUIOutlineThickness, location.y + 1 - GUIOutlineThickness, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1 + GUIOutlineThickness, location.y + 1 - GUIOutlineThickness, 0f));
				//down
				areaOutlineGUICaches.Add(new Vector3(location.x - 1, location.y - 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1, location.y - 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1 - GUIOutlineThickness, location.y - 1 + GUIOutlineThickness, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1 + GUIOutlineThickness, location.y - 1 + GUIOutlineThickness, 0f));
				#endregion
				TemplateAreaEditorCache templateAreaEditorCache = new TemplateAreaEditorCache(areaCellLocationsCache, areaEntryLocationsCache, areaOutlineGUICaches);
				_AreaEditorCaches.Add(templateAreaEditorCache);
				foreach (Int2 cell in areaCellLocationsCache) {
					AddLocationToOverlapCache(cell);
				}
			}
			else {
				_UniqueIDs.Add(uniqueID);
				Int2 boundsMin = location;
				Int2 boundsMax = location;
				List<Int2> cellLocationsCache = new List<Int2>();
				List<Int2> entryLocationsCache = new List<Int2>();
				List<Vector3> areaOutlineGUICaches = new List<Vector3>();
				int numCell = battleMapArea.CellHolderLocations.Count;
				Int2[] eightDir = new Int2[8]; //left, right, up, down //up-right, down-right, up-left, down-left
				bool outlineLeft, outlineRight, outlineUp, outlineDown;
				bool cellUpRight, cellDownRight, cellUpLeft, cellDownLeft;
				for (int i = 0; i < numCell; i++) {
					Int2 cell = battleMapArea.CellHolderLocations[i];
					Int2 loc = (cell + location);
					if (loc.x < boundsMin.x) {
						boundsMin.x = loc.x;
					}
					else if (loc.x > boundsMax.x) {
						boundsMax.x = loc.x;
					}
					if (loc.y < boundsMin.y) {
						boundsMin.y = loc.y;
					}
					else if (loc.y > boundsMax.y) {
						boundsMax.y = loc.y;
					}
					cellLocationsCache.Add(loc);
					if (battleMapArea.VariantCellHolderTypes[i] == VariantCellHolderType.Entry) {
						entryLocationsCache.Add(loc);
					}
					///////////////////////////
					////    GUI Outline    ///
					/////////////////////////
					#region [ GUI Outline ]
					if (battleMapArea.VariantCellHolderTypes[i] == VariantCellHolderType.Wall) {
						eightDir[0] = new Int2(cell.x - 1, cell.y);
						eightDir[1] = new Int2(cell.x + 1, cell.y);
						eightDir[2] = new Int2(cell.x, cell.y + 1);
						eightDir[3] = new Int2(cell.x, cell.y - 1);
						eightDir[4] = new Int2(cell.x + 1, cell.y + 1);
						eightDir[5] = new Int2(cell.x + 1, cell.y - 1);
						eightDir[6] = new Int2(cell.x - 1, cell.y + 1);
						eightDir[7] = new Int2(cell.x - 1, cell.y - 1);
						outlineLeft = !battleMapArea.CellHolderLocations.Contains(eightDir[0]);
						outlineRight = !battleMapArea.CellHolderLocations.Contains(eightDir[1]);
						outlineUp = !battleMapArea.CellHolderLocations.Contains(eightDir[2]);
						outlineDown = !battleMapArea.CellHolderLocations.Contains(eightDir[3]);
						cellUpRight = battleMapArea.CellHolderLocations.Contains(eightDir[4]);
						cellDownRight = battleMapArea.CellHolderLocations.Contains(eightDir[5]);
						cellUpLeft = battleMapArea.CellHolderLocations.Contains(eightDir[6]);
						cellDownLeft = battleMapArea.CellHolderLocations.Contains(eightDir[7]);
						if (outlineLeft) {
							Vector3[] quad = new Vector3[] {
								new Vector3(loc.x, loc.y),
								new Vector3(loc.x, loc.y + 1),
								new Vector3(loc.x + GUIOutlineThickness,
									outlineUp ? loc.y + 1 - GUIOutlineThickness : (cellUpLeft ? loc.y + 1 + GUIOutlineThickness : loc.y + 1)),
								new Vector3(loc.x + GUIOutlineThickness,
									outlineDown ? loc.y + GUIOutlineThickness : (cellDownLeft ? loc.y - GUIOutlineThickness : loc.y)),
							};
							foreach (Vector3 vertex in quad) {
								areaOutlineGUICaches.Add(vertex);
							}
						}
						if (outlineRight) {
							Vector3[] quad = new Vector3[] {
								new Vector3(loc.x + 1, loc.y + 1),
								new Vector3(loc.x + 1, loc.y),
								new Vector3(loc.x + 1 - GUIOutlineThickness,
									outlineDown ? loc.y + GUIOutlineThickness : (cellDownRight ? loc.y - GUIOutlineThickness : loc.y)),
								new Vector3(loc.x + 1 - GUIOutlineThickness,
									outlineUp ? loc.y + 1 - GUIOutlineThickness : (cellUpRight ? loc.y + 1 + GUIOutlineThickness : loc.y + 1)),
							};
							foreach (Vector3 vertex in quad) {
								areaOutlineGUICaches.Add(vertex);
							}
						}
						if (outlineUp) {
							Vector3[] quad = new Vector3[] {
								new Vector3(loc.x, loc.y + 1),
								new Vector3(loc.x + 1, loc.y + 1),
								new Vector3(outlineRight ? loc.x + 1 - GUIOutlineThickness : (cellUpRight ? loc.x + 1 + GUIOutlineThickness : loc.x + 1),
									loc.y + 1 - GUIOutlineThickness),
								new Vector3(outlineLeft ? loc.x + GUIOutlineThickness : (cellUpLeft ? loc.x - GUIOutlineThickness : loc.x),
									loc.y + 1 - GUIOutlineThickness),
							};
							foreach (Vector3 vertex in quad) {
								areaOutlineGUICaches.Add(vertex);
							}
						}
						if (outlineDown) {
							Vector3[] quad = new Vector3[] {
								new Vector3(loc.x, loc.y),
								new Vector3(loc.x + 1, loc.y),
								new Vector3(outlineRight ? loc.x + 1 - GUIOutlineThickness : (cellDownRight ? loc.x + 1 + GUIOutlineThickness : loc.x + 1),
									loc.y + GUIOutlineThickness),
								new Vector3(outlineLeft ? loc.x + GUIOutlineThickness : (cellDownLeft ? loc.x - GUIOutlineThickness : loc.x),
									loc.y + GUIOutlineThickness),
							};
							foreach (Vector3 vertex in quad) {
								areaOutlineGUICaches.Add(vertex);
							}
						}
						#endregion
					}
				}
				TemplateAreaEditorCache templateAreaEditorCache = new TemplateAreaEditorCache(cellLocationsCache, entryLocationsCache, areaOutlineGUICaches);
				_AreaEditorCaches.Add(templateAreaEditorCache);
				_AreaBounds.Add(new Int4(boundsMin.x, boundsMin.y, boundsMax.x - boundsMin.x + 1, boundsMax.y - boundsMin.y + 1));
				foreach (Int2 cell in cellLocationsCache) {
					AddLocationToOverlapCache(cell);
				}
			}
		}
		void AddLocationToOverlapCache(Int2 location) {
			if (location.x < 0 || location.x > 99 || location.y < 0 || location.y > 99) {
				return;
			}
			_OverlapCache[location.x + location.y * 100]++;
		}
		void RemoveLocationFromOverlapCache(Int2 location) {
			if (location.x < 0 || location.x > 99 || location.y < 0 || location.y > 99) {
				return;
			}
			_OverlapCache[location.x + location.y * 100]--;
		}
		/// <summary>
		/// 检测是否有模板更新或被删除
		/// </summary>
		/// <returns></returns>
		public bool CheckAreaChanged() {
			for (int i = 0; i < _Areas.Count; i++) {
				if (_Areas[i] == null) {
					return true;
				}
				if (_UniqueIDs[i] != _Areas[i].UniqueID) {
					return true;
				}
			}
			return false;
		}
		/// <summary>
		/// 更新所有产生变化的模块
		/// </summary>
		public void CheckUpdateAndReimport() {
			List<BattleMapArea> areaToReimport = new List<BattleMapArea>();
			List<Int2> locationToReimport = new List<Int2>();
			List<AreaRotation> rotationToReimport = new List<AreaRotation>();
			for (int i = 0; i < _Areas.Count; i++) {
				if (_Areas[i] == null) {
					_Areas.RemoveAt(i);
					foreach (Int2 cell in _AreaEditorCaches[i].AreaCellLocationsCache) {
						RemoveLocationFromOverlapCache(cell);
					}
					_AreaEditorCaches.RemoveAt(i);
					_AreaBounds.RemoveAt(i);
					_AreaLocations.RemoveAt(i);
					_AreaRotations.RemoveAt(i);
					_UniqueIDs.RemoveAt(i);
					i--;
					continue;
				}
				if (_Areas[i].UniqueID != _UniqueIDs[i]) {
					areaToReimport.Add(_Areas[i]);
					locationToReimport.Add(_AreaLocations[i]);
					rotationToReimport.Add(_AreaRotations[i]);
					_Areas.RemoveAt(i);
					foreach (Int2 cell in _AreaEditorCaches[i].AreaCellLocationsCache) {
						RemoveLocationFromOverlapCache(cell);
					}
					_AreaEditorCaches.RemoveAt(i);
					_AreaBounds.RemoveAt(i);
					_AreaLocations.RemoveAt(i);
					_AreaRotations.RemoveAt(i);
					_UniqueIDs.RemoveAt(i);
					i--;
					continue;
				}
			}
			int numReimport = areaToReimport.Count;
			for (int i = 0; i < numReimport; i++) {
				DragAddNewArea(areaToReimport[i], locationToReimport[i]);
				_Selecting = _Areas.Count - 1;
				for (int j = 0; j < (int)rotationToReimport[i]; j++) {
					RotateSelecting(true);
				}
			}
			_Selecting = -1;
		}
		#endregion
		//////////////////////////////
		////    Select Control    ///
		////////////////////////////
		#region [ Select Control ]
		/// <summary>
		/// 鼠标点下时的选择反馈
		/// </summary>
		/// <param name="clickPosition">点击位置</param>
		public bool MouseDownSelect(Vector2 clickPosition) {
			int numArea = _Areas.Count;
			Int4 currentBounds;
			bool repeatClick = _Selecting >= 0 && LastSelectedLocation.Contain(clickPosition);
			if (repeatClick) {
				return false;
			}
			for (int i = 0; i < numArea; i++) {
				currentBounds = _AreaBounds[i];
				if (currentBounds.Contain(clickPosition)) {
					foreach (Int2 cell in _AreaEditorCaches[i].AreaCellLocationsCache) {
						if (cell.Contain(clickPosition)) {
							_Selecting = i;
							LastSelectedLocation = cell;
							LastSelectedList.Clear();
							LastSelectedList.Add(i);
							return true;
						}
					}
				}
			}
			_Selecting = -1;
			LastSelectedLocation = Int2.Zero;
			LastSelectedList.Clear();
			return true;
		}
		/// <summary>
		/// 选择重复位置时在鼠标抬起时切换选择
		/// </summary>
		/// <param name="clickPosition"></param>
		public void MouseUpSelect(Vector2 clickPosition) {
			int numArea = _Areas.Count;
			Int4 currentBounds;
			bool repeatClick = _Selecting >= 0 && LastSelectedLocation.Contain(clickPosition);
			if (!repeatClick) {
				return;
			}
			for (int i = 0; i < numArea; i++) {
				if (LastSelectedList.Contains(i)) {
					continue;
				}
				currentBounds = _AreaBounds[i];
				if (currentBounds.Contain(clickPosition)) {
					foreach (Int2 cell in _AreaEditorCaches[i].AreaCellLocationsCache) {
						if (cell.Contain(clickPosition)) {
							_Selecting = i;
							LastSelectedList.Add(i);
							return;
						}
					}
				}
			}
			_Selecting = LastSelectedList[0];
			LastSelectedList.Clear();
			LastSelectedList.Add(_Selecting);
		}
		/// <summary>
		/// 右键选中模块源文件
		/// </summary>
		/// <param name="clickPosition">点击位置</param>
		/// <returns></returns>
		public BattleMapArea MouseRightClickSelect(Vector2 clickPosition) {
			if(_Selecting >= 0) {
				bool clickOnSelecting = false;
				foreach(Int2 cell in _AreaEditorCaches[_Selecting].AreaCellLocationsCache) {
					if (cell.Contain(clickPosition)) {
						clickOnSelecting = true;
						break;
					}
				}
				if (clickOnSelecting) {
					return _Areas[_Selecting];
				}
			}
			int numArea = _Areas.Count;
			Int4 currentBounds;
			for (int i = 0; i < numArea; i++) {
				currentBounds = _AreaBounds[i];
				if (currentBounds.Contain(clickPosition)) {
					foreach (Int2 cell in _AreaEditorCaches[i].AreaCellLocationsCache) {
						if (cell.Contain(clickPosition)) {
							_Selecting = i;
							LastSelectedLocation = cell;
							LastSelectedList.Clear();
							LastSelectedList.Add(i);
							return _Areas[i];
						}
					}
				}
			}
			return null;
		}
		/// <summary>
		/// 清空控制选择的变量
		/// </summary>
		public void CancleSelecting() {
			_Selecting = -1;
			LastSelectedLocation = Int2.Zero;
			LastSelectedList.Clear();
		}
		#endregion
		//////////////////////////////
		////    Select Control    ///
		////////////////////////////
		#region
		/// <summary>
		/// 移动选中模块
		/// </summary>
		/// <param name="newLocation">目标位置</param>
		public void MoveSelectingAreaTo(Int2 newLocation) {
			if (_Selecting < 0) {
				return;
			}
			if (_AreaLocations[_Selecting] == newLocation) {
				return;
			}
			TemplateAreaEditorCache cache = _AreaEditorCaches[_Selecting];
			foreach (Int2 cell in cache.AreaCellLocationsCache) {
				RemoveLocationFromOverlapCache(cell);
			}
			Int4 oldBounds = _AreaBounds[_Selecting];
			Int2 offset = newLocation - _AreaLocations[_Selecting];
			_AreaBounds[_Selecting] = new Int4(oldBounds.x + offset.x, oldBounds.y + offset.y, oldBounds.z, oldBounds.w);
			_AreaLocations[_Selecting] = newLocation;
			cache.UpdateByLocationOffset(offset);
			foreach (Int2 cell in cache.AreaCellLocationsCache) {
				AddLocationToOverlapCache(cell);
			}
		}
		/// <summary>
		/// 旋转选中模块
		/// </summary>
		/// <param name="clockwise">顺时针</param>
		public void RotateSelecting(bool clockwise) {
			if (_Selecting < 0) {
				return;
			}
			TemplateAreaEditorCache cache = _AreaEditorCaches[_Selecting];
			foreach (Int2 cell in cache.AreaCellLocationsCache) {
				RemoveLocationFromOverlapCache(cell);
			}
			Int2 location = _AreaLocations[_Selecting];
			Int4 oldBounds = _AreaBounds[_Selecting];
			Int2 offset = new Int2(oldBounds.x - location.x, oldBounds.y - location.y);
			Int2 boundsPoint0 = new Int2((clockwise ? offset.y : -offset.y) + location.x, (clockwise ? -offset.x : offset.x) + location.y);
			offset = new Int2(oldBounds.x + oldBounds.z - location.x, oldBounds.y + oldBounds.w - location.y);
			Int2 boundsPoint1 = new Int2((clockwise ? offset.y : -offset.y) + location.x, (clockwise ? -offset.x : offset.x) + location.y);
			_AreaBounds[_Selecting] = new Int4(
				boundsPoint0.x <= boundsPoint1.x ? boundsPoint0.x : boundsPoint1.x,
				boundsPoint0.y <= boundsPoint1.y ? boundsPoint0.y : boundsPoint1.y,
				oldBounds.w, oldBounds.z);
			cache.UpdateByRotation(location, clockwise);
			AreaRotation currentRotation = _AreaRotations[_Selecting];
			if (clockwise) {
				int newRotationEnumInt = (int)currentRotation + 1;
				if (newRotationEnumInt > 3) {
					newRotationEnumInt = 0;
				}
				_AreaRotations[_Selecting] = (AreaRotation)newRotationEnumInt;
			}
			else {
				int newRotationEnumInt = (int)currentRotation - 1;
				if (newRotationEnumInt < 0) {
					newRotationEnumInt = 3;
				}
				_AreaRotations[_Selecting] = (AreaRotation)newRotationEnumInt;
			}
			foreach (Int2 cell in cache.AreaCellLocationsCache) {
				AddLocationToOverlapCache(cell);
			}
		}
		/// <summary>
		/// 删除模块
		/// </summary>
		/// <param name="index">索引</param>
		public void DeleteSelecting() {
			RemoveAreaAt(_Selecting);
			_Selecting = -1;
		}

		void RemoveAreaAt(int index) {
			if (index < 0 || index >= _Areas.Count) {
				return;
			}
			TemplateAreaEditorCache cache = _AreaEditorCaches[index];
			foreach (Int2 cell in cache.AreaCellLocationsCache) {
				RemoveLocationFromOverlapCache(cell);
			}
			_Areas.RemoveAt(index);
			_AreaEditorCaches.RemoveAt(index);
			_AreaBounds.RemoveAt(index);
			_UniqueIDs.RemoveAt(index);
			_AreaLocations.RemoveAt(index);
			_AreaRotations.RemoveAt(index);			
		}
		#endregion
#endif
		#endregion
	}
}
