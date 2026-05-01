
using System.Collections.Generic;
using System.Threading;

namespace CupkekGames.BehaviourTrees
{
    public class SequencerNode : BTNodeComposite
    {
        public bool ExitOnFail = false;
        private int _index = 0;
        private BTNodeRuntimeState _lastState;
        private CancellationToken? _cancellationToken;
        protected override BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime)
        {
            if (!_cancellationToken.HasValue)
            {
                CancellationToken ct1;
                if (Blackboard.ContainsKey("CancellationToken"))
                {
                    ct1 = (CancellationToken)Blackboard["CancellationToken"];
                    if (ct1.IsCancellationRequested)
                    {
                        return BTNodeRuntimeState.Fail;
                    }
                }
                CancellationToken ct2;
                if (Blackboard.ContainsKey("CancellationTokenCasterDeath"))
                {
                    ct1 = (CancellationToken)Blackboard["CancellationTokenCasterDeath"];
                    if (ct1.IsCancellationRequested)
                    {
                        return BTNodeRuntimeState.Fail;
                    }
                }
                CancellationToken ct3;
                if (Blackboard.ContainsKey("CancellationTokenCasterInterrupt"))
                {
                    ct1 = (CancellationToken)Blackboard["CancellationTokenCasterInterrupt"];
                    if (ct1.IsCancellationRequested)
                    {
                        return BTNodeRuntimeState.Fail;
                    }
                }

                _cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(ct1, ct2, ct3).Token;
            }

            if (_cancellationToken.Value.IsCancellationRequested)
            {
                return BTNodeRuntimeState.Fail;
            }

            if (_index == Children.Count)
            {
                // Reset and loop if update called again after squence is done
                _index = 0;
            }

            BTNode child = Children[_index];

            if (child == null)
            {
                _lastState = BTNodeRuntimeState.Success;
            }
            else
            {
                _lastState = child.UpdateNode(ref Blackboard, deltaTime);
            }

            if (ExitOnFail && _lastState == BTNodeRuntimeState.Fail)
            {
                return BTNodeRuntimeState.Fail;
            }

            if (_lastState == BTNodeRuntimeState.Success || _lastState == BTNodeRuntimeState.Fail)
            {
                _index++;

                if (_index == Children.Count)
                {
                    return BTNodeRuntimeState.Success;
                }
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