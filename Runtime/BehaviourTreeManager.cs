using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public static class BehaviourTreeManager
    {
        public static Dictionary<string, object> PersistentContext = new();

        public static void ClearPersistentContext()
        {
            PersistentContext.Clear();
        }
    }
}