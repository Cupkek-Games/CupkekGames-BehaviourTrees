#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Callbacks;

namespace CupkekGames.BehaviourTrees.Editor
{
    public class BehaviourTreeEditor : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _container;
        [SerializeField] private VisualTreeAsset _tree;
        [SerializeField] private VisualTreeAsset _leftPanel;

        private BehaviourTreeView _behaviourTreeView;
        public BehaviourTreeView BehaviourTreeView => _behaviourTreeView;
        private VisualElement _leftPanelInstance;
        private VisualElement _treeInstance;
        private VisualElement _leftEditorContainer;
        private UnityEditor.Editor _editor;
        public BehaviourTree OriginalTree = null;
        public static event Action<BehaviourTreeEditor> OnGameObjectSelect;

        // Priority 200 — group 3: authoring tools (behaviour tree graph editor).
        [MenuItem("Tools/CupkekGames/BehaviourTreeEditor", false, 200)]
        public static void OpenWindow()
        {
            BehaviourTreeEditor wnd = GetWindow<BehaviourTreeEditor>();
            wnd.titleContent = new GUIContent("BehaviourTreeEditor");
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is BehaviourTree)
            {
                OpenWindow();

                return true;
            }

            return false;
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            VisualElement container = _container != null ? _container.Instantiate() : new VisualElement();
            container.style.flexGrow = 1;
            root.Add(container);

            TwoPaneSplitView split = new TwoPaneSplitView();
            split.style.flexGrow = 1;
            container.Add(split);

            _leftPanelInstance = _leftPanel != null ? _leftPanel.Instantiate() : new Label("_leftPanel null");
            split.Add(_leftPanelInstance);

            _treeInstance = _tree != null ? _tree.Instantiate() : new Label("_tree null");
            split.Add(_treeInstance);

            _behaviourTreeView = _treeInstance.Q<BehaviourTreeView>();
            _behaviourTreeView.OnNodeSelected += OnNodeSelectionChanged;

            OnSelectionChange();

            _leftEditorContainer = _leftPanelInstance.Q<VisualElement>("EditorContainer");
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredEditMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    OnSelectionChange();
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    if (OriginalTree != null)
                    {
                        _behaviourTreeView.PopulateView(OriginalTree);
                        OriginalTree = null;
                    }
                    break;
            }
        }

        private void OnNodeSelectionChanged(BTNode node)
        {
            _leftEditorContainer.Clear();

            if (node == null)
            {
                return;
            }

            if (_editor != null)
            {
                UnityEngine.Object.DestroyImmediate(_editor);
            }

            _editor = UnityEditor.Editor.CreateEditor(node);

            IMGUIContainer container = new IMGUIContainer(() => { _editor.OnInspectorGUI(); });

            _leftEditorContainer.Add(container);
        }

        void OnSelectionChange()
        {
            if (Application.isPlaying)
            {
                if (Selection.activeGameObject)
                {
                    BehaviourTreeRunnerMono runner = Selection.activeGameObject.GetComponent<BehaviourTreeRunnerMono>();

                    if (runner)
                    {
                        _behaviourTreeView.PopulateView(runner.Runner.RuntimeClone);
                        OriginalTree = runner.BehaviourTree;
                    }
                    else
                    {
                        OnGameObjectSelect?.Invoke(this);
                    }
                }
            }

            BehaviourTree tree = Selection.activeObject as BehaviourTree;

            if (tree && AssetDatabase.CanOpenAssetInEditor(tree.GetInstanceID()))
            {
                _behaviourTreeView.PopulateView(tree);
                OriginalTree = null;
            }
        }

        void OnInspectorUpdate()
        {
            _behaviourTreeView?.UpdateNodeStates();
        }
    }
}
#endif