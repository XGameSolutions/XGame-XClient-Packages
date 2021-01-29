/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using UnityEditor;
using UnityEngine;
using XBuild;

public static class BuildAPI
{
    [MenuItem("XBuild/Build-Window-AB")]
    public static void BuildWindowAB()
    {
        BuildLog.Instance.Init("BuildAPI.BuildWindowAB");
        BuildTool.BuildWindowAB();
    }

    [MenuItem("XBuild/Build-Window-EXE")]
    public static void BuildWindowExe()
    {
        BuildLog.Instance.Init("BuildAPI.BuildWindowExe");
        BuildTool.BuildWindowExe();
    }

    [MenuItem("XBuild/Build-Window-AB-And-EXE")]
    public static void BuildWindowABAndExe()
    {
        BuildLog.Instance.Init("BuildAPI.BuildWindowABAndExe");
        BuildTool.BuildWindowABAndExe();
    }

    [MenuItem("XBuild/Build-OSX-AB")]
    public static void BuildOSXAB()
    {
        BuildLog.Instance.Init("BuildAPI.BuildOSXAB");
        BuildTool.BuildOSXAB();
    }

    [MenuItem("XBuild/Build-iOS-AB")]
    public static void BuildIOSAB()
    {
        BuildLog.Instance.Init("BuildAPI.BuildIOSAB");
        BuildTool.BuildIOSAB();
    }

    [MenuItem("XBuild/Build-iOS-XCode")]
    public static void BuildIOSXcode()
    {
        BuildLog.Instance.Init("BuildAPI.BuildIOSXcode");
        BuildTool.BuildIOSXcode();
    }

    [MenuItem("XBuild/Build-iOS-AB-And-XCode")]
    public static void BuildIOSABAndXcode()
    {
        BuildLog.Instance.Init("BuildAPI.BuildIOSABAndXcode");
        BuildTool.BuildIOSAB();
        BuildTool.BuildIOSXcode();
    }

    [MenuItem("XBuild/Build-Android-AB")]
    public static void BuildAndroidAB()
    {
        BuildLog.Instance.Init("BuildAPI.BuildAndroidAB");
        BuildTool.BuildAndroidAB();
    }

    [MenuItem("XBuild/Build-Android-APK")]
    public static void BuildAndroidApk()
    {
        BuildLog.Instance.Init("BuildAPI.BuildAndroidApk");
        BuildTool.BuildAndroidApk();
    }

    [MenuItem("XBuild/Build-Android-AB-And-APK")]
    public static void BuildAndroidABAndApk()
    {
        BuildLog.Instance.Init("BuildAPI.BuildAndroidABAndApk");
        BuildTool.BuildAndroidABAndApk();
    }

    [MenuItem("XBuild/Scene-RefreshList")]
    public static void SceneRefreshSettingList()
    {
        SceneTool.RefreshSettingList(false);
    }
}