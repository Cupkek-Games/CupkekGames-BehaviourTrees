using System.Collections.Generic;
using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Multi-child container. Children are resolved from the owning
    /// <see cref="BehaviourTree"/>'s connection list, ordered by
    /// <see cref="GraphConnection.OrderIndex"/>.
    /// </summary>
    public abstract class BTNodeComposite : BTNode
    {
        public override IReadOnlyList<GraphPortDef> OutputPorts => MultiOutput;

        public override Color HeaderColor => new Color(0.55f, 0.45f, 0.30f);

        /// <summary>Three-bar glyph — composites fan out to multiple children.</summary>
        public override string IconGlyph => "≡";

        static readonly IReadOnlyList<GraphPortDef> MultiOutput = new[]
        {
            new GraphPortDef { Capacity = PortCapacity.Multi },
        };

        /// <summary>
        /// Resolve this composite's children via the owner tree's
        /// connections, ordered by <see cref="GraphConnection.OrderIndex"/>.
        /// </summary>
        public IReadOnlyList<BTNode> GetChildren()
        {
            var result = new List<BTNode>();
            if (OwnerTree != null)
            {
                var children = GraphTopology.ChildrenOf(OwnerTree, this);
                for (int i = 0; i < children.Count; i++)
                    if (children[i] is BTNode bt) result.Add(bt);
            }
            return result;
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
