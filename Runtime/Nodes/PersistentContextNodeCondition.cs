using System.Threading;
using CupkekGames.Graphs;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Composite that branches based on a value in
    /// <see cref="BehaviourTreeManager.PersistentContext"/>. The first
    /// child runs when the stored int matches <see cref="Condition"/>,
    /// otherwise the second child runs. The branch is locked on first
    /// evaluation and not re-checked until <see cref="OnReset"/>.
    /// </summary>
    public class PersistentContextNodeCondition : BTNodeComposite
    {
        public string Key = "PersistentContextKey";
        public int Condition = 0;

        private CancellationToken? _cancellationToken;
        private int _selected = -1;

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

            if (_selected == -1)
            {
                bool match = false;
                if (BehaviourTreeManager.PersistentContext.TryGetValue(Key, out object stored)
                    && stored is int intValue)
                {
                    match = Condition == intValue;
                }
                _selected = match ? 0 : 1;
            }

            if (_selected >= children.Count) return BTNodeRuntimeState.Success;
            var child = children[_selected];
            return child != null
                ? child.UpdateNode(frame, deltaTime)
                : BTNodeRuntimeState.Success;
        }

        protected override void OnReset()
        {
            _selected = -1;
            _cancellationToken = null;
        }
    }
}
