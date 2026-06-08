using System.Collections.Generic;
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
        /// <summary>
        /// Every runner currently alive. Populated in the constructor,
        /// cleaned in <see cref="Dispose"/>. The editor uses this to
        /// reflect runtime state back onto authored nodes (per-node
        /// state pulse during play).
        /// </summary>
        public static readonly List<BehaviourTreeRunner> ActiveRunners = new List<BehaviourTreeRunner>();

        // Under "Enter Play Mode Without Domain Reload" a runner not Dispose()'d on teardown
        // would linger in this static list across sessions (the editor BT overlay reads it).
        // Clear at each play-enter as a safety net; live runners re-Add in their constructor.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => ActiveRunners.Clear();

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
            RootFrame = new GraphFrame(Blackboard);

            ActiveRunners.Add(this);
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
            ActiveRunners.Remove(this);
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
