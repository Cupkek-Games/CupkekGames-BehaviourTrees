using System.Collections.Generic;
using System.Threading;
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

            for (int i = 0; i < Children.Count; i++)
            {
                _childStates.Add(BTNodeRuntimeState.Running);
            }
        }

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

            bool anyRunning = false;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];

                // Skip null children
                if (child == null)
                {
                    _childStates[i] = BTNodeRuntimeState.Success;
                    continue;
                }

                // Only update nodes that are still running
                if (_childStates[i] == BTNodeRuntimeState.Running)
                {
                    _childStates[i] = child.UpdateNode(ref Blackboard, deltaTime);
                }

                // Check the state and apply exit conditions
                if (_childStates[i] == BTNodeRuntimeState.Running)
                {
                    anyRunning = true;
                }
                else if (_childStates[i] == BTNodeRuntimeState.Fail && ExitOnAnyFail)
                {
                    return BTNodeRuntimeState.Fail; // Immediate exit if any fail condition is met
                }
                else if (_childStates[i] == BTNodeRuntimeState.Success && ExitOnAnySuccess)
                {
                    return BTNodeRuntimeState.Success; // Immediate exit if any success condition is met
                }
            }

            // If no children are running, determine the final state
            return anyRunning ? BTNodeRuntimeState.Running : BTNodeRuntimeState.Success;
        }

        protected override void OnReset()
        {
            _childStates.Clear();

            for (int i = 0; i < Children.Count; i++)
            {
                _childStates.Add(BTNodeRuntimeState.Running);
            }

            _cancellationToken = null;
        }
    }
}
