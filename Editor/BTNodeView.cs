#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace CupkekGames.BehaviourTrees.Editor
{
    [UxmlElement]
    public partial class BTNodeView : Node
    {
        private BehaviourTreeView _behaviourTreeView;
        public BTNode Node;
        public Port Input;
        public List<Port> Outputs = new();

        private ProgressBar _stateProgressBar;

        public BTNodeView()
        {
            _stateProgressBar = new ProgressBar();
            _stateProgressBar.AddToClassList("stateElement");

            _stateProgressBar.lowValue = 0;
            _stateProgressBar.highValue = 1;
            _stateProgressBar.value = 1;

            Add(_stateProgressBar);
        }

        public void SetBehaviourTreeView(BehaviourTreeView view)
        {
            _behaviourTreeView = view;
        }

        public void SetNode(BTNode node)
        {
            Node = node;
            title = node.name;

            this.viewDataKey = node.Guid.ValueStr;

            style.left = node.Position.x;
            style.top = node.Position.y;

            CreateInputPorts();
            CreateOutputPorts();

            if (Node is BTNodeRoot)
            {
                AddToClassList("root");
            }
            else if (Node is BTNodeAction)
            {
                AddToClassList("action");
            }
            else if (Node is BTNodeComposite)
            {
                AddToClassList("composite");
            }
            else if (Node is BTNodeDecorator)
            {
                AddToClassList("decorator");
            }

            UpdateStateStyle();
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            Node.Position = newPos.min;

            EditorUtility.SetDirty(Node);
        }

        private void CreateInputPorts()
        {
            if (Node is BTNodeRoot)
            {
                Input = null;
            }
            else if (Node is BTNodeAction)
            {
                Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            }
            else if (Node is BTNodeComposite)
            {
                Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            }
            else if (Node is BTNodeDecorator)
            {
                Input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(bool));
            }

            if (Input != null)
            {
                Input.portName = "";
                inputContainer.Add(Input);
            }
        }

        private void CreateOutputPorts()
        {
            Outputs = new();
            outputContainer.Clear();

            if (Node is BTNodeComposite composite)
            {
                var button = new Button(() =>
                {
                    if (composite.Children.Count == 0)
                    {
                        return;
                    }

                    int index = composite.Children.Count - 1;

                    Port port = Outputs[index];
                    foreach (Edge edge in port.connections)
                    {
                        edge.input.Disconnect(edge);
                        _behaviourTreeView.RemoveElement(edge);
                    }
                    port.DisconnectAll();

                    composite.Children.RemoveAt(index);
                    Outputs.RemoveAt(index);
                    outputContainer.RemoveAt(index + 1);
                });
                button.text = "Remove Pin";
                outputContainer.Add(button);

                for (int i = 0; i < composite.Children.Count; i++)
                {
                    AddOutputPort(i.ToString());
                }

                button = new Button(() =>
                {
                    composite.Children.Add(null);
                    AddOutputPort((composite.Children.Count - 1).ToString(), outputContainer.childCount - 1);
                });
                button.text = "Add Pin";
                outputContainer.Add(button);
            }
            else if (Node is BTNodeDecorator)
            {
                AddOutputPort("");
            }

            EditorUtility.SetDirty(Node);
        }

        public override void OnSelected()
        {
            base.OnSelected();

            _behaviourTreeView?.OnNodeSelected?.Invoke(Node);
        }


        private void AddOutputPort(string name, int index = -1)
        {
            var output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));

            output.portName = name;

            Outputs.Add(output);

            if (index == -1)
            {
                outputContainer.Add(output);
            }
            else
            {
                outputContainer.Insert(index, output);
            }
        }

        public void UpdateStateStyle()
        {
            RemoveFromClassList("runtime");
            if (Application.isPlaying)
            {
                AddToClassList("runtime");
            }

            BTNodeRuntimeState state = Node.State;

            RemoveFromClassList("state-idle");
            RemoveFromClassList("state-running");
            RemoveFromClassList("state-success");
            RemoveFromClassList("state-fail");

            if (state == BTNodeRuntimeState.Idle)
            {
                AddToClassList("state-idle");
            }
            else if (state == BTNodeRuntimeState.Running)
            {
                AddToClassList("state-running");
            }
            else if (state == BTNodeRuntimeState.Success)
            {
                AddToClassList("state-success");
            }
            else if (state == BTNodeRuntimeState.Fail)
            {
                AddToClassList("state-fail");
            }

            _stateProgressBar.title = state.ToString();
        }
    }
}
#endif