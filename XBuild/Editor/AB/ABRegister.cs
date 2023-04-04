

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XBuild.AB
{
    internal static class ABRegister
    {
        public static LoadAssetBundleDelegate loadABDelegate;

        public static AssetBundle LoadAssetBundle(string path)
        {
            if (loadABDelegate != null)
            {
                return loadABDelegate(path);
            }
            else
            {
                return AssetBundle.LoadFromFile(path);
            }
        }
    }
}