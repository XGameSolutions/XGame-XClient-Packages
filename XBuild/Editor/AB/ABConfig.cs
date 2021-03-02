/******************************************/
/*                                        */
/*     Copyright (c) 2020 monitor1394     */
/*     https://github.com/monitor1394     */
/*                                        */
/******************************************/

using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XBuild.AB
{
    [Serializable]
    public class ABConfig : ScriptableObject
    {
        private static ABConfig s_Instance;
        public const string configName = "ABConfig";
        public const string configPath = "Assets/XPlugins/XBuild/Editor/Resources/ABConfig.asset";
        public string AB_DIR = "../ResAB";
        public string dep = "_dep_";
        public string ab_prefix_shader = "shader";
        public string ab_prefix_model = "model_";
        public string ab_prefix_model_fx = "model_fx";
        public string ab_prefix_model_character = "model_role_";
        public string ab_prefix_model_weapon = "model_weapon_";
        public string ab_prefix_model_monster = "model_monster_";
        public string ab_prefix_model_scene = "model_scene_";
        public string ab_prefix_model_other = "model_other_";
        public string ab_prefix_model_dep = "model_dep_";
        public string ab_prefix_model_dep_common_tex = "model_dep_common_tex_";
        public string ab_prefix_model_dep_material = "model_dep_mat_";

        public string ab_prefix_scene = "scene_";
        public string ab_prefix_scene_dep = "scene_dep_";
        public string ab_prefix_scene_dep_meterial = "scene_dep_mat_";
        public string ab_prefix_scene_dep_prefab = "scene_dep_prefab_";
        public string ab_prefix_scene_dep_meterial_common_tex = "scene_dep_mat_common_tex_";
        public string ab_prefix_scene_dep_common_tex = "scene_dep_common_tex_";
        public string ab_prefix_scene_dep_common_asset = "scene_dep_common_asset_";
        public string ab_prefix_scene_dep_common_prefab = "scene_dep_common_prefab_";

        public string ab_prefix_ui = "ui_";
        public string ab_prefix_ui_big_image = "ui_big_image_";

        public string res_dir_model = "Assets/ResModel";
        public string res_dir_model_fx = "Assets/ResModel/Effects";
        public string res_dir_model_charater = "Assets/ResModel/Character";
        public string res_dir_model_weapon = "Assets/ResModel/Weapon";
        public string res_dir_model_monster = "Assets/ResModel/Monster";
        public string res_dir_model_other = "Assets/ResModel/Other";
        public string res_dir_scene = "Assets/ResScene";
        public string res_dir_ui = "Assets/ResUI";
        public string res_dir_ui_big_image = "Assets/ResUI/big_image";

        public float ab_dep_single_size_limit_material = 0.2f * 1024 * 1024;
        public float ab_dep_single_size_limit_model = 1f * 1024 * 1024;
        public float ab_dep_single_size_limit_scene = 1f * 1024 * 1024;
        public int common_dep_index_max = 5;

        public string[] common_ab_list = new string[] { "shader" };

        public string[] file_suffix_material = new string[] { ".mat" };
        public string[] file_suffix_prefab = new string[] { ".prefab", ".fbx", ".obj" };
        public string[] file_suffix_script = new string[] { ".cs" };
        public string[] file_suffix_shader = new string[] { ".shader" };
        public string[] file_suffix_asset = new string[] { ".asset" };
        public string[] file_suffix_texture = new string[] { ".tga", ".png", ".psd", ".tif",
            ".tiff", ".dds", ".jpeg", ".jpg", ".gif", ".bmp", ".exr", ".hdr"};

        public string[] file_suffix_exclude = new string[] { ".cs" };

        public static ABConfig Instance
        {
            get
            {
                if (s_Instance != null) return s_Instance;
                s_Instance = Resources.Load<ABConfig>(configPath);
                if (s_Instance == null)
                {
                    s_Instance = ScriptableObject.CreateInstance("ABConfig") as ABConfig;
                    CommonUtil.CheckAndCreateDir(Application.dataPath, "XPlugins/XBuild/Editor/Resources/");
                    AssetDatabase.CreateAsset(s_Instance, ABConfig.configPath);
                }
                return s_Instance;
            }
        }

        public static bool IsValidABName(string abName)
        {
            if (string.IsNullOrEmpty(abName)) return true;
            return abName.StartsWith(Instance.ab_prefix_model) ||
            abName.StartsWith(Instance.ab_prefix_scene) ||
            abName.StartsWith(Instance.ab_prefix_ui) ||
            abName.StartsWith(Instance.ab_prefix_shader);
        }

        public static bool IsDepABName(string abName)
        {
            if (string.IsNullOrEmpty(abName)) return false;
            return abName.IndexOf(Instance.dep) >= 0;
        }

        public static AssetsCategory GetDepCategory(string path)
        {
            if (path.StartsWith(Instance.res_dir_model)) return AssetsCategory.Model;
            else if (path.StartsWith(Instance.res_dir_scene)) return AssetsCategory.Scene;
            else if (path.StartsWith(Instance.res_dir_ui)) return AssetsCategory.UI;
            else return AssetsCategory.Other;
        }

        public static AssetsType GetAssetType(string extention)
        {
            if (IsShader(extention)) return AssetsType.Shader;
            else if (IsMaterial(extention)) return AssetsType.Material;
            else if (IsTexture(extention)) return AssetsType.Texture;
            else if (IsPrefab(extention)) return AssetsType.Prefab;
            else if (IsAsset(extention)) return AssetsType.Asset;
            else return AssetsType.Other;
        }

        public static bool IsModel(string path)
        {
            return path.StartsWith(Instance.res_dir_model);
        }

        public static bool IsScene(string path)
        {
            return path.StartsWith(Instance.res_dir_scene);
        }

        public static bool IsUI(string path)
        {
            return path.StartsWith(Instance.res_dir_ui);
        }

        public static bool IsUIBigImage(string path)
        {
            return path.StartsWith(Instance.res_dir_ui_big_image);
        }

        public static bool IsMaterial(string extention)
        {
            foreach (var suffix in Instance.file_suffix_material)
            {
                if (extention.Equals(suffix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
        public static bool IsPrefab(string extention)
        {
            foreach (var suffix in Instance.file_suffix_prefab)
            {
                if (extention.Equals(suffix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
        public static bool IsScript(string extention)
        {
            foreach (var suffix in Instance.file_suffix_script)
            {
                if (extention.Equals(suffix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
        public static bool IsShader(string extention)
        {
            foreach (var suffix in Instance.file_suffix_shader)
            {
                if (extention.Equals(suffix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
        public static bool IsAsset(string extention)
        {
            foreach (var suffix in Instance.file_suffix_asset)
            {
                if (extention.Equals(suffix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }
        public static bool IsTexture(string extention)
        {
            foreach (var suffix in Instance.file_suffix_texture)
            {
                if (extention.Equals(suffix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool IsUseDirNameForABName(string path)
        {
            return path.StartsWith(Instance.res_dir_ui) && !path.StartsWith(Instance.res_dir_ui_big_image);
        }

        public static bool IsValidDep(AssetsCategory category, AssetsType type)
        {
            if (category == AssetsCategory.UI) return false;
            return type == AssetsType.Material || type == AssetsType.Texture || type == AssetsType.Shader
                || type == AssetsType.Prefab || type == AssetsType.Asset;
        }

        public static bool IsExcludeAssets(string path)
        {
            foreach (var suffix in Instance.file_suffix_exclude)
            {
                if (path.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool IsExcludeExtention(string extention)
        {
            foreach (var suffix in Instance.file_suffix_exclude)
            {
                if (extention.Equals(suffix, StringComparison.OrdinalIgnoreCase)) return true;
            }
            return false;
        }

        public static bool IsCommonAB(string abName)
        {
            foreach (var ab in Instance.common_ab_list)
            {
                if (ab.Equals(abName)) return true;
            }
            return false;
        }

        /// <summary>
        /// 根据资源路径获得默认的ABName前缀
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetABNamePrefix(string path)
        {
            if (IsShader(path)) return Instance.ab_prefix_shader;
            else if (path.StartsWith(Instance.res_dir_model_fx)) return Instance.ab_prefix_model_fx;
            else if (path.StartsWith(Instance.res_dir_model_charater)) return Instance.ab_prefix_model_character;
            else if (path.StartsWith(Instance.res_dir_model_weapon)) return Instance.ab_prefix_model_weapon;
            else if (path.StartsWith(Instance.res_dir_model_monster)) return Instance.ab_prefix_model_monster;
            else if (path.StartsWith(Instance.res_dir_model_other)) return Instance.ab_prefix_model_other;
            else if (path.StartsWith(Instance.res_dir_model)) return Instance.ab_prefix_model;
            else if (path.StartsWith(Instance.res_dir_scene)) return Instance.ab_prefix_scene;
            else if (path.StartsWith(Instance.res_dir_ui_big_image)) return Instance.ab_prefix_ui_big_image;
            else if (path.StartsWith(Instance.res_dir_ui)) return Instance.ab_prefix_ui;
            else return "";
        }

        /// <summary>
        /// 根据资源路径获得默认的ABName
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetDefaultABName(string path)
        {
            if (IsShader(path)) return Instance.ab_prefix_shader;
            if (IsUseDirNameForABName(path))
            {
                return GetABNamePrefix(path) + GetResUISubDirName(path).ToLower();
            }
            else
            {
                var fileName = Path.GetFileNameWithoutExtension(path);
                return GetABNamePrefix(path) + fileName.ToLower();
            }
        }

        public static string GetResUISubDirName(string path)
        {
            if (!path.StartsWith(Instance.res_dir_ui)) return null;
            var list = path.Substring(Instance.res_dir_ui.Length + 1).Split('/');
            return list[0];
        }

        internal static int GetCommonDepIndex(string path)
        {
            var sum = 0;
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            var bytes = System.Text.Encoding.UTF8.GetBytes(path);
            var byteHash = md5.ComputeHash(bytes);
            foreach (var by in byteHash) sum += by;
            return sum % Instance.common_dep_index_max;
        }

        internal static string GetDepNeedSetABName(AssetsInfo info)
        {
            if (IsNeedSetSingleABName(info)) return GetDepSingleABName(info);
            else return GetDepCommonABName(info);
        }

        internal static string GetSceneDepMaterialABName(string fileName)
        {
            return Instance.ab_prefix_scene_dep_meterial + fileName.ToLower();
        }
        internal static string GetSceneDepPrefabABName(string fileName)
        {
            return Instance.ab_prefix_scene_dep_prefab + fileName.ToLower();
        }

        internal static string GetSceneDepMaterialABName(string fileName, long fileSize)
        {
            if (fileSize > Instance.ab_dep_single_size_limit_material)
            {
                return GetSceneDepMaterialABName(fileName);
            }
            else
            {
                return Instance.ab_prefix_scene_dep_common_tex;
            }
        }

        internal static string GetSceneDepPrefabABName(string fileName, long fileSize)
        {
            if (fileSize > Instance.ab_dep_single_size_limit_material)
            {
                return GetSceneDepMaterialABName(fileName);
            }
            else
            {
                return Instance.ab_prefix_scene_dep_common_prefab;
            }
        }

        internal static bool IsNeedSetSingleABName(AssetsInfo info)
        {
            return (info.category == AssetsCategory.Model && info.size > Instance.ab_dep_single_size_limit_model)
                || (info.category == AssetsCategory.Scene && info.size > Instance.ab_dep_single_size_limit_scene);
        }

        internal static string GetDepSingleABName(AssetsInfo info)
        {
            switch (info.category)
            {
                case AssetsCategory.Model:
                    if (info.type == AssetsType.Texture || info.type == AssetsType.Prefab
                        || info.type == AssetsType.Asset)
                    {
                        return Instance.ab_prefix_model_dep + info.name.ToLower();
                    }
                    break;
                case AssetsCategory.Scene:
                    if (info.type == AssetsType.Texture || info.type == AssetsType.Prefab
                        || info.type == AssetsType.Asset)
                    {
                        return Instance.ab_prefix_scene_dep + info.name.ToLower();
                    }
                    break;
            }
            return null;
        }

        internal static string GetDepCommonABName(AssetsInfo info)
        {
            switch (info.category)
            {
                case AssetsCategory.Model:
                    if (info.type == AssetsType.Texture)
                    {
                        return Instance.ab_prefix_model_dep_common_tex + GetCommonDepIndex(info.path);
                    }
                    break;
                case AssetsCategory.Scene:
                    if (info.type == AssetsType.Texture)
                    {
                        return Instance.ab_prefix_scene_dep_common_tex + GetCommonDepIndex(info.path);
                    }
                    else if (info.type == AssetsType.Asset)
                    {
                        return Instance.ab_prefix_scene_dep_common_asset + GetCommonDepIndex(info.path);
                    }
                    break;
            }
            return null;
        }

        internal static string GetABNameWithDepSuffix(string abName)
        {
            return abName + Instance.dep;
        }

        internal static string GetABDirPath()
        {
            if (Instance.AB_DIR.StartsWith("/")) Instance.AB_DIR = Instance.AB_DIR.Substring(1);
            if (Instance.AB_DIR.EndsWith("/")) Instance.AB_DIR = Instance.AB_DIR.Substring(0, Instance.AB_DIR.Length - 1);
            return string.Format("{0}/{1}", Application.dataPath, Instance.AB_DIR);
        }
    }
}