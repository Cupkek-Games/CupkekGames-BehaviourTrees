using System.Collections.Generic;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class BehaviourTreeRunner
    {
        private BehaviourTree _originalTree;
        public BehaviourTree OriginalTree => _originalTree;
        private BehaviourTree _runtimeClone;
        public BehaviourTree RuntimeClone => _runtimeClone;
        public Dictionary<string, object> Blackboard = new Dictionary<string, object>();
        public BehaviourTreeRunner(BehaviourTree originalTree)
        {
            _originalTree = originalTree;
            _runtimeClone = _originalTree.Clone();
        }
        public void Prewarm(GameObject parent)
        {
            foreach (BTNode node in _runtimeClone.Nodes)
            {
                node.Prewarm(parent);
            }
        }
        public void Dispose()
        {
            foreach (BTNode node in _runtimeClone.Nodes)
            {
                node.Dispose();
            }
        }
        public BTNodeRuntimeState UpdateTree(float deltaTime)
        {
            return _runtimeClone.UpdateTree(ref Blackboard, deltaTime);
        }
        public void ResetTree()
        {
            _runtimeClone.ResetTree();
        }
    }
}