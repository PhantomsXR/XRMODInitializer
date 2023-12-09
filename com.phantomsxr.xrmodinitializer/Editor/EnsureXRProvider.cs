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

using System.Linq;
using UnityEditor;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Assertions;

namespace XRMODInitializer.Editor
{
    public class EnsureXRProvider
    {
        public static bool HasInstalledSDK(string _pluginName)
        {
            var tmp_HasBeenInstallSDK = XRPackageMetadataStore
                .GetAllPackageMetadata().FirstOrDefault(_package => _package.metadata.packageId.Contains(_pluginName));
            return tmp_HasBeenInstallSDK != default;
        }
    }
}