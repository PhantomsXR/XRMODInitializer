// // /*===============================================================================
// // Copyright (C) 2024 PhantomsXR Ltd. All Rights Reserved.
// //
// // This file is part of the XRMODInitializer.Editor.
// //
// // The UnityVisionOSLibTest cannot be copied, distributed, or made available to
// // third-parties for commercial purposes without written permission of PhantomsXR Ltd.
// //
// // Contact info@phantomsxr.com for licensing requests.
// // ===============================================================================*/

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

namespace XRMODInitializer.Editor
{
    [InitializeOnLoad]
    public static class ActivateCorrespondingVersionDll
    {
        static ActivateCorrespondingVersionDll()
        {
            string tmp_UnityVersion = Application.unityVersion;
            AddDefineSymbol(tmp_UnityVersion.Contains("6000") ? "UNITY_6000_0_OR_NEW" : "UNITY_2022_0_OR_NEW");
        }


        private static NamedBuildTarget GetCurrentBuildGroup()
        {
#if UNITY_VISIONOS
            return NamedBuildTarget.VisionOS;
#elif UNITY_ANDROID
            return NamedBuildTarget.Android;
#elif UNITY_IOS
            return NamedBuildTarget.iOS;
#elif UNITY_WEBGL
             return NamedBuildTarget.WebGL;
#else
            return NamedBuildTarget.Standalone;
#endif
        }

        private static void AddDefineSymbol(string _symbol)
        {
            // 获取当前已有的 Script Define Symbols
            string[] tmp_Defines = PlayerSettings.GetScriptingDefineSymbols(GetCurrentBuildGroup()).Split(";");
            // 如果已经有该 define，跳过
            if (tmp_Defines.Contains(_symbol))
            {
                return;
            }

            List<string> tmp_DefineList = new();
            tmp_DefineList.AddRange(tmp_Defines);
            tmp_DefineList.Add(_symbol); 
            PlayerSettings.SetScriptingDefineSymbols(GetCurrentBuildGroup(), tmp_DefineList.ToArray());
        }
    }
}