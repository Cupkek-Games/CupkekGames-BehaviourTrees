using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class DelayNode : BTNodeDecorator
    {
        public float Duration = 1;
        private float _passed = -1f;

        private CancellationToken? _cancellationToken;
        protected bool _completed = false;

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
                    ct2 = (CancellationToken)Blackboard["CancellationTokenCasterDeath"];
                    if (ct2.IsCancellationRequested)
                    {
                        return BTNodeRuntimeState.Fail;
                    }
                }
                CancellationToken ct3;
                if (Blackboard.ContainsKey("CancellationTokenCasterInterrupt"))
                {
                    ct3 = (CancellationToken)Blackboard["CancellationTokenCasterInterrupt"];
                    if (ct3.IsCancellationRequested)
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

            if (_passed < 0f)
            {
                _passed = deltaTime;
            }
            else
            {
                _passed += deltaTime;
            }

            if (_passed > Duration)
            {
                bool continueExecution = true;
                if (!_completed)
                {
                    _completed = true;
                    continueExecution = OnDelayComplete(ref Blackboard);
                }

                if (!continueExecution)
                {
                    _passed = -1f;
                    _completed = false;
                    return BTNodeRuntimeState.Fail;
                }

                BTNodeRuntimeState state = BTNodeRuntimeState.Success;

                if (Child != null)
                {
                    state = Child.UpdateNode(ref Blackboard, deltaTime);
                }

                if (state == BTNodeRuntimeState.Success || state == BTNodeRuntimeState.Fail)
                {
                    // Reset for next call
                    _passed = -1f;
                    _completed = false;
                }

                return state;
            }

            return BTNodeRuntimeState.Running;
        }
        protected override void OnReset()
        {
            _passed = -1;
            _cancellationToken = null;
            _completed = false;
        }
        protected virtual bool OnDelayComplete(ref Dictionary<string, object> Blackboard)
        {
            return true;
        }
    }
}
