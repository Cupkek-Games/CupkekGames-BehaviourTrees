#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.BehaviourTrees.Editor
{
    [UxmlElement]
    public partial class BehaviourTreeView : GraphView
    {
        private BehaviourTree _behaviourTree;
        public BehaviourTree BehaviourTree => _behaviourTree;
        public Action<BTNode> OnNodeSelected;
        public BehaviourTreeView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void PopulateView(BehaviourTree behaviourTree)
        {
            _behaviourTree = behaviourTree;

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements);
            graphViewChanged += OnGraphViewChanged;

            _behaviourTree.InitializeEditor();

            ReadOnlyCollection<BTNode> nodeCollection = _behaviourTree.Nodes;

            // Create node view
            for (int i = 0; i < nodeCollection.Count; i++)
            {
                CreateNodeView(nodeCollection[i]);
            }

            // Create edges
            for (int i = 0; i < nodeCollection.Count; i++)
            {
                BTNode parent = nodeCollection[i];

                BTNodeView viewParent = GetNodeView(parent);

                List<BTNode> children = _behaviourTree.GetChildren(parent);
                for (int y = 0; y < children.Count; y++)
                {
                    BTNode child = children[y];
                    if (child == null)
                    {
                        continue;
                    }

                    BTNodeView viewChild = GetNodeView(child);

                    Edge edge = viewParent.Outputs[y].ConnectTo(viewChild.Input);

                    AddElement(edge);
                }
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node).ToList();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(e =>
                {
                    if (e is BTNodeView nodeView)
                    {
                        _behaviourTree.DeleteNode(nodeView.Node.Guid.Value());
                    }

                    if (e is Edge edge)
                    {
                        BTNodeView parentView = edge.output.node as BTNodeView;
                        BTNodeView childView = edge.input.node as BTNodeView;

                        int outputIndex = parentView.Outputs.IndexOf(edge.output);

                        _behaviourTree.RemoveChild(parentView.Node, childView.Node, outputIndex);
                    }
                });
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    BTNodeView parentView = edge.output.node as BTNodeView;

                    BTNodeView childView = edge.input.node as BTNodeView;

                    int outputIndex = parentView.Outputs.IndexOf(edge.output);

                    _behaviourTree.AddChild(parentView.Node, childView.Node, outputIndex);
                });
            }
            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            // base.BuildContextualMenu(evt);
            {
                var types = TypeCache.GetTypesDerivedFrom<BTNodeAction>();
                foreach (var type in types)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", (a) => CreateNode(type));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<BTNodeComposite>();
                foreach (var type in types)
                {
                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", (a) => CreateNode(type));
                }
            }

            {
                var types = TypeCache.GetTypesDerivedFrom<BTNodeDecorator>();
                foreach (var type in types)
                {
                    if (type == typeof(BTNodeRoot))
                    {
                        continue;
                    }

                    if (type.IsAbstract)
                    {
                        continue;
                    }

                    evt.menu.AppendAction($"{type.BaseType.Name}/{type.Name}", (a) => CreateNode(type));
                }
            }
        }
        private void CreateNode(System.Type type)
        {
            BTNode node = _behaviourTree.CreateNode(type);

            CreateNodeView(node);
        }
        private void CreateNodeView(BTNode node)
        {
            BTNodeView nodeView = new BTNodeView();

            nodeView.SetBehaviourTreeView(this);
            nodeView.SetNode(node);

            AddElement(nodeView);
        }

        private BTNodeView GetNodeView(BTNode node)
        {
            return GetNodeByGuid(node.Guid.ValueStr) as BTNodeView;
        }

        public void UpdateNodeStates()
        {
            nodes.ForEach(n =>
            {
                BTNodeView view = n as BTNodeView;

                view.UpdateStateStyle();
            });
        }
    }
}
#endif