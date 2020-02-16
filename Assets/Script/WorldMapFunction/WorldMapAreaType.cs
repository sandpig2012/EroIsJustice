using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EIJ.WorldMap
{
    public class WorldMapAreaType : ScriptableObject
    {
        public string name { get; private set; }
        public int moveCost { get; private set; }
    }
}
