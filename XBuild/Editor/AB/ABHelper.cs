/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/


using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace XBuild.AB
{
    public static class ABHelper
    {
        public static Dictionary<string, List<string>> GetAllDependencies()
        {
            var dic = new Dictionary<string, List<string>>();
            foreach (var abName in AssetDatabase.GetAllAssetBundleNames())
            {
                var list = new List<string>();
                dic.Add(abName, list);
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(abName);
                list.AddRange(AssetDatabase.GetDependencies(assetPaths, true));
            }
            return dic;
        }

        public static Dictionary<string, List<string>> GetDependencies(List<AssetsInfo> depList)
        {
            var dic = new Dictionary<string, List<string>>();
            foreach (var info in depList)
            {
                if (!dic.ContainsKey(info.name))
                {
                    dic[info.name] = new List<string>();
                }
                dic[info.name].AddRange(AssetDatabase.GetDependencies(info.path, true));
            }
            return dic;
        }

        public static List<AssetsInfo> GetDepList(Dictionary<string, List<string>> depDic)
        {
            var infoDic = new Dictionary<string, AssetsInfo>();
            foreach (var kv in depDic)
            {
                var abName = kv.Key;
                foreach (var depPath in kv.Value)
                {
                    var category = ABConfig.GetDepCategory(depPath);
                    var type = ABConfig.GetAssetType(depPath);
                    if (!ABConfig.IsValidDep(category, type)) continue;
                    if (!infoDic.ContainsKey(depPath))
                    {
                        infoDic[depPath] = new AssetsInfo()
                        {
                            path = depPath,
                            category = category,
                            type = type,
                            name = Path.GetFileNameWithoutExtension(depPath),
                            extension = Path.GetExtension(depPath).ToLower(),
                            abName = GetABName(depPath),
                            size = 0,
                        };
                    }
                    infoDic[depPath].AddRefAB(abName);
                }
            }
            return infoDic.Values.ToList();
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


        public static long GetAssetSize(string assetPath)
        {
            var target = AssetDatabase.LoadAssetAtPath<Texture>(assetPath);
            if (target != null)
            {
                return GetTextureStorageMemorySize(target);
            }
            else
            {
                var filePath = Application.dataPath + assetPath.Substring(6);
                return GetFileSize(filePath);
            }
        }

        public static long GetFileSize(string filePath)
        {
            var file = new FileInfo(filePath);
            return file.Exists ? file.Length : 0;
        }

        private static MethodInfo s_GetTextureStorageMemorySize;
        public static long GetTextureStorageMemorySize(Texture tex)
        {
            if (s_GetTextureStorageMemorySize == null)
            {
                var type = typeof(Editor).Assembly.GetType("UnityEditor.TextureUtil");
                s_GetTextureStorageMemorySize = type.GetMethod("GetStorageMemorySize",
                    BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public);
            }
            return (long)s_GetTextureStorageMemorySize.Invoke(null, new object[] { tex });
        }

        private static MethodInfo s_GetWidthAndHeight;
        public static bool GetTextureWidthAndHeight(string assetPath, out int width, out int height)
        {
            if (s_GetWidthAndHeight == null)
            {
                s_GetWidthAndHeight = typeof(TextureImporter).GetMethod("GetWidthAndHeight",
                BindingFlags.Static | BindingFlags.Public);
            }
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                var args = new object[2] { 0, 0 };
                s_GetWidthAndHeight.Invoke(importer, args);
                width = (int)args[0];
                height = (int)args[1];
                return true;
            }
            else
            {
                width = 0;
                height = 0;
                return false;
            }
        }
    }
}