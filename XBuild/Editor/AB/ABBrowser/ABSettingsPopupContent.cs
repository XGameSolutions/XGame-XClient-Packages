
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace XBuild.AB.ABBrowser
{
    internal class ABSettingsPopupContent : PopupWindowContent
    {
        class Styles
        {
            public static readonly GUIContent abDir = new GUIContent("AB Dir", "The directory where assetbundle is in");
            public static readonly GUIContent abSource = new GUIContent("AB Source", "AB read from Directory or AssetDatabase");
        }
        bool changed = false;

        public override void OnGUI(Rect rect)
        {
            EditorGUI.BeginChangeCheck();
            ABConfig.Instance.AB_DIR = EditorGUILayout.TextField(Styles.abDir, ABConfig.Instance.AB_DIR);
            if (!Directory.Exists(ABConfig.GetABDirPath()))
            {
                EditorGUILayout.HelpBox("Directory not exists:" + ABConfig.GetABDirPath(), MessageType.Warning);
            }
            ABBrowserWindow.Instance.abSource = (ABDatabase.ABSource)EditorGUILayout.EnumPopup(Styles.abSource,
                ABBrowserWindow.Instance.abSource);
            if (EditorGUI.EndChangeCheck())
            {
                changed = true;
            }
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector3(450, 100);
        }

        public override void OnOpen()
        {
            changed = false;
        }
        public override void OnClose()
        {

            if (changed)
                ABBrowserWindow.Instance.RefreshAB();
        }
    }
}