using System;
using System.Collections.Generic;
using System.Linq;
using CupkekGames.Data.Primitives;
using CupkekGames.Graphs;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Tree-shaped graph asset. Now a <see cref="GraphAssetSO"/> — nodes
    /// and connections live on the base class; <see cref="BehaviourTree"/>
    /// adds the runtime traversal, the convenience root reference, and
    /// the runtime-clone bookkeeping.
    /// </summary>
    [CreateAssetMenu(fileName = "BehaviourTree", menuName = "CupkekGames/BehaviourTree/BehaviourTree")]
    public class BehaviourTree : GraphAssetSO, IBlackboardOwner
    {
        private BTNodeRuntimeState _state = BTNodeRuntimeState.Running;
        public BTNodeRuntimeState State => _state;

        [SerializeField] private BTNodeRoot _rootNode = null;
        public BTNodeRoot RootNode => _rootNode;

        [SerializeReference]
        [SerializeField]
        private List<BlackboardVariable> _blackboardVariables = new List<BlackboardVariable>();

        public IReadOnlyList<BlackboardVariable> BlackboardVariables => _blackboardVariables;

        public override Type NodeBaseType => typeof(BTNode);

        public override Type EditorCanvasType
        {
            get
            {
                // Resolve by name to avoid a Runtime→Editor compile-time
                // dependency (BehaviourTreeCanvas lives in the editor asmdef).
                // GraphEditorWindow falls back to GraphCanvas if the type
                // isn't found.
                return Type.GetType("CupkekGames.BehaviourTrees.Editor.BehaviourTreeCanvas, CupkekGames.BehaviourTrees.Editor");
            }
        }

        // ---------------------------------------------------------------
        // Editor convenience
        // ---------------------------------------------------------------

        /// <summary>
        /// Spawn a root node if this tree doesn't have one yet. Called by
        /// the BT canvas (Phase 4E) when first binding to an empty tree.
        /// </summary>
        public void InitializeEditor()
        {
#if UNITY_EDITOR
            if (_rootNode != null) return;

            var root = ScriptableObject.CreateInstance<BTNodeRoot>();
            root.name = nameof(BTNodeRoot);
            root.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(root, this);
            AddNode(root);
            _rootNode = root;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        // ---------------------------------------------------------------
        // Validation — surfaces tree-shape errors in the editor footer
        // ---------------------------------------------------------------

        public override IEnumerable<GraphValidationIssue> Validate()
        {
            if (_rootNode == null)
            {
                yield return new GraphValidationIssue
                {
                    Severity = GraphValidationIssue.SeverityLevel.Error,
                    Message = "Tree has no root node.",
                };
            }

            // Inbound count per node — every non-root node must have exactly one parent.
            var inbound = new Dictionary<string, int>();
            foreach (var c in Connections)
            {
                inbound.TryGetValue(c.TargetNodeGuid.ValueStr, out var n);
                inbound[c.TargetNodeGuid.ValueStr] = n + 1;
            }

            foreach (var node in Nodes)
            {
                if (node == null) continue;
                if (node is BTNodeRoot) continue;

                inbound.TryGetValue(node.Guid.ValueStr, out var count);
                if (count == 0)
                {
                    yield return new GraphValidationIssue
                    {
                        Severity = GraphValidationIssue.SeverityLevel.Warning,
                        Message = $"Orphan node — \"{node.DisplayTitle}\" has no parent.",
                        TargetNodeGuid = node.Guid,
                    };
                }
                else if (count > 1)
                {
                    yield return new GraphValidationIssue
                    {
                        Severity = GraphValidationIssue.SeverityLevel.Error,
                        Message = $"\"{node.DisplayTitle}\" has {count} parents — every BT node may have at most one.",
                        TargetNodeGuid = node.Guid,
                    };
                }
            }

            // Decorators (Single-capacity output) must not have multiple outbound.
            var outboundByNode = Connections
                .GroupBy(c => c.SourceNodeGuid.ValueStr)
                .ToDictionary(g => g.Key, g => g.Count());

            foreach (var node in Nodes)
            {
                if (node is BTNodeDecorator dec && !(node is BTNodeComposite))
                {
                    outboundByNode.TryGetValue(node.Guid.ValueStr, out var outCount);
                    if (outCount > 1)
                    {
                        yield return new GraphValidationIssue
                        {
                            Severity = GraphValidationIssue.SeverityLevel.Error,
                            Message = $"Decorator \"{node.DisplayTitle}\" has {outCount} children — decorators may have at most one.",
                            TargetNodeGuid = node.Guid,
                        };
                    }
                }
            }
        }

        // ---------------------------------------------------------------
        // Hooks from GraphAssetSO
        // ---------------------------------------------------------------

        public override void OnNodeAdded(GraphNodeSO node)
        {
            if (node is BTNodeRoot root && _rootNode == null)
                _rootNode = root;
        }

        public override void OnNodeRemoved(GraphNodeSO node)
        {
            if (ReferenceEquals(node, _rootNode))
                _rootNode = null;
        }

        // ---------------------------------------------------------------
        // Connection helpers
        // ---------------------------------------------------------------

        /// <summary>Find a node SO by GUID. Used by composite/decorator helpers to resolve children at runtime.</summary>
        public BTNode FindBTNode(SerializedGuid guid)
        {
            foreach (var n in Nodes)
            {
                if (n is BTNode bt && bt.Guid == guid)
                    return bt;
            }
            return null;
        }

        /// <summary>Children of <paramref name="parent"/> via connections (or legacy fields as fallback).</summary>
        public IReadOnlyList<BTNode> GetChildren(BTNode parent)
        {
            return parent switch
            {
                BTNodeComposite composite => composite.GetChildren(),
                BTNodeDecorator decorator => decorator.GetChild() is { } single
                    ? new List<BTNode> { single }
                    : (IReadOnlyList<BTNode>)Array.Empty<BTNode>(),
                _ => Array.Empty<BTNode>(),
            };
        }

        /// <summary>
        /// Depth-first traversal from <paramref name="node"/>. Walks
        /// children via connections (or legacy fields).
        /// </summary>
        public void Traverse(BTNode node, Action<BTNode> visitor)
        {
            if (node == null || visitor == null) return;
            visitor(node);
            foreach (var c in GetChildren(node))
                Traverse(c, visitor);
        }

        // ---------------------------------------------------------------
        // Runtime
        // ---------------------------------------------------------------

        public BTNodeRuntimeState UpdateTree(GraphFrame frame, float deltaTime)
        {
            if (_state != BTNodeRuntimeState.Running) return _state;
            if (_rootNode == null) return BTNodeRuntimeState.Success;

            _state = _rootNode.UpdateNode(frame, deltaTime);
            return _state;
        }

        public void ResetTree()
        {
            foreach (var n in Nodes)
                if (n is BTNode bt) bt.ResetNode();
            _state = BTNodeRuntimeState.Running;
        }

        /// <summary>
        /// Build a runtime clone: a fresh <see cref="BehaviourTree"/>
        /// instance with duplicated node SOs (each keeps the original's
        /// Guid so connections stay valid) and a duplicated connection
        /// list. OwnerTree is wired up on every cloned node so
        /// composite / decorator child lookups work without re-running
        /// the connection list manually.
        /// </summary>
        public BehaviourTree Clone()
        {
            var clone = ScriptableObject.CreateInstance<BehaviourTree>();
            clone.name = name;

            // Clone every BTNode SO, keeping each one's Guid so the
            // original connection list remains valid against the clone's
            // node set.
            foreach (var n in Nodes)
            {
                if (n is not BTNode bt) continue;
                var copy = bt.Clone();
                copy.OwnerTree = clone;
                clone.AddNode(copy);
                if (bt == _rootNode) clone._rootNode = (BTNodeRoot)copy;
            }

            // Copy connections verbatim — they reference node Guids, which
            // we preserved on clone, so they keep pointing at the right
            // (cloned) nodes inside the clone.
            foreach (var conn in Connections)
            {
                clone.AddConnection(new GraphConnection
                {
                    Guid = conn.Guid,
                    SourceNodeGuid = conn.SourceNodeGuid,
                    TargetNodeGuid = conn.TargetNodeGuid,
                    SourcePortId = conn.SourcePortId,
                    TargetPortId = conn.TargetPortId,
                    Label = conn.Label,
                    OrderIndex = conn.OrderIndex,
                });
            }

            return clone;
        }
    }
}
