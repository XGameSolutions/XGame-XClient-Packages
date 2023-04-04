

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace XBuild.AB.ABBrowser
{
    internal static class ABDatabase
    {
        public enum ABSource
        {
            Dir,
            AssetDatabase
        }
        class WaitingAssetsInfo
        {
            public string path;
            public List<string> parents = new List<string>();
            public WaitingAssetsInfo(string path)
            {
                this.path = path;
            }
        }
        class StatInfo
        {
            public int count;
            public long size;

            public void Stat(int count, long size)
            {
                this.count += count;
                this.size += size;
            }

            public string StrInfo()
            {
                return string.Format("{0}个, {1}", count, EditorUtility.FormatBytes(size));
            }

            public string StrInfo(int totalCount)
            {
                return string.Format("{0}/{1}个, {2}", count, totalCount, EditorUtility.FormatBytes(size));
            }
        }
        private static AssetBundleManifest s_Manifest;
        private static AssetBundle s_ManifestAB;

        private static Dictionary<string, ABInfo> s_ABInfoDic = new Dictionary<string, ABInfo>();
        private static Dictionary<ABType, StatInfo> s_ABTypeStat = new Dictionary<ABType, StatInfo>();
        private static Dictionary<string, ABAssetsInfo> s_AssetInfoDic = new Dictionary<string, ABAssetsInfo>();
        private static Dictionary<AssetsType, StatInfo> s_AssetTypeStat = new Dictionary<AssetsType, StatInfo>();

        private static Dictionary<string, WaitingAssetsInfo> s_AssetWaitingDic = new Dictionary<string, WaitingAssetsInfo>();
        private static BuildTarget s_TargetPlatform;
        private static string s_ABDirPath;
        private static bool s_ABDirty;
        private static bool s_AssetDirty;
        private static int s_TotalABs;
        private static int s_TotalAssets;
        private static List<string> s_WaitingInitABs = new List<string>();
        private static List<WaitingAssetsInfo> s_WaitingInitAssets = new List<WaitingAssetsInfo>();

        private static List<ABInfo> s_CurrentABList = new List<ABInfo>();
        private static List<ABAssetsInfo> s_CurrentAssetsList = new List<ABAssetsInfo>();

        public static string abSearchString;
        public static string assetsSearchString;
        public static ABSource abSource = ABSource.Dir;

        public static bool IsABDirty()
        {
            if (s_ABDirty)
            {
                s_ABDirty = false;
                return true;
            }
            return false;
        }

        public static bool IsAssetDirty()
        {
            if (s_AssetDirty)
            {
                s_AssetDirty = false;
                return true;
            }
            return false;
        }

        public static void Update()
        {
            if (s_WaitingInitABs.Count > 0)
            {
                InitABInfo(s_WaitingInitABs[s_WaitingInitABs.Count - 1]);
                s_WaitingInitABs.RemoveAt(s_WaitingInitABs.Count - 1);
            }
            if (s_WaitingInitAssets.Count > 0)
            {
                InitAsset(s_WaitingInitAssets[s_WaitingInitAssets.Count - 1]);
                s_WaitingInitAssets.RemoveAt(s_WaitingInitAssets.Count - 1);
            }
        }

        public static string RefreshAB(BuildTarget target)
        {
            var strPlatform = target.ToString();
            s_TargetPlatform = target;
            s_ABDirPath = string.Format("{0}/{1}/{2}/", ABConfig.GetABDirPath(), strPlatform, strPlatform);
            if (!Directory.Exists(s_ABDirPath))
            {
                var error = "can't find dir:" + s_ABDirPath;
                Debug.LogError(error);
                return error;
            }
            s_TotalABs = 0;
            s_ABInfoDic.Clear();
            s_AssetInfoDic.Clear();
            s_ABTypeStat.Clear();
            s_WaitingInitABs.Clear();
            s_ABDirty = true;

            if (s_ManifestAB != null)
            {
                s_ManifestAB.Unload(true);
                s_ManifestAB = null;
            }
            s_ManifestAB = ABRegister.LoadAssetBundle(s_ABDirPath + strPlatform);
            if (s_ManifestAB != null)
            {
                s_Manifest = s_ManifestAB.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
                s_WaitingInitABs.AddRange(s_Manifest.GetAllAssetBundles());
                s_TotalABs = s_WaitingInitABs.Count;
            }
            return null;
        }

        public static void RefreshAssets(List<ABInfo> abList)
        {
            s_TotalAssets = 0;
            s_AssetTypeStat.Clear();
            s_AssetWaitingDic.Clear();
            s_WaitingInitAssets.Clear();
            s_CurrentAssetsList.Clear();
            foreach (var info in abList)
            {
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(info.name);
                foreach (var path in AssetDatabase.GetDependencies(assetPaths, true))
                {
                    if (ABConfig.IsExcludeAssets(path)) continue;
                    if (!s_AssetWaitingDic.ContainsKey(path))
                    {
                        s_AssetWaitingDic[path] = new WaitingAssetsInfo(path);
                        s_WaitingInitAssets.Add(s_AssetWaitingDic[path]);
                    }
                    s_AssetWaitingDic[path].parents.Add(info.name);
                }
            }
            s_TotalAssets = s_AssetWaitingDic.Count;
            s_AssetDirty = true;
        }


        static List<ABInfo> s_TempABList = new List<ABInfo>();
        public static List<ABInfo> GetABInfoList(ABType type)
        {
            if (string.IsNullOrEmpty(abSearchString))
            {
                if (type == ABType.All) return s_ABInfoDic.Values.ToList();
                s_TempABList.Clear();
                foreach (var kv in s_ABInfoDic)
                {
                    if (kv.Value.type == type) s_TempABList.Add(kv.Value);
                }
            }
            else
            {
                s_TempABList.Clear();
                foreach (var kv in s_ABInfoDic)
                {
                    if ((kv.Value.type == type || type == ABType.All) && kv.Value.name.Contains(abSearchString))
                    {
                        s_TempABList.Add(kv.Value);
                    }
                }
            }
            return s_TempABList;
        }

        static List<ABInfo> s_TempABDepList = new List<ABInfo>();
        public static List<ABInfo> GetABDepInfoList(List<ABInfo> abList)
        {
            s_TempABDepList.Clear();
            if (abList != null)
            {
                foreach (var info in abList)
                {
                    var list = s_Manifest.GetAllDependencies(info.name);
                    if (list == null || list.Length == 0) continue;
                    foreach (var ab in list)
                    {
                        if (s_ABInfoDic.ContainsKey(ab) && !s_TempABDepList.Contains(s_ABInfoDic[ab]))
                        {
                            s_TempABDepList.Add(s_ABInfoDic[ab]);
                        }
                    }
                }
            }
            return s_TempABDepList;
        }
        public static List<ABInfo> GetABRefInfoList(List<ABInfo> abList)
        {
            s_TempABDepList.Clear();
            if (abList != null)
            {
                foreach (var info in abList)
                {
                    foreach (var ab in info.RefABList)
                    {
                        if (s_ABInfoDic.ContainsKey(ab) && !s_TempABDepList.Contains(s_ABInfoDic[ab]))
                        {
                            s_TempABDepList.Add(s_ABInfoDic[ab]);
                        }
                    }
                }
            }
            return s_TempABDepList;
        }

        static List<ABAssetsInfo> s_TempAssetList = new List<ABAssetsInfo>();
        public static List<ABAssetsInfo> GetAssetInfoList(AssetsType type)
        {
            if (string.IsNullOrEmpty(assetsSearchString))
            {
                if (type == AssetsType.None) return s_CurrentAssetsList;
                s_TempAssetList.Clear();
                foreach (var info in s_CurrentAssetsList)
                {
                    if (info.type == type) s_TempAssetList.Add(info);
                }
            }
            else
            {
                s_TempAssetList.Clear();
                foreach (var info in s_CurrentAssetsList)
                {
                    if ((info.type == type || type == AssetsType.None) && info.name.Contains(assetsSearchString))
                    {
                        s_TempAssetList.Add(info);
                    }
                }
            }
            return s_TempAssetList;
        }

        public static string GetABTypeSizeStr(ABType type)
        {
            if (s_ABTypeStat.ContainsKey(type))
            {
                if (type == ABType.All) return s_ABTypeStat[type].StrInfo(s_TotalABs);
                else return s_ABTypeStat[type].StrInfo();
            }
            else return "";
        }

        public static string GetAssetTypeSizeStr(AssetsType type)
        {
            if (s_AssetTypeStat.ContainsKey(type))
            {
                if (type == AssetsType.None) return s_AssetTypeStat[type].StrInfo(s_TotalAssets);
                else return s_AssetTypeStat[type].StrInfo();
            }
            else return "--";
        }

        private static void InitABInfo(string abName)
        {
            s_ABDirty = true;
            var info = GetOrInitABInfo(abName);
            ABInfoDepCountAndSize(info, abName);
        }

        private static ABInfo GetOrInitABInfo(string abName)
        {
            if (s_ABInfoDic.ContainsKey(abName)) return s_ABInfoDic[abName];
            else
            {
                var isDep = ABConfig.IsDepABName(abName);
                var size = GetABFileSize(abName);
                var type = GetABType(abName, isDep);
                var info = new ABInfo()
                {
                    type = type,
                    size = size,
                    name = abName,
                    assetIcon = EditorGUIUtility.IconContent("DefaultAsset Icon").image as Texture2D
                };
                s_ABInfoDic[abName] = info;
                if (!s_ABTypeStat.ContainsKey(ABType.All)) s_ABTypeStat[ABType.All] = new StatInfo();
                if (!s_ABTypeStat.ContainsKey(info.type)) s_ABTypeStat[info.type] = new StatInfo();
                s_ABTypeStat[ABType.All].Stat(1, info.size);
                s_ABTypeStat[info.type].Stat(1, info.size);
                return info;
            }
        }

        private static void InitAsset(WaitingAssetsInfo waitInfo)
        {
            var path = waitInfo.path;
            var extension = Path.GetExtension(path).ToLower();
            if (ABConfig.IsExcludeExtention(extension)) return;
            ABAssetsInfo info = null;
            if (!s_AssetInfoDic.ContainsKey(path))
            {
                var assetType = ABConfig.GetAssetType(extension);
                var width = 0;
                var height = 0;
                if (assetType == AssetsType.Texture)
                {
                    ABHelper.GetTextureWidthAndHeight(path, out width, out height);
                }
                info = new ABAssetsInfo()
                {
                    type = assetType,
                    name = Path.GetFileNameWithoutExtension(path),
                    extension = extension,
                    path = path,
                    abName = ABHelper.GetABName(path),
                    textureWidth = width,
                    textureHeight = height
                };
                s_AssetInfoDic[path] = info;
            }
            else
            {
                info = s_AssetInfoDic[path];
            }
            info.size = ABHelper.GetAssetSize(path);
            if (info.type == AssetsType.Texture)
            {
                var maxSize = 0;
                ABHelper.GetTextureMaxSize(s_TargetPlatform, info.path, out maxSize);
                info.textureMaxSize = maxSize;
            }
            foreach (var ab in waitInfo.parents)
            {
                info.AddRefAB(ab);
            }
            if (!s_AssetTypeStat.ContainsKey(AssetsType.None))
                s_AssetTypeStat[AssetsType.None] = new StatInfo();
            if (!s_AssetTypeStat.ContainsKey(info.type))
                s_AssetTypeStat[info.type] = new StatInfo();
            s_AssetTypeStat[AssetsType.None].Stat(1, info.size);
            s_AssetTypeStat[info.type].Stat(1, info.size);
            s_CurrentAssetsList.Add(info);
            s_AssetDirty = true;
        }

        private static ABType GetABType(string abName, bool isDep = false)
        {
            if (isDep) return ABType.Dep;
            else if (abName.StartsWith(ABConfig.Instance.ab_prefix_model)) return ABType.Model;
            else if (abName.StartsWith(ABConfig.Instance.ab_prefix_scene)) return ABType.Scene;
            else if (abName.StartsWith(ABConfig.Instance.ab_prefix_ui)) return ABType.UI;
            else return ABType.Other;
        }

        private static long GetABFileSize(string abName)
        {
            return ABHelper.GetFileSize(s_ABDirPath + abName);
        }

        private static void ABInfoDepCountAndSize(ABInfo info, string abName)
        {
            var allDeps = s_Manifest.GetAllDependencies(abName);
            if (allDeps == null || allDeps.Length == 0) return;
            var direDeps = s_Manifest.GetDirectDependencies(abName);
            foreach (var ab in allDeps)
            {
                if (info.AddDepAB(ab))
                {
                    if (!ABConfig.IsCommonAB(ab))
                    {
                        info.depSize += GetABFileSize(ab);
                    }
                    var depInfo = GetOrInitABInfo(ab);
                    if(direDeps.Contains(ab))
                        depInfo.AddRefAB(info.name);
                    ABInfoDepCountAndSize(info, ab);
                }
            }
        }
    }
}