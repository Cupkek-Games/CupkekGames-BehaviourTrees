using System.Collections.Generic;
using CupkekGames.Graphs;
using UnityEngine;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Leaf node — no output ports. Subclasses implement the actual work
    /// in <c>OnUpdate</c>; the framework calls them as terminals of the
    /// composite/decorator tree.
    /// </summary>
    public abstract class BTNodeAction : BTNode
    {
        public override IReadOnlyList<GraphPortDef> OutputPorts => GraphPortDef.None;

        public override Color HeaderColor => new Color(0.36f, 0.42f, 0.50f);

        /// <summary>Play triangle — leaf "does something" indicator.</summary>
        public override string IconGlyph => "▶";

        public override BTNode Clone()
        {
            return Instantiate(this);
        }
    }
}
