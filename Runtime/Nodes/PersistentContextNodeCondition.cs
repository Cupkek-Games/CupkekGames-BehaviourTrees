using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    public class PersistentContextNodeCondition : BTNodeComposite
    {
        public string Key = "PersistentContextKey";
        public int Condition = 0;
        private CancellationToken? _cancellationToken;
        private int _selected = -1;

        public override void Prewarm(GameObject parent)
        {
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

            if (_selected == -1)
            {
                bool condition = false;
                if (BehaviourTreeManager.PersistentContext.TryGetValue(Key, out object contextValue))
                {
                    if (contextValue is int intValue)
                    {
                        condition = Condition == intValue;
                    }
                }

                if (condition)
                {
                    _selected = 0;
                }
                else
                {
                    _selected = 1;
                }
            }

            return Children[_selected].UpdateNode(ref Blackboard, deltaTime);
        }

        protected override void OnReset()
        {
            _selected = -1;
            _cancellationToken = null;
        }
    }
}
