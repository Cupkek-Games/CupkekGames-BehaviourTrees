using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class DebugNode : BTNodeAction
    {
        public string Message;

        protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
        {
            Debug.Log($"{Message}");
            return BTNodeRuntimeState.Success;
        }

        protected override void OnReset()
        {
        }
    }
}
