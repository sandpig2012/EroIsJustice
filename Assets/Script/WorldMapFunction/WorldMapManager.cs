using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EIJ.WorldMap
{
    public class WorldMapManager : MonoBehaviour
    {
        [Serializable]
        internal class AreaInfo
        {
            [SerializeField] private WorldMapArea m_MapArea;
            public WorldMapArea MapArea { get { return m_MapArea; } }


            public AreaInfo(WorldMapArea mapArea)
            {
                m_MapArea = mapArea;
            }
        }
    }
}
