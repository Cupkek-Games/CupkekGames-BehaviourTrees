using CupkekGames.Graphs;
using CupkekGames.Graphs.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.BehaviourTrees.Editor
{
    /// <summary>
    /// BT-specific node element. Inherits the foundation's card visuals
    /// (header colour comes from <see cref="BTNode.HeaderColor"/> overrides
    /// per type — root green, composite amber, decorator violet, action
    /// grey) and adds a small state indicator in the top-right corner that
    /// mirrors the runtime clone's <see cref="BTNode.State"/> during play.
    ///
    /// The asset-time node SO never executes — its <c>State</c> stays
    /// <see cref="BTNodeRuntimeState.Idle"/>. To show runtime state we look
    /// up an active <see cref="BehaviourTreeRunner"/> whose
    /// <c>OriginalTree</c> matches the asset that owns this node, then
    /// find the matching clone node by Guid (Guid is preserved on clone).
    /// </summary>
    public class BTNodeElement : NodeElement
    {
        const float IndicatorSize = 10f;

        static readonly Color IdleColor = new Color(0.40f, 0.40f, 0.45f);
        static readonly Color RunningColor = new Color(1f, 0.83f, 0.40f);
        static readonly Color SuccessColor = new Color(0.45f, 0.85f, 0.50f);
        static readonly Color FailColor = new Color(0.95f, 0.40f, 0.40f);

        readonly BTNode _btNode;
        readonly VisualElement _stateIndicator;
        BTNodeRuntimeState _lastState = BTNodeRuntimeState.Idle;

        public BTNodeElement(GraphCanvas canvas, BTNode node)
            : base(canvas, node)
        {
            _btNode = node;

            _stateIndicator = new VisualElement
            {
                style =
                {
                    position = Position.Absolute,
                    top = 6f,
                    right = 6f,
                    width = IndicatorSize,
                    height = IndicatorSize,
                    borderTopLeftRadius = IndicatorSize * 0.5f,
                    borderTopRightRadius = IndicatorSize * 0.5f,
                    borderBottomLeftRadius = IndicatorSize * 0.5f,
                    borderBottomRightRadius = IndicatorSize * 0.5f,
                    display = DisplayStyle.None,
                },
                pickingMode = PickingMode.Ignore,
            };
            Add(_stateIndicator);

            EditorApplication.update += OnEditorUpdate;
            RegisterCallback<DetachFromPanelEvent>(_ => EditorApplication.update -= OnEditorUpdate);
        }

        void OnEditorUpdate()
        {
            if (_btNode == null || !Application.isPlaying)
            {
                if (_stateIndicator.style.display != DisplayStyle.None)
                    _stateIndicator.style.display = DisplayStyle.None;
                return;
            }

            var runtimeState = FindRuntimeState(_btNode);
            if (_stateIndicator.style.display == DisplayStyle.None)
                _stateIndicator.style.display = DisplayStyle.Flex;
            if (runtimeState != _lastState)
            {
                _lastState = runtimeState;
                _stateIndicator.style.backgroundColor = ColorFor(runtimeState);
            }
        }

        static BTNodeRuntimeState FindRuntimeState(BTNode assetNode)
        {
            // The owning BehaviourTree is the main asset at this subasset's path.
            string path = AssetDatabase.GetAssetPath(assetNode);
            if (string.IsNullOrEmpty(path)) return BTNodeRuntimeState.Idle;

            var ownerTree = AssetDatabase.LoadMainAssetAtPath(path) as BehaviourTree;
            if (ownerTree == null) return BTNodeRuntimeState.Idle;

            // First runner with a matching original tree wins. Multiple
            // concurrent runners of the same asset all show the same node
            // visual; v1 doesn't try to disambiguate which one.
            foreach (var runner in BehaviourTreeRunner.ActiveRunners)
            {
                if (runner.OriginalTree != ownerTree) continue;

                foreach (var n in runner.RuntimeClone.Nodes)
                {
                    if (n is BTNode bt && bt.Guid == assetNode.Guid)
                        return bt.State;
                }
            }
            return BTNodeRuntimeState.Idle;
        }

        static Color ColorFor(BTNodeRuntimeState state) => state switch
        {
            BTNodeRuntimeState.Running => RunningColor,
            BTNodeRuntimeState.Success => SuccessColor,
            BTNodeRuntimeState.Fail => FailColor,
            _ => IdleColor,
        };
    }
}
