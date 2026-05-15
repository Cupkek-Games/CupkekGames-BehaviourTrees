using CupkekGames.Graphs;

namespace CupkekGames.BehaviourTrees
{
    public class PersistentContextNodeToggle : BTNodeAction
    {
        public string Key = "PersistentContextKey";

        protected override BTNodeRuntimeState OnUpdate(GraphFrame frame, float deltaTime)
        {
            if (BehaviourTreeManager.PersistentContext.TryGetValue(Key, out object value))
            {
                if (value is int intValue)
                    BehaviourTreeManager.PersistentContext[Key] = intValue == 0 ? 1 : 0;
            }
            else
            {
                BehaviourTreeManager.PersistentContext[Key] = 1;
            }

            return BTNodeRuntimeState.Success;
        }

        protected override void OnReset()
        {
        }
    }
}
