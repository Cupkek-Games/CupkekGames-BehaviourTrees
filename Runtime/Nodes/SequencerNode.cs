using System.Threading;
using CupkekGames.Graphs;

namespace CupkekGames.BehaviourTrees
{
    public class SequencerNode : BTNodeComposite
    {
        public bool ExitOnFail = false;

        private int _index = 0;
        private BTNodeRuntimeState _lastState;
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

            var children = GetChildren();
            if (children.Count == 0) return BTNodeRuntimeState.Success;

            if (_index >= children.Count) _index = 0;

            var child = children[_index];
            _lastState = child != null
                ? child.UpdateNode(frame, deltaTime)
                : BTNodeRuntimeState.Success;

            if (ExitOnFail && _lastState == BTNodeRuntimeState.Fail)
                return BTNodeRuntimeState.Fail;

            if (_lastState == BTNodeRuntimeState.Success || _lastState == BTNodeRuntimeState.Fail)
            {
                _index++;
                if (_index >= children.Count)
                    return BTNodeRuntimeState.Success;
            }

            return BTNodeRuntimeState.Running;
        }

        protected override void OnReset()
        {
            _index = 0;
            _lastState = BTNodeRuntimeState.Idle;
            _cancellationToken = null;
        }
    }
}
