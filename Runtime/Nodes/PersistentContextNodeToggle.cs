using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class PersistentContextNodeToggle : BTNodeAction
    {
        public string Key = "PersistentContextKey";

        protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
        {
            if (BehaviourTreeManager.PersistentContext.TryGetValue(Key, out object value))
            {
                // If the key exists, toggle its value
                if (value is int intValue)
                {
                    if (intValue == 0)
                    {
                        intValue = 1; // Toggle to true
                    }
                    else
                    {
                        intValue = 0; // Toggle to false
                    }
                    BehaviourTreeManager.PersistentContext[Key] = intValue;
                }
            }
            else
            {
                // If the key does not exist, initialize it to true
                BehaviourTreeManager.PersistentContext[Key] = 1;
            }

            return BTNodeRuntimeState.Success;
        }

        protected override void OnReset()
        {

        }
    }
}