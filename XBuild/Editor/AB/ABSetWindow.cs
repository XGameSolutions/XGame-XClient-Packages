/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using UnityEngine;
using UnityEditor;

namespace XBuild.AB
{
    public class ABSetWindow : EditorWindow
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