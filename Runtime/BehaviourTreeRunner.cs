using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Holds a runtime clone of a <see cref="BehaviourTree"/> plus the
    /// blackboard / root frame the tree executes against. One instance
    /// per running agent — clone keeps node state isolated from other
    /// agents executing the same asset.
    /// </summary>
    public class BehaviourTreeRunner
    {
        private BehaviourTree _originalTree;
        public BehaviourTree OriginalTree => _originalTree;

        private BehaviourTree _runtimeClone;
        public BehaviourTree RuntimeClone => _runtimeClone;

        /// <summary>Untyped global state shared across the tree.</summary>
        public GraphBlackboard Blackboard { get; }

        /// <summary>
        /// Root frame for this run. Nodes can <see cref="GraphFrame.Push"/>
        /// child frames to scope a value to a sub-tree without leaking it
        /// to siblings.
        /// </summary>
        public GraphFrame RootFrame { get; }

        public BehaviourTreeRunner(BehaviourTree originalTree)
        {
            _originalTree = originalTree;
            _runtimeClone = _originalTree.Clone();
            Blackboard = new GraphBlackboard();
            // Seed declared variable defaults into the blackboard before
            // any node runs. Authored values land in globals — overrides
            // happen via frame.SetLocal in scoped decorators.
            BlackboardSeeder.Apply(_originalTree, Blackboard);
            RootFrame = new GraphFrame(Blackboard);
        }

        public void Prewarm(GameObject parent)
        {
            foreach (var n in _runtimeClone.Nodes)
                if (n is BTNode bt) bt.Prewarm(parent);
        }

        public void Dispose()
        {
            foreach (var n in _runtimeClone.Nodes)
                if (n is BTNode bt) bt.Dispose();
        }

        public BTNodeRuntimeState UpdateTree(float deltaTime)
        {
            return _runtimeClone.UpdateTree(RootFrame, deltaTime);
        }

        public void ResetTree()
        {
            _runtimeClone.ResetTree();
        }
    }
}
