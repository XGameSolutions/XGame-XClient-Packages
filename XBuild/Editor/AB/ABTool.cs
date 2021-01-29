/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace XBuild.AB
{
    public static class ABTool
    {

        /// <summary>
        /// 自动根据规则设置ABName
        /// </summary>
        [MenuItem("Assets/SetABName_Auto")]
        public static void AutoSetABName()
        {
            if (Selection.objects.Length == 0)
            {
                Debug.LogWarning("need selected a asset.");
                return;
            }
            foreach (var obj in Selection.objects)
            {
                if (EditorUtility.IsPersistent(obj) && !obj.GetType().ToString().Equals("UnityEditor.DefaultAsset"))
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    var abName = ABConfig.GetDefaultABName(path);
                    SetAB(path, abName);
                    Debug.Log(string.Format("AutoSetABName:path={0},abName={1}", path, abName));
                }
            }
        }


        /// <summary>
        /// 重新分析依赖并设置ABName
        /// </summary>
        [MenuItem("XBuild/AB-AutoResetDepABName")]
        public static void ResetAllDepABNameByAnalyzeDepdencies()
        {
            BuildLog.Log("ResetAllDepABNameByAnalyzeDepdencies ...");
            ClearAllDepABName(false);
            var depList = ABHelper.GetDepList(ABHelper.GetAllDependencies());
            foreach (var info in depList)
            {
                ClearDepAB(info.path);
            }
            SetSceneMaterialABName(ref depList);
            SetScenePrefabABName(ref depList);
            SetDepABName(depList);

            SetShaderABName();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            BuildLog.Log("ResetAllDepABNameByAnalyzeDepdencies DONE!");
        }

        /// <summary>
        /// 获得资源以设置的ABName
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetABName(string path)
        {
            var import = AssetImporter.GetAtPath(path);
            return import ? import.assetBundleName : null;
        }

        public static bool SetAB(string path, string abName)
        {
            var import = AssetImporter.GetAtPath(path);
            if (import)
            {
                import.assetBundleName = abName;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 只有资源abName为空时才可以设置依赖abName
        /// </summary>
        /// <param name="path"></param>
        /// <param name="depABName"></param>
        public static bool SetDepAB(string path, string depABName)
        {
            var import = AssetImporter.GetAtPath(path);
            if (import && string.IsNullOrEmpty(import.assetBundleName))
            {
                import.assetBundleName = depABName;
                return true;
            }
            return false;
        }

        public static bool ClearAB(string path)
        {
            var import = AssetImporter.GetAtPath(path);
            if (import)
            {
                import.assetBundleName = null;
                import.assetBundleVariant = null;
                return true;
            }
            return false;
        }

        public static bool ClearDepAB(string path)
        {
            var import = AssetImporter.GetAtPath(path);
            if (import && string.IsNullOrEmpty(import.assetBundleName) && ABConfig.IsDepABName(import.assetBundleName))
            {
                import.assetBundleName = null;
                return true;
            }
            return false;
        }

        public static bool ClearAllDepABName(bool refreshAndSaveAssets)
        {
            var needRefresh = false;
            foreach (var abName in AssetDatabase.GetAllAssetBundleNames())
            {
                if (ABConfig.IsDepABName(abName))
                {
                    foreach (var path in AssetDatabase.GetAssetPathsFromAssetBundle(abName))
                    {
                        needRefresh |= ClearAB(path);
                    }
                    AssetDatabase.RemoveAssetBundleName(abName, true);
                }
            }
            if (needRefresh && refreshAndSaveAssets)
            {
                AssetDatabase.Refresh();
                AssetDatabase.SaveAssets();
            }
            return true;
        }

        /// <summary>
        /// 场景材质球和它关联的贴图一起单独打AB
        /// </summary>
        /// <param name="depList"></param>
        private static void SetSceneMaterialABName(ref List<AssetsInfo> depList)
        {
            var targetList = new List<AssetsInfo>();
            for (int i = depList.Count - 1; i >= 0; i--)
            {
                var info = depList[i];
                if (info.type == AssetsType.Material
                    && info.category != AssetsCategory.Model && info.category != AssetsCategory.UI
                    && string.IsNullOrEmpty(info.abName))
                {
                    targetList.Add(info);
                    SetAB(info.path, ABConfig.GetSceneDepMaterialABName(info.name));
                    depList.Remove(info);
                }
            }
            var list = ABHelper.GetDepList(ABHelper.GetDependencies(targetList));
            foreach (var info in list)
            {
                if (info.type != AssetsType.Shader)
                {
                    var abName = info.GetFirstRefAB();
                    SetDepAB(info.path, ABConfig.GetSceneDepMaterialABName(abName));
                }
            }
        }

        private static void SetScenePrefabABName(ref List<AssetsInfo> depList)
        {
            var targetList = new List<AssetsInfo>();
            for (int i = depList.Count - 1; i >= 0; i--)
            {
                var info = depList[i];
                if (info.category != AssetsCategory.Model && info.category != AssetsCategory.UI
                    && info.refCount > 1
                    && string.IsNullOrEmpty(info.abName)
                    && info.extension.Equals(".prefab"))
                {
                    targetList.Add(info);
                    SetDepAB(info.path, ABConfig.GetSceneDepPrefabABName(info.name));
                    depList.Remove(info);
                }
            }
            var list = ABHelper.GetDepList(ABHelper.GetDependencies(targetList));
            foreach (var info in list)
            {
                if (info.type != AssetsType.Shader)
                {
                    var abName = info.GetFirstRefAB();
                    SetDepAB(info.path, ABConfig.GetSceneDepPrefabABName(abName));
                }
            }
        }

        private static void SetDepABName(List<AssetsInfo> depList)
        {
            foreach (var info in depList)
            {
                SetDepInfoAB(info);
            }
        }

        /// <summary>
        /// Shader所有的依赖都单独设AB
        /// </summary>
        private static void SetShaderABName()
        {
            var shaderABName = ABConfig.Instance.ab_prefix_shader;
            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(shaderABName);
            foreach (var path in AssetDatabase.GetDependencies(assetPaths, true))
            {
                SetAB(path, shaderABName);
            }
        }

        private static void SetDepInfoAB(AssetsInfo info)
        {
            var abConfig = ABConfig.Instance;
            if (info.type == AssetsType.Shader)
            {
                SetAB(info.path, abConfig.ab_prefix_shader);
            }
            else
            {
                if (info.refCount > 1)
                {
                    info.size = ABHelper.GetAssetSize(info.path);
                    var abName = ABConfig.GetDepNeedSetABName(info);
                    if (!string.IsNullOrEmpty(abName))
                    {
                        SetDepAB(info.path, abName);
                    }
                }
                else
                {
                    if (info.category == AssetsCategory.Scene)
                    {
                        if (info.type == AssetsType.Texture || info.type == AssetsType.Asset)
                        {
                            var abName = info.GetFirstRefAB();
                            SetDepAB(info.path, ABConfig.GetABNameWithDepSuffix(abName));
                        }
                    }
                }
            }
        }
    }
}