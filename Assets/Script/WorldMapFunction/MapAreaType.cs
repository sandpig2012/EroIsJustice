using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EIJ.WorldMap
{
    public class MapAreaType : ScriptableObject
    {
        [SerializeField] string Name = "Plain";
        [SerializeField] int MoveCost = 1;
    }
}
