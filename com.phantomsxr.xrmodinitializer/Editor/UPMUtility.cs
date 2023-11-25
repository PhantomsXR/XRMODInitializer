// // /*===============================================================================
// // Copyright (C) 2023 PhantomsXR Ltd. All Rights Reserved.
// //
// // This file is part of the XRMODInitializer.Editor.
// //
// // The PicoPlatform cannot be copied, distributed, or made available to
// // third-parties for commercial purposes without written permission of PhantomsXR Ltd.
// //
// // Contact info@phantomsxr.com for licensing requests.
// // ===============================================================================*/

using System.IO;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace XRMODInitializer.Editor
{
    public class UPMUtility
    {
        private static string _SOURCE_NAME = "PhantomsXR";

        public static void AddSource()
        {
            // 路径到项目的manifest.json文件
            string tmp_ManifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

            if (!File.Exists(tmp_ManifestPath))
            {
                UnityEngine.Debug.LogError("Manifest file not found: " + tmp_ManifestPath);
                return;
            }


            string tmp_ManifestContent = File.ReadAllText(tmp_ManifestPath);
            var tmp_ManifestJson = JObject.Parse(tmp_ManifestContent);
            string tmp_NewSourceUrl = "https://registry.npmjs.org";
            var tmp_Sources = tmp_ManifestJson["scopedRegistries"] as JArray ?? new JArray();

            bool sourceExists = false;
            foreach (var source in tmp_Sources)
            {
                if (source["url"] != null && source["url"].ToString() == tmp_NewSourceUrl)
                {
                    sourceExists = true;
                    break;
                }
                else if (source["name"] != null && source["name"].ToString() == _SOURCE_NAME)
                {
                    sourceExists = true;
                    break;
                }
            }

            if (sourceExists) return;

            var tmp_NewSource = new JObject
            {
                ["name"] = "PhantomsXR",
                ["url"] = tmp_NewSourceUrl,
                ["scopes"] = new JArray("com.phantomsxr")
            };

            // 将新的检索源添加到列表中
            tmp_Sources.Add(tmp_NewSource);

            // 更新manifest.json
            tmp_ManifestJson["scopedRegistries"] = tmp_Sources;
            File.WriteAllText(tmp_ManifestPath, tmp_ManifestJson.ToString());
            UPMUtility.UpdateManifest();
        }

        public static void AddDependency(string _packageName, string _version)
        {
            // 路径到项目的manifest.json文件
            string tmp_MmanifestPath = Path.Combine(Application.dataPath, "..", "Packages", "manifest.json");

            if (!File.Exists(tmp_MmanifestPath))
            {
                UnityEngine.Debug.LogError("Manifest file not found: " + tmp_MmanifestPath);
                return;
            }

            string tmp_ManifestContent = File.ReadAllText(tmp_MmanifestPath);
            var tmp_ManifestJson = JObject.Parse(tmp_ManifestContent);

            // 获取已有的依赖
            var tmp_Dependencies = tmp_ManifestJson["dependencies"] as JObject ?? new JObject();

            // 添加新的依赖
            tmp_Dependencies[_packageName] = _version;

            // 更新manifest.json
            tmp_ManifestJson["dependencies"] = tmp_Dependencies;
            File.WriteAllText(tmp_MmanifestPath, tmp_ManifestJson.ToString());
        }

        public static void AddNewPackage(string _packageName, string _version)
        {
            UnityEditor.PackageManager.Client.Add($"{_packageName}@{_version}");
        }


        public static void UpdateManifest()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate); // 强制更新
            UnityEditor.PackageManager.Client.Resolve(); // 解析新的package依赖
        }
    }
}