using UnityEngine;
using UnityEditor;

namespace XRemoteDebug
{
    internal class RemoteDebugStyles
    {
        public static readonly GUIStyle btnInvisible = "InvisibleButton";
        public static readonly Texture2D iconFolder = EditorGUIUtility.IconContent("d_Collab.FolderAdded").image as Texture2D;
        public static readonly Texture2D iconFile = EditorGUIUtility.IconContent("d_Collab.FileUpdated").image as Texture2D;
    }
}