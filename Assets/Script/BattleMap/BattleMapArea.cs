using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace EIJ.BattleMap
{
    public enum BattleMapAreaType
    {
        Normal = 0,
        Home = 1,
        Spawn = 2,
    }

    [Serializable]
    public class BattleMapAreaVariant
    {
        public List<BattleMapCell> cells = new List<BattleMapCell>();
    }

#if UNITY_EDITOR
    public enum BattleMapAreaEditorProcessStep
    {
        Template = 0,
        Varients = 1,
        Completed = 8,
    }

    public enum BattleMapAreaHolderType
    {
        Normal = 0,
        Entry = 1,
    }

   public enum VariantCellHolderType
    {
        Inside = 0,
        Wall = 1,
        Entry = 2,
    }
#endif

    [CreateAssetMenu(fileName = "BattleMapArea", menuName = "BattleMap/BattleMapArea", order = 550)]
    public class BattleMapArea : ScriptableObject
    {
        /////////////////////////////
        ////    Template Info    ////
        /////////////////////////////
        #region [ Template Info ]
        [SerializeField] private BattleMapAreaType m_Type = BattleMapAreaType.Normal;
        public BattleMapAreaType type { get { return m_Type; } set { m_Type = value; } }

        [SerializeField] private List<BattleMapAreaVariant> m_Variants = new List<BattleMapAreaVariant>();
        public List<BattleMapAreaVariant> variants { get { return m_Variants; } }

#if UNITY_EDITOR
        [SerializeField] private BattleMapAreaEditorProcessStep m_Step = BattleMapAreaEditorProcessStep.Template;
        public bool processCompleted { get { return m_Step == BattleMapAreaEditorProcessStep.Completed; } }
        public bool processTemplate { get { return m_Step == BattleMapAreaEditorProcessStep.Template; } }
        public bool processVarient { get { return m_Step == BattleMapAreaEditorProcessStep.Varients; } }

        ////////////////////////
        ////    Template    ////
        ////////////////////////
        #region [ Template ]
        [SerializeField] List<Int2> m_HolderLocations = new List<Int2>();
        [SerializeField] List<BattleMapAreaHolderType> m_HoldersTypes = new List<BattleMapAreaHolderType>();
        public bool GetTemplateInfo(out int count, out Int2[] locations, out BattleMapAreaHolderType[] types)
        {
            count = m_HolderLocations.Count;
            locations = new Int2[count];
            types = new BattleMapAreaHolderType[count];
            if (count <= 0) return false;
            for(int i = 0; i < count; i++)
            {
                types[i] = m_HoldersTypes[i];
                locations[i] = m_HolderLocations[i];
            }
            return true;
        }
        public void AddBattleMapAreaHolder(Int2 location, BattleMapAreaHolderType type)
        {
            int index = m_HolderLocations.IndexOf(location);
            if (index >= 0)
            {
                m_HoldersTypes[index] = type;
            } else
            {
                m_HolderLocations.Add(location);
                m_HoldersTypes.Add(type);
            }
        }
        public void RemoveBattleMapAreaHolder(Int2 location)
        {
            int index = m_HolderLocations.IndexOf(location);
            if (index >= 0)
            {
                m_HolderLocations.RemoveAt(index);
                m_HoldersTypes.RemoveAt(index);
            }
        }
        public void ClearAllHolders()
        {
            m_HolderLocations.Clear();
            m_HoldersTypes.Clear();
        }
        #endregion
        ///////////////////////////////////
        ////    Template -> Varients   ////
        ///////////////////////////////////
        #region [ Template -> Varients  ]
        public string ApplyTemplate()
        {
            int holderCount = m_HolderLocations.Count;
            if(holderCount < 7) return "Area is too small!";
            List<Int2> entries, walls, insides;
            GetConstuct(out entries, out walls, out insides);
            if (insides.Count <= 0) return "There is no place for path!";
            if (!CheckAllInsidesConnected(ref insides)) return "Cells inside are not all connected!";
            if (!CheckEntryValid(ref insides)) return "Invalid entry found!";

            m_CellHolderLocations.Clear();
            m_VariantCellHolderTypes.Clear();
            foreach(var location in insides)
            {
                m_CellHolderLocations.Add(location);
                m_VariantCellHolderTypes.Add(VariantCellHolderType.Inside);
            }
            foreach(var location in walls)
            {
                m_CellHolderLocations.Add(location);
                m_VariantCellHolderTypes.Add(VariantCellHolderType.Wall);
            }
            foreach(var location in entries)
            {
                m_CellHolderLocations.Add(location);
                m_VariantCellHolderTypes.Add(VariantCellHolderType.Entry);
            }
            m_Step = BattleMapAreaEditorProcessStep.Varients;
            return string.Empty;
        }
        void GetConstuct(out List<Int2> entries, out List<Int2> walls, out List<Int2> insides)
        {
            entries = new List<Int2>();
            walls = new List<Int2>();
            insides = new List<Int2>();
            Int2[] fourDir = new Int2[4];
            int count = m_HolderLocations.Count;
            bool isWall;
            for (int i = 0; i < count; i++)
            {
                Int2 currentLocation = m_HolderLocations[i];
                if (m_HoldersTypes[i] == BattleMapAreaHolderType.Entry)
                {
                    entries.Add(currentLocation);
                }
                else
                {
                    fourDir[0] = new Int2(currentLocation.x - 1, currentLocation.y);
                    fourDir[1] = new Int2(currentLocation.x + 1, currentLocation.y);
                    fourDir[2] = new Int2(currentLocation.x, currentLocation.y + 1);
                    fourDir[3] = new Int2(currentLocation.x, currentLocation.y - 1);
                    isWall = false;
                    for (int j = 0; j < 4; j++)
                    {
                        if (!m_HolderLocations.Contains(fourDir[j]))
                        {
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
        bool CheckAllInsidesConnected(ref List<Int2> list)
        {
            List<Int2> listToCheck = new List<Int2>(list);
            List<Int2> listNewFound = new List<Int2>();
            List<Int2> listChecked = new List<Int2>();
            listNewFound.Add(listToCheck[0]);
            listToCheck.RemoveAt(0);
            bool foundNew = true;
            Int2 checkedLocation;
            Int2[] fourDir = new Int2[4];
            int index;
            while (foundNew)
            {
                foundNew = false;
                listChecked.Clear();
                foreach(var location in listNewFound)
                    listChecked.Add(location);
                listNewFound.Clear();
                for (int i = 0; i < listChecked.Count; i++)
                {
                    checkedLocation = listChecked[i];
                    fourDir[0] = new Int2(checkedLocation.x - 1, checkedLocation.y);
                    fourDir[1] = new Int2(checkedLocation.x + 1, checkedLocation.y);
                    fourDir[2] = new Int2(checkedLocation.x, checkedLocation.y + 1);
                    fourDir[3] = new Int2(checkedLocation.x, checkedLocation.y - 1);
                    for (int j = 0; j < 4; j++)
                    {
                        if ((index = listToCheck.IndexOf(fourDir[j])) >= 0)
                        {
                            foundNew = true;
                            listNewFound.Add(listToCheck[index]);
                            listToCheck.RemoveAt(index);
                        }
                    }
                }
            }
            return listToCheck.Count <= 0;
        }
        bool CheckEntryValid(ref List<Int2> insides)
        {
            List<int> entryIndices = new List<int>();
            for (int i = 0; i < m_HoldersTypes.Count; i++)
            {
                if (m_HoldersTypes[i] == BattleMapAreaHolderType.Entry)
                    entryIndices.Add(i);
            }
            Int2 currentLocation;
            Int2[] fourDir = new Int2[4];
            int check, connected;
            bool connectToInside;
            foreach(var index in entryIndices)
            {
                connectToInside = false;
                connected = 0;
                currentLocation = m_HolderLocations[index];
                fourDir[0] = new Int2(currentLocation.x - 1, currentLocation.y);
                fourDir[1] = new Int2(currentLocation.x + 1, currentLocation.y);
                fourDir[2] = new Int2(currentLocation.x, currentLocation.y + 1);
                fourDir[3] = new Int2(currentLocation.x, currentLocation.y - 1);
                for (int i = 0; i < 4; i++)
                {
                    if ((check = m_HolderLocations.IndexOf(fourDir[i])) >= 0)
                    {
                        if (m_HoldersTypes[check] == BattleMapAreaHolderType.Entry)
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

        public void ReeditTemplate()
        {
            m_Step = BattleMapAreaEditorProcessStep.Template;
            m_CellHolderLocations.Clear();
            m_VariantCellHolderTypes.Clear();
        }
        #endregion
        ////////////////////////
        ////    Varients    ////
        ////////////////////////
        #region [ Varients  ]
        [SerializeField] List<Int2> m_CellHolderLocations = new List<Int2>();
        [SerializeField] List<VariantCellHolderType> m_VariantCellHolderTypes = new List<VariantCellHolderType>();
        public bool GetHolderInfo(out int count, out Int2[] locations, out VariantCellHolderType[] types)
        {
            count = m_CellHolderLocations.Count;
            locations = new Int2[count];
            types = new VariantCellHolderType[count];
            if (count <= 0) return false;
            for (int i = 0; i < count; i++)
            {
                types[i] = m_VariantCellHolderTypes[i];
                locations[i] = m_CellHolderLocations[i];
            }
            return true;
        }
        #endregion
#endif
        #endregion
    }
}
