using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Single-child wrapper. Child is now resolved from the owning
    /// <see cref="BehaviourTree"/>'s connection list at runtime rather
    /// than via a direct reference field. The legacy <see cref="Child"/>
    /// field stays for the migration window so unmigrated assets continue
    /// to deserialize; the migration tool converts it to a connection,
    /// then a follow-up commit removes the field.
    /// </summary>
    public abstract class BTNodeDecorator : BTNode
    {
        /// <summary>
        /// LEGACY — pre-migration child reference. Kept for asset compat
        /// until the migration tool has run. Code reads should go through
        /// <see cref="GetChild"/>.
        /// </summary>
        public BTNode Child;

        public override Color HeaderColor => new Color(0.45f, 0.30f, 0.55f);

        /// <summary>Diamond — wraps/modifies a single child.</summary>
        public override string IconGlyph => "◆";

        /// <summary>
        /// Resolve this decorator's single child via the owner tree's
        /// connections. Falls back to the legacy <see cref="Child"/> field
        /// if no connection exists yet — so unmigrated assets execute
        /// correctly before the migration tool runs.
        /// </summary>
        public BTNode GetChild()
        {
            if (OwnerTree != null)
            {
                // Single child via the shared GraphTopology walk (lowest
                // OrderIndex); the legacy Child fallback stays until migration.
                var children = GraphTopology.ChildrenOf(OwnerTree, this);
                for (int i = 0; i < children.Count; i++)
                    if (children[i] is BTNode bt) return bt;
            }
            return Child;
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
