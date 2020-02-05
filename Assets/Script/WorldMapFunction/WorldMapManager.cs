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
            [SerializeField] private MapArea m_MapArea;
            public MapArea MapArea { get { return m_MapArea; } }


            public AreaInfo(MapArea mapArea)
            {
                m_MapArea = mapArea;
            }
        }
    }
}
