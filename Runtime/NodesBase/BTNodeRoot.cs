using System.Collections.Generic;

namespace CupkekGames.BehaviourTrees
{
    public class BTNodeRoot : BTNodeDecorator
    {
        protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
        {
            return Child.UpdateNode(ref Blackboard, deltaTime);
        }
        protected override void OnReset()
        {

        }
    }
}