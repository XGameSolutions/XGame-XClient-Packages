
/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using UnityEngine;

namespace XBuild.AB
{
    public delegate AssetBundle LoadAssetBundleDelegate(string path);

    public static class ABAPI
    {
        public static void RegisterLoadAssetBundle(LoadAssetBundleDelegate func)
        {
            ABRegister.loadABDelegate = func;
        }
    }
}