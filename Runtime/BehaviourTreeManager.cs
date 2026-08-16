using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.BehaviourTrees
{
    public static partial class BehaviourTreeManager
    {
        [AutoStaticsCleanup]
        public static Dictionary<string, object> PersistentContext = new();

        // Under "Enter Play Mode Without Domain Reload" this static blackboard would carry
        // stale data into the next play session — clear it at each play-enter.
        public static void ClearPersistentContext()
        {
            PersistentContext.Clear();
        }
    }
}