using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.Serialization;

namespace EIJ.BattleMap {

	///////////////////////////////
	////    Battle Map Cell    ////
	///////////////////////////////
	#region [ Battle Map Cell ]
	public enum BattleMapCellType {
		Block = 0,
		Path = 1,
		Platform = 2,
		Home = 3,
		Spawn = 4,
	}

	public enum AreaMirror {
		Off,
		MirrorX,
		MirrorY,
	}

	public enum AreaRotation {
		//clockwise
		_0,
		_90,
		_180,
		_270,
	}

	[Serializable]
	public struct Int2 {
		public int x;
		public int y;
		public Int2(int x, int y) {
			this.x = x;
			this.y = y;
		}
		/// <summary>
		/// Int2(0, 0)
		/// </summary>
		public static Int2 Zero { get { return new Int2(0, 0); } }
		public static bool operator ==(Int2 a, Int2 b) => (a.x == b.x && a.y == b.y);
		public static bool operator !=(Int2 a, Int2 b) => !(a.x == b.x && a.y == b.y);
		public static Int2 operator -(Int2 a) => new Int2(-a.x, -a.y);
		public static Int2 operator +(Int2 a) => new Int2(a.x, a.y);
		public static Int2 operator +(Int2 a, Int2 b) => new Int2(a.x + b.x, a.y + b.y);
		public static Int2 operator -(Int2 a, Int2 b) => new Int2(a.x - b.x, a.y - b.y);
		public static Int2 operator *(Int2 a, Int2 b) => new Int2(a.x * b.x, a.y * b.y);
		public override bool Equals(object obj) {
			return base.Equals(obj);
		}
		public override int GetHashCode() {
			return base.GetHashCode();
		}
		public override string ToString() {
			return "[" + x + ", " + y + "]";
		}
	}

	[Serializable]
	public struct Int4 {
		public int x;
		public int y;
		public int z;
		public int w;
		public Int4(int x, int y, int z, int w) {
			this.x = x;
			this.y = y;
			this.z = z;
			this.w = w;
		}
	}

	public class BattleMapCell {
		public BattleMapCellType Type { get; set; }
		public Int2 Location { get; private set; }


		public BattleMapCell(Int2 location) {
			Type = BattleMapCellType.Block;
			Location = location;
		}

		public BattleMapCell(Int2 location, BattleMapCellType type) {
			Type = type;
			Location = location;
		}

		public BattleMapCell RelocateToBattleMap(bool centerInCell, Int2 offset, AreaMirror mirror, AreaRotation rotation) {

			int finalX = Location.x;
			int finalY = Location.y;
			//////////////////////////
			////    Relocation    ////
			//////////////////////////
			#region [ Relocation ]
			int temp = 0;
			if (centerInCell) {
				switch (mirror) {
				case AreaMirror.MirrorX:
					finalX = -finalX;
					break;
				case AreaMirror.MirrorY:
					finalY = -finalY;
					break;
				}
				switch (rotation) {
				case AreaRotation._90:
					temp = finalX;
					finalX = finalY;
					finalY = -temp;
					break;
				case AreaRotation._180:
					finalX = -finalX;
					finalY = -finalY;
					break;
				case AreaRotation._270:
					temp = finalX;
					finalX = -finalY;
					finalY = temp;
					break;
				}
			}
			else {
				switch (mirror) {
				case AreaMirror.MirrorX:
					finalX = -finalX - 1;
					break;
				case AreaMirror.MirrorY:
					finalY = -finalY - 1;
					break;
				}
				switch (rotation) {
				case AreaRotation._90:
					temp = finalX;
					finalX = finalY;
					finalY = -temp - 1;
					break;
				case AreaRotation._180:
					finalX = -finalX - 1;
					finalY = -finalY - 1;
					break;
				case AreaRotation._270:
					temp = finalX;
					finalX = -finalY - 1;
					finalY = temp;
					break;
				}
			}
			finalX += offset.x;
			finalY += offset.y;
			#endregion
			return new BattleMapCell(new Int2(finalX, finalY), Type);
		}
	}
	#endregion

	////////////////////////
	////    Path Find   ////
	////////////////////////
	#region [ Path ]
	[Serializable]
	public class BattleMapPath {
		//area local location
		[SerializeField] private List<Int2> _PathLocations = new List<Int2>();
		public List<Int2> PathLocations { get { return _PathLocations; } set { _PathLocations = value; } }
	}
	#endregion
}
