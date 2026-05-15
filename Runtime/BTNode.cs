using System;
using UnityEngine;
using CupkekGames.Graphs;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Abstract base for every node in a <see cref="BehaviourTree"/>. Now a
    /// <see cref="GraphNodeSO"/> so it inherits identity, position, port
    /// declarations, and editor visuals from the graphs foundation.
    /// Runtime concerns (state, blackboard-driven OnUpdate, prewarm /
    /// dispose lifecycle) stay BT-specific.
    /// </summary>
    public abstract class BTNode : GraphNodeSO
    {
        /// <summary>
        /// Runtime tree this cloned node belongs to. Set by
        /// <see cref="BehaviourTree.Clone"/> so composite / decorator nodes
        /// can look up their children via the owner's connection list. Null
        /// for nodes inspected at edit time outside of a runner.
        /// </summary>
        [NonSerialized] public BehaviourTree OwnerTree;

        [NonSerialized] public BTNodeRuntimeState State = BTNodeRuntimeState.Idle;

        // Header colour is overridden per-shape in subclass categories
        // (root green, composite amber, decorator violet, action grey).
        // Subclasses can override further per type.
        public override Color HeaderColor => new Color(0.35f, 0.38f, 0.45f);

        public BTNodeRuntimeState UpdateNode(GraphFrame frame, float deltaTime)
        {
            State = OnUpdate(frame, deltaTime);
            return State;
        }

        protected abstract BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime);
        public abstract BTNode Clone();

        public void ResetNode()
        {
            State = BTNodeRuntimeState.Idle;
            OnReset();
        }

        protected abstract void OnReset();

        public virtual void Prewarm(GameObject parent)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}
