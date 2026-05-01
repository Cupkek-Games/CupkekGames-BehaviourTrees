using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class DebugNode : BTNodeAction
    {
        public string Message;

        protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
        {
            Debug.Log($"{Message}");

            return BTNodeRuntimeState.Success;
        }

        protected override void OnReset()
        {

        }
    }
}