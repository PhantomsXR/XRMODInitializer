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

using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace XRMODInitializer.Editor
{
    public class XRMODInitializerEditorWindow : EditorWindow
    {
        internal const string _CONST_ASSET_PATH_ROOT = "Packages/com.phantomsxr.xrmodinitializer/Editor/Assets";

        private enum RuntimePlatformName
        {
            VisionOS,
            HandheldAR,
            Hololens,
            Pico,
            Quest,
            Rokid,
            XReal
        }

        public enum HandheldAROS
        {
            iOS,
            Android
        }

        private class PlatformDetail
        {
            public string PlatformName;
            public Sprite PlatformIcon;
        }

        private Dependencies dependenciesObj;

        private List<VisualElement> platformContainer = new List<VisualElement>();
        private static RuntimePlatformName currentRuntimePlatform;
        private static XRMODInitializerEditorWindow _WINDOW;
        private static InitializerDb InitializerDb;

        private const string SELECTED_CLASS_NAME = "platform-container-selected";
        internal static readonly string _CONST_XRMOD_INITIALIZED = $"XRMOD-Initialized";
        internal static readonly string _CONST_DEVICE_SDK_TYPE = $"DeviceSDKType";
        internal static readonly string _CONST_ALLOW_APPLY_CONFIGURE = $"AllowApplyConfigure";

        internal const string CONST_DOCS_URL =
            "https://docs.phantomsxr.com/experience-manual/prepare-for-developer/install-platform-sdk";

        [MenuItem("Tools/XR-MOD/Install XRMOD", false, priority: int.MaxValue)]
        internal static void ReInstallXRMOD()
        {
            GetWindow();
        }

        static void GetWindow()
        {
            _WINDOW = EditorWindow.GetWindow<XRMODInitializerEditorWindow>();
            _WINDOW.minSize = new Vector2(640, 480);
            _WINDOW.maxSize = new Vector2(640, 480);
            _WINDOW.titleContent = new GUIContent("XRMOD Installer");
            _WINDOW.Show();
        }

        void OnEnable()
        {
            ActivateCorrespondingVersionDll.ActivateDll();
            string projectSettingsPath = "ProjectSettings/ProjectSettings.asset";
            SerializedObject projectSettingsObject =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath(projectSettingsPath)[0]);
            SerializedProperty editorSettingsProperty = projectSettingsObject.FindProperty("locationUsageDescription");
            if (editorSettingsProperty != null)
            {
                editorSettingsProperty.stringValue = "Your location is required for feature Nearby map.";
                projectSettingsObject.Update();
                projectSettingsObject.ApplyModifiedProperties();
            }

            InitializerDb =
                AssetDatabase.LoadAssetAtPath<InitializerDb>($"{_CONST_ASSET_PATH_ROOT}/{nameof(InitializerDb)}.asset");
            var tmp_XRMODIntializerContainerVTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    $"{_CONST_ASSET_PATH_ROOT}/XRMODInitializerContainer.uxml");
            var tmp_PlatformContainerVTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    $"{_CONST_ASSET_PATH_ROOT}/XRMODPlatformComponent.uxml");

            if (tmp_XRMODIntializerContainerVTree == null || tmp_PlatformContainerVTree == null) return;

            VisualElement tmp_PlatformContainerUXMLContent = tmp_XRMODIntializerContainerVTree.CloneTree();
            rootVisualElement.Add(tmp_PlatformContainerUXMLContent);

            var tmp_PlatformListViewDataSource = new List<PlatformDetail>();
            var tmp_AllPlatformNamesStr = Enum.GetNames(typeof(RuntimePlatformName));
            foreach (string tmp_PlatformName in tmp_AllPlatformNamesStr)
            {
                tmp_PlatformListViewDataSource.Add(new PlatformDetail()
                {
                    PlatformName = tmp_PlatformName,
                    PlatformIcon =
                        AssetDatabase.LoadAssetAtPath<Sprite>(
                            $"{_CONST_ASSET_PATH_ROOT}/Sprites/{tmp_PlatformName}.png")
                });
            }


            var tmp_PlatformContainerList = rootVisualElement.Q<VisualElement>("platform_container_list");
            foreach (PlatformDetail tmp_PlatformDetail in tmp_PlatformListViewDataSource)
            {
                VisualElement tmp_PlatformUXMLContent = tmp_PlatformContainerVTree.CloneTree();
                var tmp_PlatformContainer = tmp_PlatformUXMLContent.Q<VisualElement>("platform_container");

                tmp_PlatformUXMLContent.Q<VisualElement>("platform_icon").style.backgroundImage =
                    new StyleBackground(tmp_PlatformDetail.PlatformIcon);
                tmp_PlatformUXMLContent.Q<Label>("platform_name").text = tmp_PlatformDetail.PlatformName;
                tmp_PlatformContainerList.Add(tmp_PlatformUXMLContent);
                tmp_PlatformContainer.tooltip = tmp_PlatformDetail.PlatformName;
                tmp_PlatformContainer.RegisterCallback(
                    new EventCallback<ClickEvent>(_evt => SelectPlatformElement(tmp_PlatformUXMLContent)));
                platformContainer.Add(tmp_PlatformUXMLContent);
            }

            rootVisualElement.Q<Button>("process_button").SetEnabled(false);

            RegisterButtonEvent();

            // Auto selected
            SelectPlatformElement(platformContainer[1]);
        }

        void RegisterButtonEvent()
        {
            var tmp_ProcessButton = rootVisualElement.Q<Button>("process_button");
            tmp_ProcessButton.clicked += ProcessSwitchPlatform;

            rootVisualElement.Q<Button>("read_more_button").clicked += () => { Help.BrowseURL(CONST_DOCS_URL); };
        }

        void SelectPlatformElement(VisualElement _selected)
        {
            foreach (var tmp_Element in platformContainer)
            {
                tmp_Element[0].RemoveFromClassList(SELECTED_CLASS_NAME);
            }

            _selected[0].AddToClassList(SELECTED_CLASS_NAME);

            var tmp_PlatformName = _selected.Q<Label>("platform_name").text;
            RuntimePlatformName.TryParse(tmp_PlatformName, out currentRuntimePlatform);

            var tmp_OSTypeEnumField = rootVisualElement.Q<EnumField>("os_type");
            tmp_OSTypeEnumField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            var tmp_WarringGroup = rootVisualElement.Q<VisualElement>("warring_group");
            tmp_WarringGroup.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            switch (currentRuntimePlatform)
            {
                case RuntimePlatformName.VisionOS:
                    tmp_OSTypeEnumField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    rootVisualElement.Q<Button>("process_button").SetEnabled(false);
                    break;
                case RuntimePlatformName.HandheldAR:
                    tmp_OSTypeEnumField.Init(HandheldAROS.iOS, true);
                    tmp_OSTypeEnumField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    break;
                case RuntimePlatformName.Hololens:
                    tmp_WarringGroup.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    tmp_OSTypeEnumField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    tmp_WarringGroup.Q<VisualElement>("warring_icon").style.backgroundImage =
                        EditorGUIUtility.FindTexture("console.warnicon");
                    break;
                case RuntimePlatformName.Pico:
                case RuntimePlatformName.Quest:
                case RuntimePlatformName.Rokid:
                case RuntimePlatformName.XReal:
                    tmp_WarringGroup.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                    tmp_WarringGroup.Q<VisualElement>("warring_icon").style.backgroundImage =
                        EditorGUIUtility.FindTexture("console.warnicon");
                    tmp_OSTypeEnumField.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                    tmp_OSTypeEnumField.Init(HandheldAROS.Android, true);

                    if (currentRuntimePlatform == RuntimePlatformName.XReal ||
                        currentRuntimePlatform == RuntimePlatformName.Quest)
                        rootVisualElement.Q<Button>("process_button").SetEnabled(false);

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            LoadDependenciesText();
        }

        void LoadDependenciesText()
        {
            InitializerDb.Dependencies.Clear();

            var tmp_DependenciesContainer = rootVisualElement.Q<ScrollView>("dependecies_container");
            tmp_DependenciesContainer.Clear();

            var tmp_DependenciesAsset =
                AssetDatabase.LoadAssetAtPath<TextAsset>(
                    $"{_CONST_ASSET_PATH_ROOT}/Dependencies/{currentRuntimePlatform}Dependencies.json");

            var tmp_XRMODDependencyComponent =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    $"{_CONST_ASSET_PATH_ROOT}/XRMODDependencyComponent.uxml");

            dependenciesObj = JsonUtility.FromJson<Dependencies>(tmp_DependenciesAsset.text);

            var tmp_DependenciesTitle = new Label("Dependencies");
            tmp_DependenciesTitle.text += $" ({dependenciesObj.dependencies.Count})";
            tmp_DependenciesTitle.AddToClassList("dependencies-title");
            tmp_DependenciesContainer.Add(tmp_DependenciesTitle);
            foreach (Dependency tmp_Dependency in dependenciesObj.dependencies)
            {
                var tmp_DependencyLabel = tmp_XRMODDependencyComponent.CloneTree();
                tmp_DependencyLabel.Q<Label>("package_name").text = tmp_Dependency.PackageName;
                tmp_DependencyLabel.Q<Label>("package_version").text = tmp_Dependency.PackageVersion;
                tmp_DependenciesContainer.Add(tmp_DependencyLabel);

                InitializerDb.Dependencies.Add($"{tmp_Dependency.PackageName}@{tmp_Dependency.PackageVersion}");
            }

            var tmp_ProcessButton = rootVisualElement.Q<Button>("process_button");
            tmp_ProcessButton.SetEnabled(true);
        }

        async void ProcessSwitchPlatform()
        {
            string tmp_PluginName = string.Empty;
            switch (currentRuntimePlatform)
            {
                case RuntimePlatformName.Hololens:
                case RuntimePlatformName.Rokid:
                case RuntimePlatformName.XReal:
                case RuntimePlatformName.Quest:
                    tmp_PluginName = "OpenXR";
                    break;
                case RuntimePlatformName.Pico:
                    tmp_PluginName = "Pico";
                    break;
                case RuntimePlatformName.VisionOS:
                    tmp_PluginName = "Apple visionOS";
                    break;
            }

            if (EnsureXRProvider.HasInstalledSDK(tmp_PluginName.ToLower()) ||
                currentRuntimePlatform is RuntimePlatformName.VisionOS or RuntimePlatformName.HandheldAR)
            {
                PlayerPrefs.DeleteKey(_CONST_DEVICE_SDK_TYPE);
                PlayerPrefs.DeleteKey($"{Application.productName}_{_CONST_XRMOD_INITIALIZED}");
                PlayerPrefs.DeleteKey($"{Application.productName}_{_CONST_ALLOW_APPLY_CONFIGURE}");

                // Apply configure after all asset import.
                PlayerPrefs.SetString($"{Application.productName}_{_CONST_ALLOW_APPLY_CONFIGURE}", "true");

                var tmp_OSTypeEnumField = rootVisualElement.Q<EnumField>("os_type");
                PlayerPrefs.SetString(_CONST_DEVICE_SDK_TYPE, currentRuntimePlatform.ToString());

                this.Close();

                switch (currentRuntimePlatform)
                {
                    case RuntimePlatformName.VisionOS:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.VisionOS,
                            BuildTarget.VisionOS);
                        break;
                    case RuntimePlatformName.HandheldAR:
                        switch (tmp_OSTypeEnumField.text)
                        {
                            case "iOS":
                                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
                                break;
                            case "Android":
                                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,
                                    BuildTarget.Android);
                                break;
                        }

                        break;
                    case RuntimePlatformName.Hololens:
#if UNITY_WSA
                    EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WSA, BuildTarget.WSAPlayer);
#endif
                        break;
                    case RuntimePlatformName.Pico:
                    case RuntimePlatformName.Quest:
                    case RuntimePlatformName.Rokid:
                    case RuntimePlatformName.XReal:
                        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var tmp_Request = Client.AddAndRemove(InitializerDb.Dependencies.ToArray());
                bool tmp_Prt = true;
                while (tmp_Prt)
                {
                    await Task.Delay(100);

                    if (tmp_Request.Status == StatusCode.Failure || tmp_Request.Status == StatusCode.Success)
                    {
                        // LogUtility.Log(tmp_Request.Error?.errorCode.ToString());
                        tmp_Prt = false;
                    }
                }

                ApplyConfigures();
            }
            else
            {
                if (EditorUtility.DisplayDialog("Warring",
                        "You also need to download the SDK for this platform.", "Get Help"))
                {
                    Help.BrowseURL(CONST_DOCS_URL);
                }
            }
        }

        internal static void ApplyConfigures()
        {
            var tmp_Assembly = AppDomain.CurrentDomain
                .GetAssemblies().FirstOrDefault(_assembly => _assembly.GetName().Name == "Phantom.XRMOD.Setup.Editor");
            var tmp_Type = tmp_Assembly?.GetTypes()
                .FirstOrDefault(_type => _type.FullName == "Phantom.XRMOD.Setup.Editor.XRMODEnginePreferences");
            var tmp_MethodInfo =
                tmp_Type?.GetMethod("AutoCreateConfigureFile", BindingFlags.Static | BindingFlags.Public);
            tmp_MethodInfo?.Invoke(null, new[] {PlayerPrefs.GetString(_CONST_DEVICE_SDK_TYPE)});
            PlayerPrefs.SetString(_CONST_ALLOW_APPLY_CONFIGURE, "false");
            PlayerPrefs.SetString(_CONST_XRMOD_INITIALIZED, "true");

            var tmp_XRMODBoostrapTemplateAsset =
                AssetDatabase.LoadAssetAtPath<TextAsset>($"{_CONST_ASSET_PATH_ROOT}/XRMODBootstrapTemplate.txt");

            var tmp_AllText = tmp_XRMODBoostrapTemplateAsset.text;
            tmp_AllText = tmp_AllText.Replace("#NAMESPACE#", Application.productName);
            tmp_AllText = tmp_AllText.Replace("#CLASSNAME#", Application.productName);

            var tmp_FolderPath = $"{Application.dataPath}/Scripts";
            if (!Directory.Exists(tmp_FolderPath))
            {
                Directory.CreateDirectory(tmp_FolderPath);
            }

            File.WriteAllText($"{tmp_FolderPath}/{Application.productName}XRMODBootstrap.cs", tmp_AllText);
            AssetDatabase.Refresh();
        }

        static void RestartUnityDelayCall()
        {
            if (EditorUtility.DisplayDialog("Restart Unity",
                    "We have installed and configured XRMOD for you, and we will **RESTART* your Unity for all configurations to take effect.",
                    "Restart", "Cancel"))
            {
                RestartUnityAction.RestartUnity();
            }
        }

        public static void InputSystemSetup()
        {
            var tmp_ProjectSettings =
                new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            tmp_ProjectSettings.FindProperty("activeInputHandler").intValue = 2;
            tmp_ProjectSettings.ApplyModifiedProperties();
        }

        [InitializeOnLoad]
        class AddRegisterSource
        {
            static AddRegisterSource()
            {
                EditorCoroutineUtility.StartCoroutineOwnerless(WaitForChoseSource());

                // Fix input system to both
                InputSystemSetup();

                // First time install
                Events.registeredPackages += _args =>
                {
                    PlayerPrefs.DeleteKey(_CONST_XRMOD_INITIALIZED);
                    CompilationPipeline.RequestScriptCompilation();
                };
            }


            static IEnumerator WaitForChoseSource()
            {
                var tmp_GloablPing = new Ping("packages.unity.com");
                while (!tmp_GloablPing.isDone)
                {
                    yield return null;
                }

                var tmp_ChinaPing = new Ping("registry.cn.phantomsxr.com");
                while (!tmp_ChinaPing.isDone)
                {
                    yield return null;
                }

                var tmp_IsGlobal = tmp_GloablPing.time < tmp_ChinaPing.time;
                string tmp_RegistryUrl =
                    tmp_IsGlobal ? "https://registry.npmjs.org/" : "https://registry.cn.phantomsxr.com/";
                UPMUtility.AddSource(tmp_RegistryUrl, "PhantomsXR");
            }
        }
    }
}