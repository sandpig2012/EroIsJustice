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
		[SerializeField] private Int2 _TemplateSize = new Int2(40, 40); //min_x, min_y, max_x, max_y
		public Int2 TemplateSize { get { return _TemplateSize; } }
		[SerializeField] private List<BattleMapArea> _Areas = new List<BattleMapArea>();
		public List<BattleMapArea> Areas { get { return _Areas; } }
		[SerializeField] private List<Int2> _AreaLocations = new List<Int2>();
		public List<Int2> AreaLocations { get { return _AreaLocations; } }
		[SerializeField] private List<AreaRotation> _AreaRotations = new List<AreaRotation>();
		public List<AreaRotation> AreaRotations { get { return _AreaRotations; } }

#if UNITY_EDITOR
		[SerializeField] private List<List<Int2>> _AreaCellLocationsCaches = new List<List<Int2>>();
		public List<List<Int2>> AreaCellLocationsCaches { get { return _AreaCellLocationsCaches; } }
		[SerializeField] private List<List<Int2>> _AreaEntryLocationsCaches = new List<List<Int2>>();
		public List<List<Int2>> AreaEntryLocationsCaches { get { return _AreaEntryLocationsCaches; } }
		[SerializeField] private List<List<Vector3>> _AreaOutlineGUICaches = new List<List<Vector3>>();
		public List<List<Vector3>> AreaOutlineGUICaches { get { return _AreaOutlineGUICaches; } }
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
				_AreaCellLocationsCaches.Add(new List<Int2>() {
					new Int2(0, 0) + location,
					new Int2(-1, 0) + location,
					new Int2(-1, -1) + location,
					new Int2(0, -1) + location
				});
				_AreaEntryLocationsCaches.Add(new List<Int2>());
				_AreaBounds.Add(new Int4(location.x - 1, location.y - 1, 2, 2));
				List<Vector3> areaOutlineGUICaches = new List<Vector3>();
				///////////////////////////
				////    GUI Outline    ///
				/////////////////////////
				#region [ GUI Outline ]
				//left
				areaOutlineGUICaches.Add(new Vector3(location.x - 1, location.y + 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1, location.y - 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1 + GUIOutlineThickness, location.y + 1 - GUIOutlineThickness, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1 + GUIOutlineThickness, location.y - 1 + GUIOutlineThickness, 0f));
				//right
				areaOutlineGUICaches.Add(new Vector3(location.x + 1, location.y - 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1, location.y + 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1 - GUIOutlineThickness, location.y - 1 + GUIOutlineThickness, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1 - GUIOutlineThickness, location.y + 1 - GUIOutlineThickness, 0f));
				//up
				areaOutlineGUICaches.Add(new Vector3(location.x - 1, location.y + 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1, location.y + 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1 + GUIOutlineThickness, location.y + 1 - GUIOutlineThickness, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1 - GUIOutlineThickness, location.y + 1 - GUIOutlineThickness, 0f));
				//down
				areaOutlineGUICaches.Add(new Vector3(location.x - 1, location.y - 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1, location.y - 1, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x - 1 + GUIOutlineThickness, location.y - 1 + GUIOutlineThickness, 0f));
				areaOutlineGUICaches.Add(new Vector3(location.x + 1 - GUIOutlineThickness, location.y - 1 + GUIOutlineThickness, 0f));
				#endregion
				_AreaOutlineGUICaches.Add(areaOutlineGUICaches);
			}
			else {
				_UniqueIDs.Add(uniqueID);
				Int2 boundsMin = location;
				Int2 boundsMax = location;
				List<Int2> cellLocationsCache = new List<Int2>();
				List<Int2> entryLocationsCache = new List<Int2>();
				List<Vector3> areaOutlineGUICaches = new List<Vector3>();
				int numCell = battleMapArea.CellHolderLocations.Count;
				Int2[] fourDir = new Int2[4]; //left, right, up, down
				bool outlineLeft, outlineRight, outlineUp, outlineDown;
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
						fourDir[0] = new Int2(cell.x - 1, cell.y);
						fourDir[1] = new Int2(cell.x + 1, cell.y);
						fourDir[2] = new Int2(cell.x, cell.y + 1);
						fourDir[3] = new Int2(cell.x, cell.y - 1);
						outlineLeft = !battleMapArea.CellHolderLocations.Contains(fourDir[0]);
						outlineRight = !battleMapArea.CellHolderLocations.Contains(fourDir[1]);
						outlineUp = !battleMapArea.CellHolderLocations.Contains(fourDir[2]);
						outlineDown = !battleMapArea.CellHolderLocations.Contains(fourDir[3]);
						if (outlineLeft) {
							Vector3[] quad = new Vector3[] {
								new Vector3(loc.x, loc.y, 0f),
								new Vector3(loc.x, loc.y + 1, 0f),
								new Vector3(loc.x + GUIOutlineThickness, outlineDown ? loc.y + GUIOutlineThickness : loc.y, 0f),
								new Vector3(loc.x + GUIOutlineThickness, outlineUp ? loc.y + 1 - GUIOutlineThickness: loc.y + 1, 0f)
							};
							foreach (Vector3 vertex in quad) {
								areaOutlineGUICaches.Add(vertex);
							}
						}
						if (outlineRight) {
							Vector3[] quad = new Vector3[] {
								new Vector3(loc.x + 1, loc.y + 1, 0f),
								new Vector3(loc.x + 1, loc.y, 0f),
								new Vector3(loc.x + 1 - GUIOutlineThickness, outlineUp ? loc.y + 1 - GUIOutlineThickness : loc.y + 1, 0f),
								new Vector3(loc.x + 1 - GUIOutlineThickness, outlineDown ? loc.y + GUIOutlineThickness: loc.y, 0f)
							};
							foreach (Vector3 vertex in quad) {
								areaOutlineGUICaches.Add(vertex);
							}
						}
						if (outlineUp) {
							Vector3[] quad = new Vector3[] {
								new Vector3(loc.x, loc.y + 1, 0f),
								new Vector3(loc.x + 1, loc.y + 1, 0f),
								new Vector3(outlineLeft ? loc.x + GUIOutlineThickness : loc.x, loc.y + 1 - GUIOutlineThickness),
								new Vector3(outlineRight ? loc.x + 1 - GUIOutlineThickness : loc.x + 1, loc.y + 1 - GUIOutlineThickness),
							};
							foreach (Vector3 vertex in quad) {
								areaOutlineGUICaches.Add(vertex);
							}
						}
						if (outlineDown) {
							Vector3[] quad = new Vector3[] {
								new Vector3(loc.x + 1, loc.y + 1, 0f),
								new Vector3(loc.x, loc.y + 1, 0f),
								new Vector3(outlineRight ? loc.x + 1 - GUIOutlineThickness : loc.x + 1, loc.y + GUIOutlineThickness),
								new Vector3(outlineLeft ? loc.x + GUIOutlineThickness : loc.x, loc.y + GUIOutlineThickness),
							};
							foreach (Vector3 vertex in quad) {
								areaOutlineGUICaches.Add(vertex);
							}
						}
						#endregion
					}
				}
				_AreaCellLocationsCaches.Add(cellLocationsCache);
				_AreaEntryLocationsCaches.Add(entryLocationsCache);
				_AreaOutlineGUICaches.Add(areaOutlineGUICaches);
				_AreaBounds.Add(new Int4(boundsMin.x, boundsMin.y, boundsMax.x - boundsMin.x + 1, boundsMax.y - boundsMin.y + 1));
			}
		}
#endif
	}
}
