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

		public TemplateAreaEditorCache(List<Int2> areaCellLocationsCache, List<Int2> areaEntryLocationsCache, List<Vector3> areaOutlineGUICache) {
			_AreaCellLocationsCache = areaCellLocationsCache;
			_AreaEntryLocationsCache = areaEntryLocationsCache;
			_AreaOutlineGUICache = areaOutlineGUICache;
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
		/// <summary>
		/// 设置模板尺寸
		/// </summary>
		/// <param name="range">尺寸Int4(min_x, min_y, max_x, max_y)</param>
		public void SetTemplateSize(Int2 Size) {
			_TemplateSize = Size;
		}
		/// <summary>
		/// 拖动添加新的模块
		/// </summary>
		/// <param name="battleMapArea">模块</param>
		/// <param name="location">位置</param>
		public void DragAddNewArea(BattleMapArea battleMapArea, Int2 location) {
			if (battleMapArea == null) {
				return;
			}
			Debug.Log("Test Action: Add " + battleMapArea.name + " to [" + location.x + ", " + location.y + "]");
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
#endif
		#endregion
	}
}
