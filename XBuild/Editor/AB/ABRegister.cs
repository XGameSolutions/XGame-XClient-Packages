/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XBuild.AB
{
    public delegate AssetBundle LoadAssetBundleDelegate(string path);

    public static class ABRegister
    {
        private static LoadAssetBundleDelegate s_LoadABDelegate;

        public static void RegisterLoadAssetBundle(LoadAssetBundleDelegate func)
        {
            s_LoadABDelegate = func;
        }

        internal static AssetBundle LoadAssetBundle(string path)
        {
            if (s_LoadABDelegate != null)
            {
                return s_LoadABDelegate(path);
            }
            else
            {
                return AssetBundle.LoadFromFile(path);
            }
        }
    }
}