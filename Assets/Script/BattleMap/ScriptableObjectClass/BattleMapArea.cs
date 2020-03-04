using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EIJ.BattleMap {
	public enum BattleMapAreaType {
		Normal = 0,
		Home = 1,
		Spawn = 2,
	}
	public enum CellAccessType {
		Path = 0,
		Entry = 1,
		Home = 2,
		Spawn = 3,
	}
	/// <summary>
	/// 战斗地图模板变种
	/// </summary>
	[Serializable]
	public class BattleMapAreaVariant {

		[SerializeField] private List<BattleMapPath> _Paths = new List<BattleMapPath>();
		public List<BattleMapPath> Paths { get { return _Paths; } }
		[SerializeField] private List<Int2> _PathCells = new List<Int2>();
		[SerializeField] private List<Int2> _PlatformCells = new List<Int2>();

		//////////////////////
		////    Editor    ///
		////////////////////
		#region [ Editor ]
#if UNITY_EDITOR
		[SerializeField] private List<Int2> _CellLocations = new List<Int2>();
		[SerializeField] private List<BattleMapCellType> _CellTypes = new List<BattleMapCellType>();
		[SerializeField] private float _AppearRate = 1.0f;
		public float AppearRate { get { return _AppearRate; } set { _AppearRate = Mathf.Clamp(value, 0.1f, 100.0f); } }
		[SerializeField] private bool _Compiled = false;
		public bool Compiled { get { return _Compiled; } }

		/////////////////////
		////    Paint    ///
		///////////////////
		#region [ Paint ]
		/// <summary>
		/// 增加基地单元
		/// </summary>
		/// <param name="location">位置</param>
		public void AddHome(Int2 location) {
			int homeIndex = _CellTypes.IndexOf(BattleMapCellType.Home);
			if (homeIndex >= 0) {
				_CellLocations.RemoveAt(homeIndex);
				_CellTypes.RemoveAt(homeIndex);
			}
			int index = _CellLocations.IndexOf(location);
			if (index >= 0) {
				_CellTypes[index] = BattleMapCellType.Home;
			}
			else {
				_CellLocations.Add(location);
				_CellTypes.Add(BattleMapCellType.Home);
			}
		}
		/// <summary>
		/// 增加刷怪点单元
		/// </summary>
		/// <param name="location">位置</param>
		public void AddSpawn(Int2 location) {
			int spawnIndex = _CellTypes.IndexOf(BattleMapCellType.Spawn);
			if (spawnIndex >= 0) {
				_CellLocations.RemoveAt(spawnIndex);
				_CellTypes.RemoveAt(spawnIndex);
			}
			int index = _CellLocations.IndexOf(location);
			if (index >= 0) {
				_CellTypes[index] = BattleMapCellType.Spawn;
			}
			else {
				_CellLocations.Add(location);
				_CellTypes.Add(BattleMapCellType.Spawn);
			}
		}
		/// <summary>
		/// 增加路径单元
		/// </summary>
		/// <param name="location">位置</param>
		public void AddPath(Int2 location) {
			int index = _CellLocations.IndexOf(location);
			if (index >= 0) _CellTypes[index] = BattleMapCellType.Path;
			else {
				_CellLocations.Add(location);
				_CellTypes.Add(BattleMapCellType.Path);
			}
		}
		/// <summary>
		/// 增加平台单元
		/// </summary>
		/// <param name="location">位置</param>
		public void AddPlatform(Int2 location) {
			int index = _CellLocations.IndexOf(location);
			if (index >= 0) _CellTypes[index] = BattleMapCellType.Platform;
			else {
				_CellLocations.Add(location);
				_CellTypes.Add(BattleMapCellType.Platform);
			}
		}
		/// <summary>
		/// 移除单元
		/// </summary>
		/// <param name="location">位置</param>
		public void RemoveCell(Int2 location) {
			int index = _CellLocations.IndexOf(location);
			if (index >= 0) {
				_CellLocations.RemoveAt(index);
				_CellTypes.RemoveAt(index);
			}
		}
		/// <summary>
		/// 获取单元信息
		/// </summary>
		/// <param name="cellCount">数量</param>
		/// <param name="locations">位置</param>
		/// <param name="types">类型</param>
		/// <returns>是否成功</returns>
		public bool GetInfo(out int cellCount, out Int2[] locations, out BattleMapCellType[] types) {
			cellCount = _CellLocations.Count;
			locations = new Int2[cellCount];
			types = new BattleMapCellType[cellCount];
			if (cellCount <= 0) return false;
			for (int i = 0; i < cellCount; i++) {
				locations[i] = _CellLocations[i];
				types[i] = _CellTypes[i];
			}
			return true;
		}
		#endregion
		///////////////////////
		////    Compile    ///
		/////////////////////
		#region [ Compile ]
		/// <summary>
		/// 确认并编译变种
		/// </summary>
		/// <param name="type">基础模板类型</param>
		/// <param name="entries">入口位置</param>
		/// <returns>错误信息，成功则为空</returns>
		public string Compile(BattleMapAreaType type, List<Int2> entries) {
			_Compiled = false;
			GetPathsAndPlatforms(out List<Int2> paths, out List<Int2> platforms, out List<Int2> home, out List<Int2> spawn);
			if (type == BattleMapAreaType.Home && home.Count != 1)
				return "【基地】类型模板需要至少一个【基地单元】";
			if (type == BattleMapAreaType.Spawn && spawn.Count != 1)
				return "【刷怪点】类型模板需要至少一个【刷怪点单元】";
			if (type == BattleMapAreaType.Normal && (home.Count > 0 || spawn.Count > 0))
				return "【普通】类型模板中不能存在【基地单元】或【刷怪点单元】";
			if (type == BattleMapAreaType.Normal && paths.Count <= 0)
				return "【普通】类型模板必须至少存在一个【路径单元】";
			List<Int2> allCells = new List<Int2>(paths);
			if (type == BattleMapAreaType.Home) {
				allCells.Add(home[0]);
			}
			else if (type == BattleMapAreaType.Spawn) {
				allCells.Add(spawn[0]);
			}
			foreach (Int2 entry in entries) {
				allCells.Add(entry);
			}
			if (!CheckAllPathsConnected(ref allCells))
				return "路径单元和关键单元没有全部连接！";
			List<CellAccessData> accessDatas = GenerateCellAccessData(ref allCells);
			if (type == BattleMapAreaType.Home) {
				string log = FindPathForHomeAndSpawnType(home[0], ref entries, ref accessDatas);
				if (log.Length > 0) {
					return log;
				}
				else {
					_Compiled = true;
				}
			}
			else if (type == BattleMapAreaType.Spawn) {
				string log = FindPathForHomeAndSpawnType(spawn[0], ref entries, ref accessDatas);
				if (log.Length > 0) {
					return log;
				}
				else {
					_Compiled = true;
				}
			}
			else if (type == BattleMapAreaType.Normal) {
				string log = FindPathForNormalType(ref entries, ref accessDatas);
				if (log.Length > 0) {
					return log;
				}
				else {
					_Compiled = true;
				}
			}
			RemoveUnusedPathCells();
			ApplyPathAndPlatformCells(ref platforms);
			return string.Empty;
		}
		void GetPathsAndPlatforms(out List<Int2> paths, out List<Int2> platforms, out List<Int2> home, out List<Int2> spawn) {
			paths = new List<Int2>();
			platforms = new List<Int2>();
			home = new List<Int2>();
			spawn = new List<Int2>();
			int count = _CellLocations.Count;
			for (int i = 0; i < count; i++) {
				Int2 location = _CellLocations[i];
				switch (_CellTypes[i]) {
				case BattleMapCellType.Path:
					paths.Add(location);
					break;
				case BattleMapCellType.Platform:
					platforms.Add(location);
					break;
				case BattleMapCellType.Home:
					home.Add(location);
					break;
				case BattleMapCellType.Spawn:
					spawn.Add(location);
					break;
				}
			}
		}
		bool CheckAllPathsConnected(ref List<Int2> paths) {
			if (paths.Count == 0) return true;
			List<Int2> listToCheck = new List<Int2>(paths);
			List<Int2> listNewFound = new List<Int2>();
			List<Int2> listChecked = new List<Int2>();
			listNewFound.Add(listToCheck[0]);
			listToCheck.RemoveAt(0);
			bool foundNew = true;
			Int2 checkedLocation;
			Int2[] fourDir = new Int2[4];
			int index;
			while (foundNew) {
				foundNew = false;
				listChecked.Clear();
				foreach (var location in listNewFound)
					listChecked.Add(location);
				listNewFound.Clear();
				for (int i = 0; i < listChecked.Count; i++) {
					checkedLocation = listChecked[i];
					fourDir[0] = new Int2(checkedLocation.x - 1, checkedLocation.y);
					fourDir[1] = new Int2(checkedLocation.x + 1, checkedLocation.y);
					fourDir[2] = new Int2(checkedLocation.x, checkedLocation.y + 1);
					fourDir[3] = new Int2(checkedLocation.x, checkedLocation.y - 1);
					for (int j = 0; j < 4; j++) {
						if ((index = listToCheck.IndexOf(fourDir[j])) >= 0) {
							foundNew = true;
							listNewFound.Add(listToCheck[index]);
							listToCheck.RemoveAt(index);
						}
					}
				}
			}
			return listToCheck.Count <= 0;
		}
		List<CellAccessData> GenerateCellAccessData(ref List<Int2> cells) {
			List<CellAccessData> cellAccessData = new List<CellAccessData>();
			Int2[] fourDirs = new Int2[4];
			foreach (Int2 cell in cells) {
				List<Int2> nextLocations = new List<Int2>();
				fourDirs[0] = new Int2(cell.x + 1, cell.y);
				fourDirs[1] = new Int2(cell.x - 1, cell.y);
				fourDirs[2] = new Int2(cell.x, cell.y + 1);
				fourDirs[3] = new Int2(cell.x, cell.y - 1);
				for (int i = 0; i < 4; i++) {
					if (cells.Contains(fourDirs[i])) {
						nextLocations.Add(fourDirs[i]);
					}
				}
				cellAccessData.Add(new CellAccessData(cell, nextLocations));
			}
			return cellAccessData;
		}
		string FindPathForHomeAndSpawnType(Int2 start, ref List<Int2> entries, ref List<CellAccessData> accessDatas) {
			foreach (Int2 entry in entries) {
				string log = FindPath(start, entry, ref accessDatas, out BattleMapPath pathFound);
				if (log.Length > 0) {
					_Paths.Clear();
					return log;
				}
				else {
					_Paths.Add(pathFound);
				}
			}
			return string.Empty;
		}
		string FindPathForNormalType(ref List<Int2> entries, ref List<CellAccessData> accessDatas) {
			int numEntry = entries.Count;
			for (int i = 0; i < numEntry - 1; i++) {
				for (int j = i + 1; j < numEntry; j++) {
					string log = FindPath(entries[i], entries[j], ref accessDatas, out BattleMapPath pathFound);
					if (log.Length > 0) {
						_Paths.Clear();
						return log;
					}
					else {
						_Paths.Add(pathFound);
					}
				}
			}
			return string.Empty;
		}
		string FindPath(Int2 start, Int2 end, ref List<CellAccessData> accessDatas, out BattleMapPath pathFound) {
			pathFound = new BattleMapPath();
			bool pathExist = false;
			List<List<Int2>> steps = new List<List<Int2>>();
			List<Int2> cellChecked = new List<Int2>() { start };
			steps.Add(new List<Int2>() { start });
			List<Int2> entryNext = GatherAccessableCells(start, ref accessDatas, false, Int2.Zero);
			foreach (Int2 next in entryNext) {
				cellChecked.Add(next);
			}
			steps.Add(entryNext);
			while (steps.Count > 0) {
				int step = steps.Count - 1;
				if (steps[step].Count > 0) {
					Int2 locataion = steps[step][0];
					if (locataion == end) {
						if (pathExist) {
							return "存在多重路径！";
						}
						else {
							List<Int2> currentPathFromStep = new List<Int2>();
							for (int i = 0; i < steps.Count - 1; i++) {
								currentPathFromStep.Add(steps[i][0]);
							}
							currentPathFromStep.Add(end);
							pathFound.PathLocations = currentPathFromStep;
							pathExist = true;
							steps[step].RemoveAt(0);
						}
					}
					else {
						bool hasPrevious = step >= 1;
						Int2 previous = hasPrevious ? steps[step - 1][0] : Int2.Zero;
						List<Int2> nextStep = GatherAccessableCells(locataion, ref accessDatas, hasPrevious, previous);
						foreach (Int2 next in nextStep) {
							if (cellChecked.Contains(next)) {
								return "存在多重路径";
							}
							else {
								cellChecked.Add(next);
							}
						}
						if (nextStep.Count > 0) {
							steps.Add(nextStep);
						}
						else {
							steps[step].RemoveAt(0);
						}
					}
				}
				else {
					steps.RemoveAt(step);
					if (step >= 1) {
						steps[step - 1].RemoveAt(0);
					}
				}
			}
			if (!pathExist) {
				return "存在未连通入口";
			}
			return string.Empty;
		}
		List<Int2> GatherAccessableCells(Int2 currentPosition, ref List<CellAccessData> accessDatas, bool hasPrevious, Int2 previous) {
			foreach (CellAccessData data in accessDatas) {
				if (data.Location == currentPosition) {
					List<Int2> nexts = new List<Int2>(data.Nexts);
					if (hasPrevious) {
						nexts.Remove(previous);
					}
					return nexts;
				}
			}
			return new List<Int2>();
		}
		void RemoveUnusedPathCells() {
			if (_Paths.Count <= 0) {
				return;
			}
			int numCell = _CellLocations.Count;
			for (int i = 0; i < numCell; i++) {
				BattleMapCellType currentType = _CellTypes[i];
				if (currentType != BattleMapCellType.Path) {
					continue;
				}
				Int2 currentLocation = _CellLocations[i];
				bool containedInAnyPath = false;
				foreach (BattleMapPath path in _Paths) {
					if (path.PathLocations.Contains(currentLocation)) {
						containedInAnyPath = true;
						break;
					}
				}
				if (!containedInAnyPath) {
					_CellLocations.RemoveAt(i);
					_CellTypes.RemoveAt(i);
					i--;
					numCell--;
				}
			}
		}
		void ApplyPathAndPlatformCells(ref List<Int2> platforms) {
			_PathCells.Clear();
			_PlatformCells.Clear();
			foreach (Int2 platform in platforms) {
				_PlatformCells.Add(platform);
			}
			int numCell = _CellLocations.Count;
			for (int i = 0; i < numCell; i++) {
				if (_CellTypes[i] == BattleMapCellType.Path) {
					_PathCells.Add(_CellLocations[i]);
				}
			}
		}
		/// <summary>
		/// 解锁变种以重新编辑
		/// </summary>
		public void Unlock() {
			_Compiled = false;
			_Paths.Clear();
			_PathCells.Clear();
			_PlatformCells.Clear();
		}
		#endregion
		//////////////////////
		////    Helper    ///
		////////////////////
		#region [ Helper ]
		/// <summary>
		/// 清空当前变种
		/// </summary>
		public void Clear() {
			_CellLocations.Clear();
			_CellTypes.Clear();
		}
		/// <summary>
		/// 设置单元位置及类型
		/// </summary>
		/// <param name="locations">坐标</param>
		/// <param name="types">战斗地推单元类型</param>
		public void SetLocationAndType(List<Int2> locations, List<BattleMapCellType> types) {
			Clear();
			foreach (var location in locations) _CellLocations.Add(location);
			foreach (var type in types) _CellTypes.Add(type);
		}
		/// <summary>
		/// 复制一个当前变种作为新的变种
		/// </summary>
		/// <returns></returns>
		public BattleMapAreaVariant CreateCopy() {
			BattleMapAreaVariant copy = new BattleMapAreaVariant();
			copy.SetLocationAndType(_CellLocations, _CellTypes);
			copy.AppearRate = _AppearRate;
			return copy;
		}
		/// <summary>
		/// 使当前变种匹配新的基础模板
		/// </summary>
		/// <param name="mapType">模板类型</param>
		/// <param name="cellHolderLocations">基础模板单元位置</param>
		/// <param name="cellHolderTypes">基础模板单元类型</param>
		public void MatchNewTemplate(BattleMapAreaType mapType, ref List<Int2> cellHolderLocations, ref List<VariantCellHolderType> cellHolderTypes) {
			int count = _CellLocations.Count;
			Int2 currentLocation;
			int currentIndex;
			BattleMapCellType currentCellType;
			VariantCellHolderType currentHolderType;
			for (int i = 0; i < count; i++) {
				currentLocation = _CellLocations[i];
				currentIndex = cellHolderLocations.IndexOf(currentLocation);
				if (currentIndex < 0) {
					_CellLocations.RemoveAt(i);
					_CellTypes.RemoveAt(i);
					count--;
					i--;
					continue;
				}
				else {
					currentHolderType = cellHolderTypes[currentIndex];
					currentCellType = _CellTypes[i];
					if (currentHolderType == VariantCellHolderType.Entry) {
						_CellLocations.RemoveAt(i);
						_CellTypes.RemoveAt(i);
						count--;
						i--;
						continue;
					}
					else if (currentHolderType == VariantCellHolderType.Wall) {
						if (currentCellType != BattleMapCellType.Platform) {
							_CellLocations.RemoveAt(i);
							_CellTypes.RemoveAt(i);
							count--;
							i--;
							continue;
						}
					}
					if (currentCellType == BattleMapCellType.Home && mapType != BattleMapAreaType.Home) {
						_CellLocations.RemoveAt(i);
						_CellTypes.RemoveAt(i);
						count--;
						i--;
						continue;
					}
					if (currentCellType == BattleMapCellType.Spawn && mapType != BattleMapAreaType.Spawn) {
						_CellLocations.RemoveAt(i);
						_CellTypes.RemoveAt(i);
						count--;
						i--;
						continue;
					}
				}
			}
		}
		#endregion
#endif
		#endregion
	}
	/// <summary>
	/// 战斗地图模块ScriptableObject
	/// </summary>
	[CreateAssetMenu(fileName = "BattleMapArea", menuName = "Battle Map/Area", order = 550)]
	public class BattleMapArea : ScriptableObject {

		[SerializeField] private BattleMapAreaType _Type = BattleMapAreaType.Normal;
		public BattleMapAreaType Type { get { return _Type; } set { _Type = value; } }
		[SerializeField] private List<Int2> _Entries = new List<Int2>();
		[SerializeField] private List<BattleMapAreaVariant> _Variants = new List<BattleMapAreaVariant>();
		public List<BattleMapAreaVariant> Variants { get { return _Variants; } }
		[SerializeField] private float[] _RateSplits = null;
		public float[] RateSplitPoints { get { return _RateSplits; } }

		//////////////////////
		////    Editor    ///
		////////////////////
		#region [ Editor ]
#if UNITY_EDITOR
		[SerializeField] private string _UniqueID = string.Empty;
		public string UniqueID { get { return _UniqueID; } }
		[SerializeField] private BattleMapAreaEditorProcessStep _Step = BattleMapAreaEditorProcessStep.Template;
		public bool ProcessCompleted { get { return _Step == BattleMapAreaEditorProcessStep.Completed; } }
		public bool ProcessTemplate { get { return _Step == BattleMapAreaEditorProcessStep.Template; } }
		public bool ProcessVarients { get { return _Step == BattleMapAreaEditorProcessStep.Varients; } }

		////////////////////////
		////    Template    ///
		//////////////////////
		#region [ Template ]
		[SerializeField] List<Int2> _HolderLocations = new List<Int2>();
		[SerializeField] List<BattleMapAreaHolderType> _HoldersTypes = new List<BattleMapAreaHolderType>();
		/// <summary>
		/// 获取模板单元信息
		/// </summary>
		/// <param name="count">单元数量</param>
		/// <param name="locations">单元位置</param>
		/// <param name="types">单元类型</param>
		/// <returns></returns>
		public bool GetTemplateInfo(out int count, out Int2[] locations, out BattleMapAreaHolderType[] types) {
			count = _HolderLocations.Count;
			locations = new Int2[count];
			types = new BattleMapAreaHolderType[count];
			if (count <= 0) return false;
			for (int i = 0; i < count; i++) {
				types[i] = _HoldersTypes[i];
				locations[i] = _HolderLocations[i];
			}
			return true;
		}
		/// <summary>
		/// 增加一个新的模板基础单元
		/// </summary>
		/// <param name="location">位置</param>
		/// <param name="type">类型</param>
		public void AddBattleMapAreaHolder(Int2 location, BattleMapAreaHolderType type) {
			int index = _HolderLocations.IndexOf(location);
			if (index >= 0) {
				_HoldersTypes[index] = type;
			}
			else {
				_HolderLocations.Add(location);
				_HoldersTypes.Add(type);
			}
		}
		/// <summary>
		/// 移除一个已有的模板基础单元
		/// </summary>
		/// <param name="location">位置</param>
		public void RemoveBattleMapAreaHolder(Int2 location) {
			int index = _HolderLocations.IndexOf(location);
			if (index >= 0) {
				_HolderLocations.RemoveAt(index);
				_HoldersTypes.RemoveAt(index);
			}
		}
		/// <summary>
		/// 清除一个模板基础单元
		/// </summary>
		public void ClearAllHolders() {
			_HolderLocations.Clear();
			_HoldersTypes.Clear();
		}
		#endregion
		///////////////////////////////////
		////    Template -> Varients   ///
		/////////////////////////////////
		#region [ Template -> Varients  ]
		/// <summary>
		/// 确认并使用模板
		/// </summary>
		/// <returns></returns>
		public string ApplyTemplate() {
			int holderCount = _HolderLocations.Count;
			if (holderCount < 7) return "区域过小！";
			List<Int2> entries, walls, insides;
			GetConstuct(out entries, out walls, out insides);
			if (insides.Count <= 0) return "没有位置放置路径单元！";
			if (_Type != BattleMapAreaType.Normal) {
				if (entries.Count < 1) return "【基地】或【刷怪点】模板必须拥有至少一个入口！";
			}
			else if (entries.Count < 2) return "【普通】模板必须拥有至少两个入口！";

			if (!CheckAllInsidesConnected(ref insides)) return "内部单元格没有完全连通！";
			if (!CheckEntryValid(ref insides)) return "存在无效的入口！";

			_CellHolderLocations.Clear();
			_VariantCellHolderTypes.Clear();
			foreach (var location in insides) {
				_CellHolderLocations.Add(location);
				_VariantCellHolderTypes.Add(VariantCellHolderType.Inside);
			}
			foreach (var location in walls) {
				_CellHolderLocations.Add(location);
				_VariantCellHolderTypes.Add(VariantCellHolderType.Wall);
			}
			foreach (var location in entries) {
				_CellHolderLocations.Add(location);
				_VariantCellHolderTypes.Add(VariantCellHolderType.Entry);
			}
			foreach (var variant in _Variants) {
				variant.MatchNewTemplate(_Type, ref _CellHolderLocations, ref _VariantCellHolderTypes);
			}
			_Step = BattleMapAreaEditorProcessStep.Varients;
			return string.Empty;
		}
		void GetConstuct(out List<Int2> entries, out List<Int2> walls, out List<Int2> insides) {
			entries = new List<Int2>();
			walls = new List<Int2>();
			insides = new List<Int2>();
			Int2[] fourDir = new Int2[4];
			int count = _HolderLocations.Count;
			bool isWall;
			for (int i = 0; i < count; i++) {
				Int2 currentLocation = _HolderLocations[i];
				if (_HoldersTypes[i] == BattleMapAreaHolderType.Entry) {
					entries.Add(currentLocation);
				}
				else {
					fourDir[0] = new Int2(currentLocation.x - 1, currentLocation.y);
					fourDir[1] = new Int2(currentLocation.x + 1, currentLocation.y);
					fourDir[2] = new Int2(currentLocation.x, currentLocation.y + 1);
					fourDir[3] = new Int2(currentLocation.x, currentLocation.y - 1);
					isWall = false;
					for (int j = 0; j < 4; j++) {
						if (!_HolderLocations.Contains(fourDir[j])) {
							isWall = true; break;
						}
					}
					if (isWall)
						walls.Add(currentLocation);
					else
						insides.Add(currentLocation);
				}
			}
		}
		bool CheckAllInsidesConnected(ref List<Int2> list) {
			List<Int2> listToCheck = new List<Int2>(list);
			List<Int2> listNewFound = new List<Int2>();
			List<Int2> listChecked = new List<Int2>();
			listNewFound.Add(listToCheck[0]);
			listToCheck.RemoveAt(0);
			bool foundNew = true;
			Int2 checkedLocation;
			Int2[] fourDir = new Int2[4];
			int index;
			while (foundNew) {
				foundNew = false;
				listChecked.Clear();
				foreach (var location in listNewFound)
					listChecked.Add(location);
				listNewFound.Clear();
				for (int i = 0; i < listChecked.Count; i++) {
					checkedLocation = listChecked[i];
					fourDir[0] = new Int2(checkedLocation.x - 1, checkedLocation.y);
					fourDir[1] = new Int2(checkedLocation.x + 1, checkedLocation.y);
					fourDir[2] = new Int2(checkedLocation.x, checkedLocation.y + 1);
					fourDir[3] = new Int2(checkedLocation.x, checkedLocation.y - 1);
					for (int j = 0; j < 4; j++) {
						if ((index = listToCheck.IndexOf(fourDir[j])) >= 0) {
							foundNew = true;
							listNewFound.Add(listToCheck[index]);
							listToCheck.RemoveAt(index);
						}
					}
				}
			}
			return listToCheck.Count <= 0;
		}
		bool CheckEntryValid(ref List<Int2> insides) {
			List<int> entryIndices = new List<int>();
			for (int i = 0; i < _HoldersTypes.Count; i++) {
				if (_HoldersTypes[i] == BattleMapAreaHolderType.Entry)
					entryIndices.Add(i);
			}
			Int2 currentLocation;
			Int2[] fourDir = new Int2[4];
			int check, connected;
			bool connectToInside;
			foreach (var index in entryIndices) {
				connectToInside = false;
				connected = 0;
				currentLocation = _HolderLocations[index];
				fourDir[0] = new Int2(currentLocation.x - 1, currentLocation.y);
				fourDir[1] = new Int2(currentLocation.x + 1, currentLocation.y);
				fourDir[2] = new Int2(currentLocation.x, currentLocation.y + 1);
				fourDir[3] = new Int2(currentLocation.x, currentLocation.y - 1);
				for (int i = 0; i < 4; i++) {
					if ((check = _HolderLocations.IndexOf(fourDir[i])) >= 0) {
						if (_HoldersTypes[check] == BattleMapAreaHolderType.Entry)
							return false;
						if (insides.Contains(fourDir[i]))
							connectToInside = true;
						connected++;
					}
				}
				if (connected != 3) return false;
				if (!connectToInside) return false;
			}
			return true;
		}
		/// <summary>
		/// 返回到基础模板编辑模式以重新编辑
		/// </summary>
		public void ReeditTemplate() {
			_Step = BattleMapAreaEditorProcessStep.Template;
			_CellHolderLocations.Clear();
			_VariantCellHolderTypes.Clear();
			_VariantEditingIndex = -1;
			foreach (var variant in _Variants) {
				variant.Unlock();
			}
		}
		#endregion
		////////////////////////
		////    Variants    ///
		//////////////////////
		#region [ Variants  ]
		[SerializeField] List<Int2> _CellHolderLocations = new List<Int2>();
		public List<Int2> CellHolderLocations { get { return _CellHolderLocations; } }
		[SerializeField] List<VariantCellHolderType> _VariantCellHolderTypes = new List<VariantCellHolderType>();
		public List<VariantCellHolderType> VariantCellHolderTypes { get { return _VariantCellHolderTypes; } }
		[SerializeField] int _VariantEditingIndex = -1;
		public int EditingVariant { get { return _VariantEditingIndex; } set { _VariantEditingIndex = value; } }
		/// <summary>
		/// 清空正在编辑的变种
		/// </summary>
		public void ClearCurrentEdit() {
			if (_VariantEditingIndex < 0) return;
			_Variants[_VariantEditingIndex].Clear();
		}
		/// <summary>
		/// 获取变种单元信息
		/// </summary>
		/// <param name="count">数量</param>
		/// <param name="locations">位置</param>
		/// <param name="types">类型</param>
		/// <returns></returns>
		public bool GetHolderInfo(out int count, out Int2[] locations, out VariantCellHolderType[] types) {
			count = _CellHolderLocations.Count;
			locations = new Int2[count];
			types = new VariantCellHolderType[count];
			if (count <= 0) return false;
			for (int i = 0; i < count; i++) {
				types[i] = _VariantCellHolderTypes[i];
				locations[i] = _CellHolderLocations[i];
			}
			return true;
		}
		/// <summary>
		/// 增加一个新变种
		/// </summary>
		public void AddNewVariants() {
			_Variants.Add(new BattleMapAreaVariant());
			_VariantEditingIndex = _Variants.Count - 1;
		}
		/// <summary>
		/// 移除变种
		/// </summary>
		/// <param name="index">索引</param>
		public void RemoveVariantAt(int index) {
			if (index < 0 || index >= _Variants.Count) return;
			_Variants.RemoveAt(index);
			if (_VariantEditingIndex == index) _VariantEditingIndex = -1;
			else if (_VariantEditingIndex > index) _VariantEditingIndex--;
		}
		/// <summary>
		/// 复制目标变种作为一个新变种添加
		/// </summary>
		/// <param name="index">索引</param>
		public void CreateVariantCopyFrom(int index) {
			if (index < 0 || index >= _Variants.Count) return;
			_Variants.Add(_Variants[index].CreateCopy());
			_VariantEditingIndex = _Variants.Count - 1;
		}
		/// <summary>
		/// 清空所有变种
		/// </summary>
		public void ClearAllVariants() {
			_Variants.Clear();
		}
		/// <summary>
		/// 设置当前变种为正在编辑状态
		/// </summary>
		/// <param name="index"></param>
		public void SetEditing(int index) {
			_VariantEditingIndex = index;
		}
		/// <summary>
		/// 为正在编辑的变种增加单元
		/// </summary>
		/// <param name="location">位置</param>
		/// <param name="type">类型</param>
		public void AddVariantCell(Int2 location, BattleMapCellType type) {
			if (_VariantEditingIndex < 0) return;
			if (_Variants[_VariantEditingIndex].Compiled) return;
			int index = _CellHolderLocations.IndexOf(location);
			if (index < 0) return;
			VariantCellHolderType holderType = _VariantCellHolderTypes[index];
			if (holderType == VariantCellHolderType.Entry) return;
			if (type == BattleMapCellType.Home && _Type == BattleMapAreaType.Home && holderType == VariantCellHolderType.Inside)
				_Variants[_VariantEditingIndex].AddHome(location);
			else if (type == BattleMapCellType.Spawn && _Type == BattleMapAreaType.Spawn && holderType == VariantCellHolderType.Inside)
				_Variants[_VariantEditingIndex].AddSpawn(location);
			else if (type == BattleMapCellType.Path && holderType == VariantCellHolderType.Inside)
				_Variants[_VariantEditingIndex].AddPath(location);
			else if (type == BattleMapCellType.Platform)
				_Variants[_VariantEditingIndex].AddPlatform(location);
		}
		/// <summary>
		/// 从正在编辑的变种移除一个单元
		/// </summary>
		/// <param name="location">位置</param>
		public void RemoveVarientCell(Int2 location) {
			if (_VariantEditingIndex < 0) return;
			if (_Variants[_VariantEditingIndex].Compiled) return;
			int index = _CellHolderLocations.IndexOf(location);
			if (index < 0) return;
			_Variants[_VariantEditingIndex].RemoveCell(location);
		}
		/// <summary>
		/// 获取正在编辑的变种的单元信息
		/// </summary>
		/// <param name="cellCount">数量</param>
		/// <param name="locations">位置</param>
		/// <param name="types">类型</param>
		/// <returns>是否成功获取</returns>
		public bool GetCurrentVariantInfo(out int cellCount, out Int2[] locations, out BattleMapCellType[] types) {
			if (_VariantEditingIndex < 0) {
				cellCount = 0;
				locations = new Int2[0];
				types = new BattleMapCellType[0];
				return false;
			}
			return (_Variants[_VariantEditingIndex].GetInfo(out cellCount, out locations, out types));
		}
		#endregion
		////////////////////////////////////
		////    Variants -> Complete    ///
		//////////////////////////////////
		#region [ Variants -> Complete ]
		static readonly char[] Charactors = new char[] {
			'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
			'1','2','3','4','5','6','7','8','9','0'};
		/// <summary>
		/// 确认并编译变种
		/// </summary>
		/// <param name="index">所以</param>
		/// <returns>Log，成功则为Empty</returns>
		public string CompileVariant(int index) {
			if (index < 0 || index >= _Variants.Count) return "Exception: VariantIndex is out of range";
			//get all entries
			List<Int2> entries = new List<Int2>();
			for (int i = 0; i < _VariantCellHolderTypes.Count; i++) {
				if (_VariantCellHolderTypes[i] == VariantCellHolderType.Entry) {
					entries.Add(_CellHolderLocations[i]);
				}
			}
			return (_Variants[index].Compile(_Type, entries));
		}
		/// <summary>
		/// 解锁变种以重新编辑
		/// </summary>
		/// <param name="index">所以</param>
		public void UnlockVariant(int index) {
			if (index < 0 || index >= _Variants.Count) return;
			_Variants[index].Unlock();
		}

		/// <summary>
		/// 完成并确认当前模板
		/// </summary>
		/// <returns>log</returns>
		public string CompleteTemplate() {
			int numVariant = _Variants.Count;
			if (numVariant <= 0) {
				return "需要至少一个变种！";
			}
			for (int i = 0; i < numVariant; i++) {
				if (_Variants[i].Compiled) {
					continue;
				}
				string log = CompileVariant(i);
				if (log.Length > 0) {
					return "序号[" + i.ToString() + "]的变种生成路径失败！\n" + log;
				}
			}
			float sumRate = 0.0f;
			for (int i = 0; i < numVariant; i++) {
				sumRate += _Variants[i].AppearRate;
			}
			_RateSplits = new float[numVariant];
			float sumCurrent = 0.0f;
			for (int i = 0; i < numVariant - 1; i++) {
				sumCurrent += _Variants[i].AppearRate;
				_RateSplits[i] = sumCurrent / sumRate;
			}
			_RateSplits[numVariant - 1] = 1.1f;
			GenerateRandomUniqueID();
			_Entries.Clear();
			for (int i = 0; i < _VariantCellHolderTypes.Count; i++) {
				if (_VariantCellHolderTypes[i] == VariantCellHolderType.Entry) {
					_Entries.Add(_CellHolderLocations[i]);
				}
			}
			_Step = BattleMapAreaEditorProcessStep.Completed;
			return string.Empty;
		}

		void GenerateRandomUniqueID() {
			string id = string.Empty;
			for (int i = 0; i < 20; i++) {
				float randomNumber = UnityEngine.Random.Range(0f, 36f);
				int randomStringIndex = Mathf.FloorToInt(randomNumber);
				if (randomStringIndex > 35) {
					randomStringIndex = 35;
				}
				id += Charactors[randomStringIndex];
			}
			_UniqueID = id;
		}

		/// <summary>
		/// 重新返回编辑模式
		/// </summary>
		public void ReturnToEditable() {
			_UniqueID = string.Empty;
			_RateSplits = null;
			_Entries.Clear();
			_Step = BattleMapAreaEditorProcessStep.Varients;
		}
		#endregion
#endif
		#endregion
	}

	//////////////////////
	////    Editor    ///
	////////////////////
	#region [ Editor ]
#if UNITY_EDITOR
	/// <summary>
	/// 变种编译时的单元连通信息
	/// </summary>
	public class CellAccessData {
		public Int2 Location;
		public List<Int2> Nexts;

		/// <summary>
		/// 创建新的单元连通信息
		/// </summary>
		/// <param name="location">当前单元位置</param>
		/// <param name="nexts">连通单元位置</param>
		/// <param name="nextTypes">连通单元类型</param>
		public CellAccessData(Int2 location, List<Int2> nexts) {
			Location = location;
			Nexts = nexts;
		}
	}
	public enum BattleMapAreaEditorProcessStep {
		Template = 0,
		Varients = 1,
		Completed = 8,
	}
	public enum BattleMapAreaHolderType {
		Normal = 0,
		Entry = 1,
	}
	public enum VariantCellHolderType {
		Inside = 0,
		Wall = 1,
		Entry = 2,
	}
#endif
	#endregion
}
