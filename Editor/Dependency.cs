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

using System.Collections.Generic;

namespace XRMODInitializer.Editor
{
    [System.Serializable]
    public class Dependency
    {
        public string PackageName;
        public string PackageVersion;
    }

    [System.Serializable]
    public class Dependencies
    {
        public string PlatformName;
        public List<Dependency> dependencies = new List<Dependency>();
    }
}