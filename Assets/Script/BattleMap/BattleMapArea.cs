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
        [SerializeField] private List<Int2> m_CellLocations = new List<Int2>();
        [SerializeField] private List<BattleMapCellType> m_CellTypes = new List<BattleMapCellType>();

        //compiled data
        [SerializeField] private int m_EntryCount = 0;
        public int entryCount { get { return m_EntryCount; } }
        [SerializeField] private BattleMapPath[,] m_Paths = null;
        public BattleMapPath[,] paths { get { return m_Paths; } }
        [SerializeField] private float[] m_RateSplitPoints = null;
        public float[] rateSplitPoints { get { return m_RateSplitPoints; } }

#if UNITY_EDITOR
        [SerializeField] private float m_AppearRate = 1.0f;
        public float appearRate { get { return m_AppearRate; } set { m_AppearRate = Mathf.Clamp(value, 0.1f, 100.0f); } }
        [SerializeField] private bool m_Compiled = false;
        public bool compiled { get { return m_Compiled; } }

        /////////////////////
        ////    Paint    ////
        /////////////////////
        #region [ Paint ]
        public void AddHome(Int2 location)
        {
            int homeIndex = m_CellTypes.IndexOf(BattleMapCellType.Home);
            if(homeIndex >= 0)
            {
                m_CellLocations.RemoveAt(homeIndex);
                m_CellTypes.RemoveAt(homeIndex);
            }
            int index = m_CellLocations.IndexOf(location);
            if(index >= 0)
            {
                m_CellTypes[index] = BattleMapCellType.Home;
            }
            else
            {
                m_CellLocations.Add(location);
                m_CellTypes.Add(BattleMapCellType.Home);
            }
        }
        public void AddSpawn(Int2 location)
        {
            int spawnIndex = m_CellTypes.IndexOf(BattleMapCellType.Spawn);
            if (spawnIndex >= 0)
            {
                m_CellLocations.RemoveAt(spawnIndex);
                m_CellTypes.RemoveAt(spawnIndex);
            }
            int index = m_CellLocations.IndexOf(location);
            if (index >= 0)
            {
                m_CellTypes[index] = BattleMapCellType.Spawn;
            }
            else
            {
                m_CellLocations.Add(location);
                m_CellTypes.Add(BattleMapCellType.Spawn);
            }
        }
        public void AddPath(Int2 location)
        {
            int index = m_CellLocations.IndexOf(location);
            if (index >= 0) m_CellTypes[index] = BattleMapCellType.Path;
            else
            {
                m_CellLocations.Add(location);
                m_CellTypes.Add(BattleMapCellType.Path);
            }
        }
        public void AddPlatform(Int2 location)
        {
            int index = m_CellLocations.IndexOf(location);
            if (index >= 0) m_CellTypes[index] = BattleMapCellType.Platform;
            else
            {
                m_CellLocations.Add(location);
                m_CellTypes.Add(BattleMapCellType.Platform);
            }
        }
        public void RemoveCell(Int2 location)
        {
            int index = m_CellLocations.IndexOf(location);
            if(index >= 0)
            {
                m_CellLocations.RemoveAt(index);
                m_CellTypes.RemoveAt(index);
            }
        }
        public bool GetInfo(out int cellCount, out Int2[] locations, out BattleMapCellType[] types)
        {
            cellCount = m_CellLocations.Count;
            locations = new Int2[cellCount];
            types = new BattleMapCellType[cellCount];
            if (cellCount <= 0) return false;
            for(int i = 0; i < cellCount; i++)
            {
                locations[i] = m_CellLocations[i];
                types[i] = m_CellTypes[i];
            }
            return true;
        }
        #endregion
        ///////////////////////
        ////    Compile    ////
        ///////////////////////
        #region [ Compile ]
        public void Compile() 
        {
            m_Compiled = true;
        }
        public void Decompile()
        {
            m_Compiled = false;
        }
        #endregion
        //////////////////////
        ////    Helper    ////
        //////////////////////
        #region [ Helper ]
        public void Clear()
        {
            m_CellLocations.Clear();
            m_CellTypes.Clear();
        }
        public void SetLocationAndType(List<Int2> locations, List<BattleMapCellType> types)
        {
            Clear();
            foreach (var location in locations) m_CellLocations.Add(location);
            foreach (var type in types) m_CellTypes.Add(type);
        }
        public BattleMapAreaVariant CreateCopy()
        {
            BattleMapAreaVariant copy = new BattleMapAreaVariant();
            copy.SetLocationAndType(m_CellLocations, m_CellTypes);
            copy.appearRate = m_AppearRate;
            return copy;
        }
        public void MatchNewTemplate(BattleMapAreaType mapType, ref List<Int2> cellHolderLocations, ref List<VariantCellHolderType> cellHolderTypes)
        {
            int count = m_CellLocations.Count;
            Int2 currentLocation;
            int currentIndex;
            BattleMapCellType currentCellType;
            VariantCellHolderType currentHolderType;
            for (int i = 0; i < count; i++)
            {
                currentLocation = m_CellLocations[i];
                currentIndex = cellHolderLocations.IndexOf(currentLocation);
                if(currentIndex < 0)
                {
                    m_CellLocations.RemoveAt(i);
                    m_CellTypes.RemoveAt(i);
                    count--;
                    i--;
                    continue;
                }
                else
                {
                    currentHolderType = cellHolderTypes[currentIndex];
                    currentCellType = m_CellTypes[i];
                    if (currentHolderType == VariantCellHolderType.Entry)
                    {
                        m_CellLocations.RemoveAt(i);
                        m_CellTypes.RemoveAt(i);
                        count--;
                        i--;
                        continue;
                    }
                    else if (currentHolderType == VariantCellHolderType.Wall)
                    {
                        if(currentCellType != BattleMapCellType.Platform)
                        {
                            m_CellLocations.RemoveAt(i);
                            m_CellTypes.RemoveAt(i);
                            count--;
                            i--;
                            continue;
                        }
                    }
                    if(currentCellType == BattleMapCellType.Home && mapType != BattleMapAreaType.Home)
                    {
                        m_CellLocations.RemoveAt(i);
                        m_CellTypes.RemoveAt(i);
                        count--;
                        i--;
                        continue;
                    }
                    if(currentCellType == BattleMapCellType.Spawn && mapType != BattleMapAreaType.Spawn)
                    {
                        m_CellLocations.RemoveAt(i);
                        m_CellTypes.RemoveAt(i);
                        count--;
                        i--;
                        continue;
                    }
                }
            }
        }
        #endregion
#endif
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
        public bool processVarients { get { return m_Step == BattleMapAreaEditorProcessStep.Varients; } }

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
            foreach(var variant in m_Variants)
            {
                variant.MatchNewTemplate(m_Type, ref m_CellHolderLocations, ref m_VariantCellHolderTypes);
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
            m_VariantEditingIndex = -1;
            foreach(var variant in m_Variants)
            {
                variant.Decompile();
            }
        }
        #endregion
        ////////////////////////
        ////    Varients    ////
        ////////////////////////
        #region [ Varients  ]
        [SerializeField] List<Int2> m_CellHolderLocations = new List<Int2>();
        [SerializeField] List<VariantCellHolderType> m_VariantCellHolderTypes = new List<VariantCellHolderType>();
        [SerializeField] int m_VariantEditingIndex = -1;
        public int editingVariant { get { return m_VariantEditingIndex; } set { m_VariantEditingIndex = value; } }
        public void ClearCurrentEdit()
        {
            if (m_VariantEditingIndex < 0) return;
            m_Variants[m_VariantEditingIndex].Clear();
        }
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
        public void AddNewVariants()
        {
            m_Variants.Add(new BattleMapAreaVariant());
            m_VariantEditingIndex = m_Variants.Count - 1;
        }
        public void RemoveVariantAt(int index)
        {
            if (index < 0 || index >= m_Variants.Count) return;
            m_Variants.RemoveAt(index);
            if (m_VariantEditingIndex == index) m_VariantEditingIndex = -1;
            else if (m_VariantEditingIndex > index) m_VariantEditingIndex--;
        }
        public void CreateVariantCopyFrom(int index)
        {
            if (index < 0 || index >= m_Variants.Count) return;
            m_Variants.Add(m_Variants[index].CreateCopy());
            m_VariantEditingIndex = m_Variants.Count - 1;
        }
        public void ClearAllVariants()
        {
            m_Variants.Clear();
        }
        public void SetEditing(int index)
        {
            m_VariantEditingIndex = index;
        }
        public void AddVariantCell(Int2 location, BattleMapCellType type)
        {
            if (m_VariantEditingIndex < 0) return;
            int index = m_CellHolderLocations.IndexOf(location);
            if (index < 0) return;
            VariantCellHolderType holderType = m_VariantCellHolderTypes[index];
            if (holderType == VariantCellHolderType.Entry) return;
            if (type == BattleMapCellType.Home && m_Type == BattleMapAreaType.Home && holderType == VariantCellHolderType.Inside)
                m_Variants[m_VariantEditingIndex].AddHome(location);
            else if (type == BattleMapCellType.Spawn && m_Type == BattleMapAreaType.Spawn && holderType == VariantCellHolderType.Inside)
                m_Variants[m_VariantEditingIndex].AddSpawn(location);
            else if (type == BattleMapCellType.Path && holderType == VariantCellHolderType.Inside)
                m_Variants[m_VariantEditingIndex].AddPath(location);
            else if (type == BattleMapCellType.Platform)
                m_Variants[m_VariantEditingIndex].AddPlatform(location);
        }
        public void RemoveVarientCell(Int2 location)
        {
            if (m_VariantEditingIndex < 0) return;
            int index = m_CellHolderLocations.IndexOf(location);
            if (index < 0) return;
            m_Variants[m_VariantEditingIndex].RemoveCell(location);
        }
        public bool GetCurrentVariantInfo(out int cellCount, out Int2[] locations, out BattleMapCellType[] types)
        {
            if(m_VariantEditingIndex < 0)
            {
                cellCount = 0;
                locations = new Int2[0];
                types = new BattleMapCellType[0];
                return false;
            }
            return (m_Variants[m_VariantEditingIndex].GetInfo(out cellCount, out locations, out types));
        }
        #endregion
#endif
        #endregion
    }
}
