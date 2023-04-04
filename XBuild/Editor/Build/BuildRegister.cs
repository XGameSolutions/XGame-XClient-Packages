

using System;
using UnityEditor.SceneManagement;

namespace XBuild
{
    internal static class BuildRegister
    {
        public static Action<BuildParams> onBeforeBuildAB;
        public static Action<BuildParams> onAfterBuildAB;
        public static Action<BuildParams> onBeforeBuildPackage;
        public static Action<BuildParams> onAfterBuildPackage;

        /// <summary>
        /// 打AB前的回调
        /// </summary>
        public static void OnBeforeBuildAB(BuildParams buildParam)
        {
            if (onBeforeBuildAB != null)
            {
                onBeforeBuildAB(buildParam);
            }
        }

        /// <summary>
        /// 打AB后的回调
        /// </summary>
        public static void OnAfterBuildAB(BuildParams buildParam)
        {
            if (onAfterBuildAB != null)
            {
                onAfterBuildAB(buildParam);
            }
        }

        /// <summary>
        /// 打包体前的回调
        /// </summary>
        public static void OnBeforeBuildPackage(BuildParams buildParam)
        {
            if (onBeforeBuildPackage != null)
            {
                onBeforeBuildPackage(buildParam);
            }
            SceneTool.RefreshSettingList(true);
            EditorSceneManager.OpenScene(buildParam.startScene);
        }

        /// <summary>
        /// 打包体后的回调
        /// </summary>
        public static void OnAfterBuildPackage(BuildParams buildParam, bool success)
        {
            SceneTool.RefreshSettingList(false);
            if (onAfterBuildPackage != null)
            {
                onAfterBuildPackage(buildParam);
            }
        }
    }
}