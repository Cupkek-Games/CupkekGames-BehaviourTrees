using System.Collections.Generic;
using System.Threading;
using CupkekGames.Graphs;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Shared helper for the three-token cancel-check pattern every BT
    /// composite / decorator used to inline. Reads:
    /// <list type="bullet">
    ///   <item><c>CancellationToken</c> — combat-wide</item>
    ///   <item><c>CancellationTokenCasterDeath</c> — caster died</item>
    ///   <item><c>CancellationTokenCasterInterrupt</c> — caster was interrupted</item>
    /// </list>
    /// Whichever ones are present on the frame get linked into a single
    /// token that nodes cache for the duration of their run.
    /// </summary>
    public static class BTCancellation
    {
        public const string KeyCombat = "CancellationToken";
        public const string KeyCasterDeath = "CancellationTokenCasterDeath";
        public const string KeyCasterInterrupt = "CancellationTokenCasterInterrupt";

        /// <summary>
        /// Build a linked cancellation token from the three standard frame
        /// keys. Returns <c>false</c> when any of the source tokens has
        /// already requested cancellation — caller should fail immediately.
        /// </summary>
        public static bool TryCreateLinkedToken(GraphFrame frame, out CancellationToken linked)
        {
            if (frame == null)
            {
                linked = CancellationToken.None;
                return true;
            }

            var sources = new List<CancellationToken>(3);

            if (frame.TryGet<CancellationToken>(KeyCombat, out var ct1))
            {
                if (ct1.IsCancellationRequested) { linked = ct1; return false; }
                sources.Add(ct1);
            }
            if (frame.TryGet<CancellationToken>(KeyCasterDeath, out var ct2))
            {
                if (ct2.IsCancellationRequested) { linked = ct2; return false; }
                sources.Add(ct2);
            }
            if (frame.TryGet<CancellationToken>(KeyCasterInterrupt, out var ct3))
            {
                if (ct3.IsCancellationRequested) { linked = ct3; return false; }
                sources.Add(ct3);
            }

            linked = sources.Count switch
            {
                0 => CancellationToken.None,
                1 => sources[0],
                _ => CancellationTokenSource.CreateLinkedTokenSource(sources.ToArray()).Token,
            };
            return true;
        }
    }
}
