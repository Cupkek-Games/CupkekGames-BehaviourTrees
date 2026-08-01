using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Single-child wrapper. Child is resolved from the owning
    /// <see cref="BehaviourTree"/>'s connection list at runtime.
    /// </summary>
    public abstract class BTNodeDecorator : BTNode
    {
        public override Color HeaderColor => new Color(0.45f, 0.30f, 0.55f);

        /// <summary>Diamond — wraps/modifies a single child.</summary>
        public override string IconGlyph => "◆";

        /// <summary>
        /// Resolve this decorator's single child (lowest OrderIndex) via the
        /// owner tree's connections.
        /// </summary>
        public BTNode GetChild()
        {
            if (OwnerTree != null)
            {
                var children = GraphTopology.ChildrenOf(OwnerTree, this);
                for (int i = 0; i < children.Count; i++)
                    if (children[i] is BTNode bt) return bt;
            }
            return null;
        }

        public override BTNode Clone()
        {
            // Connections are cloned at the BehaviourTree level. The
            // decorator clone keeps its Guid + serialized fields; the
            // owner's Clone() loops re-link OwnerTree afterwards.
            return Instantiate(this);
        }
    }
}
