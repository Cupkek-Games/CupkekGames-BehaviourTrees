using System.Collections.Generic;
using CupkekGames.Data.Primitives;
using CupkekGames.Graphs;
using UnityEditor;
using UnityEngine;

namespace CupkekGames.BehaviourTrees.Editor
{
    /// <summary>
    /// One-shot conversion of pre-graphs-foundation BT assets. Walks every
    /// composite's <c>Children</c> list and every decorator's <c>Child</c>
    /// reference; emits an equivalent <see cref="GraphConnection"/> on the
    /// owning <see cref="BehaviourTree"/>; then clears the legacy fields so
    /// the new connection-based shape is the single source of truth.
    /// </summary>
    /// <remarks>
    /// Run once per project after pulling the graphs-foundation migration.
    /// After running and verifying assets, the legacy <c>Children</c> /
    /// <c>Child</c> fields can be deleted from the runtime code in the 4G
    /// cleanup commit — assets will no longer rely on them.
    /// </remarks>
    public static class BehaviourTreeMigrationTool
    {
        [MenuItem("CupkekGames/BehaviourTrees/Migrate Legacy Trees")]
        public static void MigrateAllInProject()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(BehaviourTree));
            int migrated = 0;
            int unchanged = 0;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                EditorUtility.DisplayProgressBar(
                    "Migrating BehaviourTrees",
                    path,
                    (float)i / Mathf.Max(1, guids.Length));

                BehaviourTree tree = AssetDatabase.LoadAssetAtPath<BehaviourTree>(path);
                if (tree == null) continue;

                if (MigrateTree(tree)) migrated++;
                else unchanged++;
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.SaveAssets();

            Debug.Log($"BehaviourTree migration complete — {migrated} converted, {unchanged} already up-to-date.");
        }

        [MenuItem("CupkekGames/BehaviourTrees/Migrate Selected Tree")]
        public static void MigrateSelected()
        {
            if (Selection.activeObject is BehaviourTree tree)
            {
                bool changed = MigrateTree(tree);
                AssetDatabase.SaveAssets();
                Debug.Log(changed
                    ? $"Migrated {tree.name}."
                    : $"{tree.name} had no legacy data — nothing to migrate.");
            }
            else
            {
                Debug.LogWarning("Select a BehaviourTree (or BehaviourTree subclass) asset first.");
            }
        }

        // ---------------------------------------------------------------

        public static bool MigrateTree(BehaviourTree tree)
        {
            if (tree == null) return false;

            Undo.RegisterCompleteObjectUndo(tree, "Migrate Behaviour Tree");

            bool anyChange = false;

            // Snapshot the node list — the loop mutates Connections via
            // AddConnection which doesn't touch _nodes, but defensive copy
            // is cheap and avoids any surprises.
            var snapshot = new List<GraphNodeSO>(tree.Nodes);

            foreach (var n in snapshot)
            {
                if (n is BTNodeComposite composite)
                {
                    anyChange |= MigrateComposite(tree, composite);
                }
                else if (n is BTNodeDecorator decorator)
                {
                    anyChange |= MigrateDecorator(tree, decorator);
                }
            }

            if (anyChange)
            {
                EditorUtility.SetDirty(tree);
            }
            return anyChange;
        }

        static bool MigrateComposite(BehaviourTree tree, BTNodeComposite composite)
        {
            var legacy = composite.Children;
            if (legacy == null || legacy.Count == 0) return false;

            bool changed = false;
            int nextOrder = NextOrderIndex(tree, composite.Guid);

            foreach (var child in legacy)
            {
                if (child == null) continue;
                if (ConnectionExists(tree, composite.Guid, child.Guid)) continue;

                tree.AddConnection(new GraphConnection
                {
                    SourceNodeGuid = composite.Guid,
                    TargetNodeGuid = child.Guid,
                    OrderIndex = nextOrder++,
                });
                changed = true;
            }

            // Always clear the legacy list once we've made it past the read
            // — even if every child was already connected, the legacy list
            // shouldn't keep dangling references.
            composite.Children.Clear();
            Undo.RecordObject(composite, "Migrate Composite");
            EditorUtility.SetDirty(composite);

            return changed;
        }

        static bool MigrateDecorator(BehaviourTree tree, BTNodeDecorator decorator)
        {
            var legacy = decorator.Child;
            if (legacy == null) return false;

            bool changed = false;
            if (!ConnectionExists(tree, decorator.Guid, legacy.Guid))
            {
                tree.AddConnection(new GraphConnection
                {
                    SourceNodeGuid = decorator.Guid,
                    TargetNodeGuid = legacy.Guid,
                    OrderIndex = 0,
                });
                changed = true;
            }

            decorator.Child = null;
            Undo.RecordObject(decorator, "Migrate Decorator");
            EditorUtility.SetDirty(decorator);

            return changed;
        }

        static bool ConnectionExists(BehaviourTree tree, SerializedGuid src, SerializedGuid dst)
        {
            foreach (var c in tree.Connections)
            {
                if (c.SourceNodeGuid == src && c.TargetNodeGuid == dst)
                    return true;
            }
            return false;
        }

        static int NextOrderIndex(BehaviourTree tree, SerializedGuid src)
        {
            int max = -1;
            foreach (var c in tree.Connections)
            {
                if (c.SourceNodeGuid == src && c.OrderIndex > max)
                    max = c.OrderIndex;
            }
            return max + 1;
        }
    }
}
