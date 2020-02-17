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
    public enum BattleMapCellType
    {
        Block = 0,
        Path = 1,
        Platform = 2,
        Home = 3,
        Spawn = 4,
    }

    public enum AreaMirror
    {
        Off,
        MirrorX,
        MirrorY,
    }

    public enum AreaRotation
    {
        //clockwise
        _0,
        _90,
        _180,
        _270,
    }

    [Serializable]
    public struct Int2
    {
        public int x;
        public int y;
        public Int2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }

    public class BattleMapCell
    {
        public BattleMapCellType type { get; set; }
        public Int2 location { get; private set; }


        public BattleMapCell(Int2 location)
        {
            this.type = BattleMapCellType.Block;
            this.location = location;
        }

        public BattleMapCell(Int2 location, BattleMapCellType type)
        {
            this.type = type;
            this.location = location;
        }

        public BattleMapCell RelocateToBattleMap(bool centerInCell, Int2 offset, AreaMirror mirror, AreaRotation rotation)
        {

            int finalX = location.x;
            int finalY = location.y;
            //////////////////////////
            ////    Relocation    ////
            //////////////////////////
            #region [ Relocation ]
            int temp = 0;
            if(centerInCell)
            {
                switch (mirror)
                {
                    case AreaMirror.MirrorX:
                        finalX = -finalX;
                        break;
                    case AreaMirror.MirrorY:
                        finalY = -finalY;
                        break;
                }
                switch (rotation)
                {
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
            else
            {
                switch (mirror)
                {
                    case AreaMirror.MirrorX:
                        finalX = -finalX - 1;
                        break;
                    case AreaMirror.MirrorY:
                        finalY = -finalY - 1;
                        break;
                }
                switch (rotation)
                {
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
            return new BattleMapCell(new Int2(finalX, finalY), type);
        }
    }
    #endregion

    ////////////////////////
    ////    Path Find   ////
    ////////////////////////
    #region [ Path ]
    [Serializable]
    public class BattleMapPath
    {
        //area local location
        public List<Int2> m_PathLocations = new List<Int2>();
    }
    #endregion
}
