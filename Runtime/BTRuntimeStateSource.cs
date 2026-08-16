#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using CupkekGames.Graphs;
using UnityEngine;
using Unity.Scripting.LifecycleManagement;

namespace CupkekGames.BehaviourTrees
{
    /// <summary>
    /// Editor-only runtime-state source for a <see cref="BehaviourTree"/>: paints each
    /// node's live status onto the graph canvas in play mode — amber <c>running</c>,
    /// green <c>ok</c>, red <c>fail</c>, nothing for idle. BT is multi-instance (many
    /// agents run one tree), so it exposes the matching live
    /// <see cref="BehaviourTreeRunner"/>s as instances and the canvas shows a picker.
    /// Continuous (state changes every tick), so it implements
    /// <see cref="IGraphRuntimePollable"/> and is ticked by the canvas — no
    /// <c>EditorApplication</c> dependency here. Replaces the old per-node
    /// <c>BTNodeElement</c> indicator (which polled EditorApplication.update once per
    /// node). Matches runners to this graph by node <c>Guid</c>.
    /// </summary>
    internal sealed partial class BTRuntimeStateSource : IGraphRuntimeStateSource, IGraphRuntimeInstanceSelector, IGraphRuntimePollable
    {
        static readonly Color RunningColor = new Color(1f, 0.83f, 0.40f);
        static readonly Color SuccessColor = new Color(0.45f, 0.85f, 0.50f);
        static readonly Color FailColor    = new Color(0.95f, 0.40f, 0.40f);
        [NoAutoStaticsCleanup]
        static readonly NodeBadge RunningBadge = new NodeBadge("running", new Color(0.62f, 0.50f, 0.22f), "Currently executing.");
        [NoAutoStaticsCleanup]
        static readonly NodeBadge SuccessBadge = new NodeBadge("ok",      new Color(0.30f, 0.55f, 0.34f), "Last tick: success.");
        [NoAutoStaticsCleanup]
        static readonly NodeBadge FailBadge    = new NodeBadge("fail",    new Color(0.60f, 0.28f, 0.28f), "Last tick: failure.");

        readonly HashSet<string> _myGuids = new();
        readonly List<BehaviourTreeRunner> _matching = new();
        readonly List<string> _labels = new();
        int _selected;
        int _signature;

        public event Action Changed;

        public BTRuntimeStateSource(BehaviourTree tree)
        {
            if (tree != null)
                foreach (var n in tree.Nodes)
                    if (n != null) _myGuids.Add(n.Guid.ValueStr);
            RefreshMatching();
            _signature = ComputeSignature();
        }

        public bool IsLive => Application.isPlaying && _matching.Count > 0;

        public IReadOnlyList<string> InstanceLabels => _labels;

        public int SelectedInstance
        {
            get => _selected;
            set
            {
                int v = Mathf.Clamp(value, 0, Mathf.Max(0, _matching.Count - 1));
                if (v == _selected) return;
                _selected = v;
                _signature = ComputeSignature();
                Changed?.Invoke();
            }
        }

        public bool TryGetState(GraphNodeSO node, out GraphNodeRuntimeState state)
        {
            state = default;
            if (node == null) return false;
            var runner = (_selected >= 0 && _selected < _matching.Count) ? _matching[_selected] : null;
            if (runner == null) return false;

            switch (StateForGuid(runner, node.Guid.ValueStr))
            {
                case BTNodeRuntimeState.Running: state = new GraphNodeRuntimeState(RunningColor, RunningBadge); return true;
                case BTNodeRuntimeState.Success: state = new GraphNodeRuntimeState(SuccessColor, SuccessBadge); return true;
                case BTNodeRuntimeState.Fail:    state = new GraphNodeRuntimeState(FailColor, FailBadge); return true;
                default: return false; // Idle → no overlay
            }
        }

        // Ticked by the canvas (continuous source). Coalesces — only raises Changed
        // when the matching runners or the selected runner's node states changed.
        public void Poll()
        {
            RefreshMatching();
            int sig = ComputeSignature();
            if (sig != _signature) { _signature = sig; Changed?.Invoke(); }
        }

        public void Dispose() => Changed = null;

        void RefreshMatching()
        {
            _matching.Clear();
            var all = BehaviourTreeRunner.ActiveRunners;
            for (int i = 0; i < all.Count; i++)
            {
                var r = all[i];
                if (r != null && r.RuntimeClone != null && RunnerMatches(r)) _matching.Add(r);
            }
            if (_selected >= _matching.Count) _selected = Mathf.Max(0, _matching.Count - 1);

            _labels.Clear();
            for (int i = 0; i < _matching.Count; i++) _labels.Add("Runner " + (i + 1));
        }

        bool RunnerMatches(BehaviourTreeRunner r)
        {
            var nodes = r.RuntimeClone.Nodes;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i] != null && _myGuids.Contains(nodes[i].Guid.ValueStr)) return true;
            return false;
        }

        int ComputeSignature()
        {
            unchecked
            {
                int h = 17;
                h = h * 31 + _matching.Count;
                h = h * 31 + _selected;
                var runner = (_selected >= 0 && _selected < _matching.Count) ? _matching[_selected] : null;
                if (runner != null)
                {
                    var nodes = runner.RuntimeClone.Nodes;
                    for (int i = 0; i < nodes.Count; i++)
                        if (nodes[i] is BTNode bt) h = h * 31 + ((int)bt.State + 1);
                }
                return h;
            }
        }

        static BTNodeRuntimeState StateForGuid(BehaviourTreeRunner runner, string guid)
        {
            var nodes = runner.RuntimeClone.Nodes;
            for (int i = 0; i < nodes.Count; i++)
                if (nodes[i] is BTNode bt && bt.Guid.ValueStr == guid) return bt.State;
            return BTNodeRuntimeState.Idle;
        }
    }
}
#endif
