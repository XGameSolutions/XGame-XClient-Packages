/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System;
using UnityEditor;
using UnityEngine;
using XBuild;

public static class BuildAPI
{
    [MenuItem("X/XBuild/Build-Window-AB")]
    public static void BuildWindowAB()
    {
        BuildLog.Instance.Init("BuildAPI.BuildWindowAB");
        BuildTool.BuildWindowAB();
    }

    [MenuItem("X/XBuild/Build-Window-EXE")]
    public static void BuildWindowExe()
    {
        BuildLog.Instance.Init("BuildAPI.BuildWindowExe");
        BuildTool.BuildWindowExe();
    }

    [MenuItem("X/XBuild/Build-Window-AB-And-EXE")]
    public static void BuildWindowABAndExe()
    {
        BuildLog.Instance.Init("BuildAPI.BuildWindowABAndExe");
        BuildTool.BuildWindowABAndExe();
    }

    [MenuItem("X/XBuild/Build-OSX-AB")]
    public static void BuildOSXAB()
    {
        BuildLog.Instance.Init("BuildAPI.BuildOSXAB");
        BuildTool.BuildOSXAB();
    }

    [MenuItem("X/XBuild/Build-iOS-AB")]
    public static void BuildIOSAB()
    {
        BuildLog.Instance.Init("BuildAPI.BuildIOSAB");
        BuildTool.BuildIOSAB();
    }

    [MenuItem("X/XBuild/Build-iOS-XCode")]
    public static void BuildIOSXcode()
    {
        BuildLog.Instance.Init("BuildAPI.BuildIOSXcode");
        BuildTool.BuildIOSXcode();
    }

    [MenuItem("X/XBuild/Build-iOS-AB-And-XCode")]
    public static void BuildIOSABAndXcode()
    {
        BuildLog.Instance.Init("BuildAPI.BuildIOSABAndXcode");
        BuildTool.BuildIOSAB();
        BuildTool.BuildIOSXcode();
    }

    [MenuItem("X/XBuild/Build-Android-AB")]
    public static void BuildAndroidAB()
    {
        BuildLog.Instance.Init("BuildAPI.BuildAndroidAB");
        BuildTool.BuildAndroidAB();
    }

    [MenuItem("X/XBuild/Build-Android-APK")]
    public static void BuildAndroidApk()
    {
        BuildLog.Instance.Init("BuildAPI.BuildAndroidApk");
        BuildTool.BuildAndroidApk();
    }

    [MenuItem("X/XBuild/Build-Android-AB-And-APK")]
    public static void BuildAndroidABAndApk()
    {
        BuildLog.Instance.Init("BuildAPI.BuildAndroidABAndApk");
        BuildTool.BuildAndroidABAndApk();
    }

    [MenuItem("X/XBuild/Scene-RefreshList")]
    public static void SceneRefreshSettingList()
    {
        SceneTool.RefreshSettingList(false);
    }

    /// <summary>
    /// 注册Build AB前的回调
    /// </summary>
    public static void RegisterBeforeBuildABCallback(Action<BuildParams> callback)
    {
        BuildRegister.onBeforeBuildAB = callback;
    }
    /// <summary>
    /// 注册Build AB后的回调
    /// </summary>
    /// <param name="callback"></param>
    public static void RegisterAfterBuildABCallback(Action<BuildParams> callback)
    {
        BuildRegister.onAfterBuildAB = callback;
    }
    /// <summary>
    /// 注册打包前的回调
    /// </summary>
    public static void RegisterBeforeBuildPackageCallback(Action<BuildParams> callback)
    {
        BuildRegister.onBeforeBuildPackage = callback;
    }
    /// <summary>
    /// 注册打包后的回调
    /// </summary>
    public static void RegisterAfterBuildPacakgeCallback(Action<BuildParams> callback)
    {
        BuildRegister.onAfterBuildPackage = callback;
    }
}