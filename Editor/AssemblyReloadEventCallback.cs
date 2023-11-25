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

using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace XRMODInitializer.Editor
{
    public class AssemblyReloadEventCallback
    {
        [InitializeOnLoadMethod]
        private static void AssemblyReload()
        {
            EditorApplication.delayCall += DelayCallToApplyConfigures;
        }

        private static async void DelayCallToApplyConfigures()
        {
            var tmp_UnInitialized =
                PlayerPrefs.GetString(XRMODInitializerEditorWindow._CONST_XRMOD_INITIALIZED, "false") == "false";

            var tmp_AllowApplyConfigure =
                PlayerPrefs.GetString(XRMODInitializerEditorWindow._CONST_ALLOW_APPLY_CONFIGURE, "false") == "true";

            if (!tmp_UnInitialized || !tmp_AllowApplyConfigure) return;
            await Task.Delay(2000);
            XRMODInitializerEditorWindow.ApplyConfigures();
        }
    }
}