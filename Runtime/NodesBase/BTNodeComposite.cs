using System.Collections.Generic;
using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Multi-child container. Children are now resolved from the owning
    /// <see cref="BehaviourTree"/>'s connection list, ordered by
    /// <see cref="GraphConnection.OrderIndex"/>. The legacy
    /// <see cref="Children"/> field stays for the migration window so
    /// unmigrated assets continue to deserialize and execute; the
    /// migration tool converts it to connections, then a follow-up
    /// commit removes the field.
    /// </summary>
    public abstract class BTNodeComposite : BTNode
    {
        /// <summary>
        /// LEGACY — pre-migration child list. Kept for asset compat until
        /// the migration tool has run. Code reads should go through
        /// <see cref="GetChildren"/>.
        /// </summary>
        public List<BTNode> Children = new();

        public override IReadOnlyList<GraphPortDef> OutputPorts => MultiOutput;

        public override Color HeaderColor => new Color(0.55f, 0.45f, 0.30f);

        static readonly IReadOnlyList<GraphPortDef> MultiOutput = new[]
        {
            new GraphPortDef { Capacity = PortCapacity.Multi },
        };

        /// <summary>
        /// Resolve this composite's children via the owner tree's
        /// connections, ordered by <see cref="GraphConnection.OrderIndex"/>.
        /// Falls back to the legacy <see cref="Children"/> list if no
        /// connections exist yet — so unmigrated assets execute correctly
        /// before the migration tool runs.
        /// </summary>
        public IReadOnlyList<BTNode> GetChildren()
        {
            if (OwnerTree != null)
            {
                List<(int order, BTNode node)> matches = null;
                foreach (var conn in OwnerTree.Connections)
                {
                    if (conn.SourceNodeGuid != Guid) continue;
                    var found = OwnerTree.FindBTNode(conn.TargetNodeGuid);
                    if (found == null) continue;
                    (matches ??= new()).Add((conn.OrderIndex, found));
                }

                if (matches != null && matches.Count > 0)
                {
                    matches.Sort((a, b) => a.order.CompareTo(b.order));
                    var result = new List<BTNode>(matches.Count);
                    foreach (var (_, n) in matches) result.Add(n);
                    return result;
                }
            }
            return Children;
        }

        public override BTNode Clone()
        {
            // Connections are cloned at the BehaviourTree level. The
            // composite clone keeps its Guid + serialized fields; OwnerTree
            // is re-linked by the owner's Clone() loop afterwards.
            return Instantiate(this);
        }
    }
}
