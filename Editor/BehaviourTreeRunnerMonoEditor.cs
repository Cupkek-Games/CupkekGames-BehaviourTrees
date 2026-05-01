#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace CupkekGames.BehaviourTrees.Editor
{
    [CustomEditor(typeof(BehaviourTreeRunnerMono))]
    public class BehaviourTreeRunnerMonoEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // Draw the default inspector layout
            DrawDefaultInspector();

            // Add a space before the button
            GUILayout.Space(10);

            // Get a reference to the target script
            BehaviourTreeRunnerMono myComponent = (BehaviourTreeRunnerMono)target;

            // Add a button and check if it was clicked
            if (GUILayout.Button("Reset Tree"))
            {
                myComponent.Runner.ResetTree();
            }
        }
        public override VisualElement CreateInspectorGUI()
        {
            // Create a root VisualElement container
            VisualElement root = new VisualElement();

            // Draw default inspector elements
            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            // Create a button
            Button myButton = new Button(() =>
            {
                // Call the method on the target MonoBehaviour when the button is clicked
                ((BehaviourTreeRunnerMono)target).Runner.ResetTree();
            })
            {
                text = "Reset Tree"
            };

            // Add the button to the root element
            root.Add(myButton);

            return root;
        }
    }
}
#endif