using System.Collections.Generic;
using System.Threading;
using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class ParallelNode : BTNodeComposite
    {
        public bool ExitOnAnyFail = false;
        public bool ExitOnAnySuccess = false;

        private List<BTNodeRuntimeState> _childStates;
        private CancellationToken? _cancellationToken;

        public override void Prewarm(GameObject parent)
        {
            _childStates = new List<BTNodeRuntimeState>();
            foreach (var _ in GetChildren())
                _childStates.Add(BTNodeRuntimeState.Running);
        }

        protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
        {
            if (!_cancellationToken.HasValue)
            {
                if (!BTCancellation.TryCreateLinkedToken(frame, out var linked))
                    return BTNodeRuntimeState.Fail;
                _cancellationToken = linked;
            }
            if (_cancellationToken.Value.IsCancellationRequested)
                return BTNodeRuntimeState.Fail;

            var children = GetChildren();
            if (children.Count == 0) return BTNodeRuntimeState.Success;

            // _childStates is allocated in Prewarm; if Prewarm wasn't called
            // (e.g. test harness) lazy-init here so we don't NRE.
            if (_childStates == null || _childStates.Count != children.Count)
            {
                _childStates = new List<BTNodeRuntimeState>(children.Count);
                for (int i = 0; i < children.Count; i++)
                    _childStates.Add(BTNodeRuntimeState.Running);
            }

            bool anyRunning = false;

            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                if (child == null)
                {
                    _childStates[i] = BTNodeRuntimeState.Success;
                    continue;
                }

                if (_childStates[i] == BTNodeRuntimeState.Running)
                    _childStates[i] = child.UpdateNode(frame, deltaTime);

                if (_childStates[i] == BTNodeRuntimeState.Running)
                {
                    anyRunning = true;
                }
                else if (_childStates[i] == BTNodeRuntimeState.Fail && ExitOnAnyFail)
                {
                    return BTNodeRuntimeState.Fail;
                }
                else if (_childStates[i] == BTNodeRuntimeState.Success && ExitOnAnySuccess)
                {
                    return BTNodeRuntimeState.Success;
                }
            }

            return anyRunning ? BTNodeRuntimeState.Running : BTNodeRuntimeState.Success;
        }

        protected override void OnReset()
        {
            var children = GetChildren();
            _childStates?.Clear();
            _childStates ??= new List<BTNodeRuntimeState>(children.Count);
            for (int i = 0; i < children.Count; i++)
                _childStates.Add(BTNodeRuntimeState.Running);

            _cancellationToken = null;
        }
    }
}
