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

using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XRMODInitializer.Editor
{
    //[Obsolete("", true)]
    public class InitializerDb : ScriptableObject
    {
        // public bool Intialized;
        // public string DeviceSDKType;
        // public bool EnableForApplyConfigure;

        public List<string> Dependencies = new List<string>();

        // internal XRMODInitializerEditorWindow window;

        // private static InitializerDb _INITIALIZER_DB;

        // public static InitializerDb INSTANCE
        // {
        //     get
        //     {
        //         var tmp_FileStr = "Assets/Scripts/XRMODInitializer/Editor/Assets/InitializerDb.asset";
        //         _INITIALIZER_DB = AssetDatabase.LoadAssetAtPath<InitializerDb>(tmp_FileStr);
        //         if (_INITIALIZER_DB != null) return _INITIALIZER_DB;
        //
        //         _INITIALIZER_DB = CreateInstance<InitializerDb>();
        //         AssetDatabase.CreateAsset(_INITIALIZER_DB, tmp_FileStr);
        //         AssetDatabase.SaveAssets();
        //         AssetDatabase.Refresh();
        //         return _INITIALIZER_DB;
        //     }
        // }
    }
}