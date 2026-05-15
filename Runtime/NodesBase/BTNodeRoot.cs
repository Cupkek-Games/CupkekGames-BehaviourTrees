using System.Collections.Generic;
using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Top of the tree. No inputs (nothing connects to the root); single
    /// output to the actual behaviour entry point. Delegates updates
    /// straight to its single child resolved via the connection list.
    /// </summary>
    public class BTNodeRoot : BTNodeDecorator
    {
        public override IReadOnlyList<GraphPortDef> InputPorts => GraphPortDef.None;

        public override Color HeaderColor => new Color(0.30f, 0.55f, 0.30f);

        public override string DisplayTitle => "Root";

        protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
        {
            var child = GetChild();
            if (child == null) return BTNodeRuntimeState.Success;
            return child.UpdateNode(frame, deltaTime);
        }

        protected override void OnReset()
        {
        }
    }
}
