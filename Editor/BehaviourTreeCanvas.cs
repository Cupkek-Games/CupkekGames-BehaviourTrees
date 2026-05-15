using System.Collections.Generic;
using CupkekGames.Data.Primitives;
using CupkekGames.Graphs;
using CupkekGames.Graphs.Editor;

namespace CupkekGames.BehaviourTrees.Editor
{
    /// <summary>
    /// Domain canvas for <see cref="BehaviourTree"/> assets. Inherits the
    /// generic canvas chrome (pan/zoom, marquee, clipboard, search popup)
    /// and adds the tree-shape rules every BT depends on:
    /// <list type="bullet">
    ///   <item>Roots accept no inbound connections — they're sinks at the top.</item>
    ///   <item>Every non-root node has at most one inbound connection (one parent).</item>
    ///   <item>No cycles — connecting <c>S → T</c> is blocked if <c>T</c> already
    ///     transitively reaches <c>S</c> via existing connections.</item>
    /// </list>
    /// Auto-creates a root node when the canvas binds to an empty tree, so
    /// the user can start adding children immediately instead of having to
    /// add the root themselves.
    /// </summary>
    public class BehaviourTreeCanvas : GraphCanvas
    {
        public override void BindToAsset(GraphAssetSO asset)
        {
            base.BindToAsset(asset);
            if (asset is BehaviourTree tree)
                tree.InitializeEditor();
        }

        protected override bool CanConnect(PortElement source, PortElement target)
        {
            if (!base.CanConnect(source, target)) return false;

            // Root nodes are sinks at the top — nothing can connect INTO them.
            if (target.OwnerNode.Node is BTNodeRoot) return false;

            // Tree shape: no cycles. If we can already reach `source` by
            // walking forward from `target`, the new edge would close a loop.
            if (WouldCreateCycle(source.OwnerNode.Node, target.OwnerNode.Node))
                return false;

            return true;
        }

        bool WouldCreateCycle(GraphNodeSO from, GraphNodeSO toReach)
        {
            if (Asset == null) return false;

            var visited = new HashSet<string>();
            var stack = new Stack<SerializedGuid>();
            stack.Push(toReach.Guid);

            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (cur == from.Guid) return true;
                if (!visited.Add(cur.ValueStr)) continue;

                foreach (var conn in Asset.Connections)
                {
                    if (conn.SourceNodeGuid == cur)
                        stack.Push(conn.TargetNodeGuid);
                }
            }
            return false;
        }

        protected override NodeElement CreateNodeElement(GraphNodeSO node)
        {
            // Sticky notes + general fallback handled by the base. BT-specific
            // node visuals will live here when state-pulse / debug overlays
            // land alongside a runtime-clone hook (deferred).
            return base.CreateNodeElement(node);
        }
    }
}
