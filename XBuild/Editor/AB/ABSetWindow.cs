

using UnityEngine;
using UnityEditor;

namespace XBuild.AB
{
    internal class ABSetWindow : EditorWindow
    {
        [MenuItem("Assets/SetABName")]
        private static void ShowWindow()
        {
            var window = GetWindow<ABSetWindow>();
            window.titleContent = new GUIContent("SetABName");
            window.Show();
        }

        private void OnGUI()
        {
        }
    }
}