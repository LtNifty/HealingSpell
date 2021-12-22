using System.Collections;
using ThunderRoad;
using UnityEngine;

namespace HealingSpell
{
    public class HealingSpellLevelModule : LevelModule
    {
        public override IEnumerator OnLoadCoroutine()
        {
            Debug.Log("(Healing Spell) Loaded successfully!");
            return base.OnLoadCoroutine();
        }
    }
}