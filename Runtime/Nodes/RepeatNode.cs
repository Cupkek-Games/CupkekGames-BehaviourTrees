using System.Threading;
using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class RepeatNode : BTNodeDecorator
    {
        [Header("(RepeatAmount <= 0) for infinite")]
        public int RepeatAmount;
        public bool ExitOnFail = false;

        private int _repeated = 0;
        private CancellationToken? _cancellationToken;

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

            var child = GetChild();
            if (child == null) return BTNodeRuntimeState.Success;

            var state = child.UpdateNode(frame, deltaTime);

            if (ExitOnFail && state == BTNodeRuntimeState.Fail)
                return BTNodeRuntimeState.Fail;

            if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
            {
                if (_repeated + 1 == RepeatAmount)
                    return BTNodeRuntimeState.Success;
                _repeated++;
            }

            return BTNodeRuntimeState.Running;
        }

        protected override void OnReset()
        {
            _repeated = 0;
            _cancellationToken = null;
        }
    }
}
