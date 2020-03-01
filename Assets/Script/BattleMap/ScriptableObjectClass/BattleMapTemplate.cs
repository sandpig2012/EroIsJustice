using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EIJ.BattleMap {
	/// <summary>
	/// 战斗地图模板ScriptableObject
	/// </summary>
	[CreateAssetMenu(fileName = "BattleMapTemplate", menuName = "Battle Map/Template", order = 551)]
	public class BattleMapTemplate : ScriptableObject {
		[SerializeField] private Int4 _TemplateRange = new Int4(-20, -20, 20, 20); //min_x, min_y, max_x, max_y
		public Int4 TemplateRange { get { return _TemplateRange; } }
		[SerializeField] private List<BattleMapArea> _Areas = new List<BattleMapArea>();
		public List<BattleMapArea> Areas { get { return _Areas; } }
		[SerializeField] private List<Int2> _AreaLocations = new List<Int2>();
		public List<Int2> AreaLocations { get { return _AreaLocations; } }
		[SerializeField] private List<AreaRotation> _AreaRotations = new List<AreaRotation>();
		public List<AreaRotation> AreaRotations { get { return _AreaRotations; } }

#if UNITY_EDITOR
		[SerializeField] private List<Int4> _AreaBounds = new List<Int4>(); //xmin, ymin, xmax, ymax
		[SerializeField] private List<string> _UniqueIDs = new List<string>();
		/// <summary>
		/// 设置模板尺寸
		/// </summary>
		/// <param name="range">尺寸Int4(min_x, min_y, max_x, max_y)</param>
		public void SetTemplateRange(Int4 range) {
			_TemplateRange = range;
		}
		/// <summary>
		/// 拖动添加新的模块
		/// </summary>
		/// <param name="battleMapArea">模块</param>
		/// <param name="location">位置</param>
		public void DragAddNewArea(BattleMapArea battleMapArea, Int2 location) {
			Debug.Log("Test Action: Add " + battleMapArea.name + " to [" + location.x + ", " + location.y + "]");
		}
#endif
	}
}
