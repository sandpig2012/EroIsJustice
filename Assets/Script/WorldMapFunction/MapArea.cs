using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EIJ.WorldMap
{
    public class MapArea : MonoBehaviour
    {
        [SerializeField] MapAreaType m_MapAreaType = null;
        bool m_IsVisible = false;
        bool m_IsFogged = true;
        public MapAreaType MapAreaType { get { return m_MapAreaType; } }
        public bool IsVisible { get { return m_IsVisible; } set { m_IsVisible = value;  } }
        public bool IsFogged { get { return m_IsFogged; } set { m_IsFogged = value; } }
    }
}
