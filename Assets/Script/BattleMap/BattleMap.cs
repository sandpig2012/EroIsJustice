using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EIJ.BattleMap;

namespace EIJ.OnBattle
{
    public class BattleMap
    {
        public int spawnCount { get; private set; }
        public List<BattleMapCell> spawns { get; private set; }
        public BattleMapCell home { get; private set; }
        public List<BattleMapPath> paths { get; private set; }
    }
}
