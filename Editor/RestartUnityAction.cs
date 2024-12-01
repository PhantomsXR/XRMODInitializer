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

using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace XRMODInitializer.Editor
{
    public class RestartUnityAction
    {
        public static void RestartUnity()
        {
            string tmp_UnityEditorPath = Path.Combine(EditorApplication.applicationPath, GetPlatformExecutor());
            string tmp_CurrentProjecFullPath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));

            string tmp_ShellScriptPath = $"{XRMODInitializerEditorWindow._CONST_ASSET_PATH_ROOT}restart_unity.sh";
            string tmp_Arguments = $"{tmp_ShellScriptPath} \"{tmp_UnityEditorPath}\" \"{tmp_CurrentProjecFullPath}\"";

            LogUtility.Log(tmp_Arguments);

            ProcessStartInfo startInfo = new ProcessStartInfo("/bin/bash", tmp_Arguments);
            Process process = new Process();
            process.StartInfo = startInfo;
            process.Start();

            EditorApplication.Exit(0);
        }

        static string GetPlatformExecutor()
        {
#if UNITY_EDITOR_OSX
            return "Contents/MacOS/Unity";
#else
            return "";
#endif
        }
    }
}