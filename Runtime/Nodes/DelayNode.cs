using System.Threading;
using CupkekGames.Graphs;

namespace CupkekGames.BehaviourTrees
{
    public class DelayNode : BTNodeDecorator
    {
        public float Duration = 1;

        private float _passed = -1f;
        private CancellationToken? _cancellationToken;
        protected bool _completed = false;

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

            _passed = _passed < 0f ? deltaTime : _passed + deltaTime;

            if (_passed > Duration)
            {
                bool continueExecution = true;
                if (!_completed)
                {
                    _completed = true;
                    continueExecution = OnDelayComplete(frame);
                }

                if (!continueExecution)
                {
                    _passed = -1f;
                    _completed = false;
                    return BTNodeRuntimeState.Fail;
                }

                var state = BTNodeRuntimeState.Success;
                var child = GetChild();
                if (child != null)
                    state = child.UpdateNode(frame, deltaTime);

                if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
                {
                    _passed = -1f;
                    _completed = false;
                }

                return state;
            }

            return BTNodeRuntimeState.Running;
        }

        protected override void OnReset()
        {
            _passed = -1f;
            _cancellationToken = null;
            _completed = false;
        }

        protected virtual bool OnDelayComplete(GraphFrame frame)
        {
            return true;
        }
    }
}
