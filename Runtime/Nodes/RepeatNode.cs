using System.Collections.Generic;
using System.Threading;
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

            BTNodeRuntimeState state = Child.UpdateNode(ref Blackboard, deltaTime);

            if (ExitOnFail && state == BTNodeRuntimeState.Fail)
            {
                return BTNodeRuntimeState.Fail;
            }

            if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
            {
                if (_repeated + 1 == RepeatAmount)
                {
                    return BTNodeRuntimeState.Success;
                }

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