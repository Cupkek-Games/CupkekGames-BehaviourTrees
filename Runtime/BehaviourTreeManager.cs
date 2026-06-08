using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public static class BehaviourTreeManager
    {
        public static Dictionary<string, object> PersistentContext = new();

        // Under "Enter Play Mode Without Domain Reload" this static blackboard would carry
        // stale data into the next play session — clear it at each play-enter.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => PersistentContext.Clear();

        public static void ClearPersistentContext()
        {
            PersistentContext.Clear();
        }
    }
}