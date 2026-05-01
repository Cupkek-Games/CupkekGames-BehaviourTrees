using System;
using System.Collections.Generic;
using UnityEngine;
using CupkekGames.Data;
using CupkekGames.Data.Primitives;

namespace CupkekGames.BehaviourTrees
{
    public abstract class BTNode : ScriptableObject
    {
        [HideInInspector] public SerializedGuid Guid = new SerializedGuid(System.Guid.NewGuid());
        [HideInInspector] public Vector2 Position;
        [NonSerialized] public BTNodeRuntimeState State = BTNodeRuntimeState.Idle;

        public BTNodeRuntimeState UpdateNode(ref Dictionary<string, object> Blackboard, float deltaTime)
        {
            State = OnUpdate(ref Blackboard, deltaTime);

            return State;
        }

        protected abstract BTNodeRuntimeState OnUpdate(ref Dictionary<string, object> Blackboard, float deltaTime);
        public abstract BTNode Clone();

        public void ResetNode()
        {
            State = BTNodeRuntimeState.Idle;

            OnReset();
        }

        protected abstract void OnReset();

        public virtual void Prewarm(GameObject parent)
        {
        }

        public virtual void Dispose()
        {
        }
    }
}