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
using System.IO;
using UnityEditor;
using UnityEngine;

namespace XRMODInitializer.Editor
{
    [InitializeOnLoad]
    public static class LogUtility
    {
        private static readonly string LOG_FILE_PATH;

        static LogUtility()
        {
            LOG_FILE_PATH = Path.Combine(XRMODInitializerEditorWindow._CONST_ASSET_PATH_ROOT,
                "XRMODInitializerLog.log");
            Application.logMessageReceived += LogMessage;
        }

        public static void Log(string _msg)
        {
            Debug.Log(_msg);
        }

        private static void LogMessage(string _logString, string _stackTrace, LogType _type)
        {
            if (_type == LogType.Warning) return;

            string logEntry = $"[{_type}] [{DateTime.Now}] {_logString}\n";
            if (_type == LogType.Exception)
            {
                logEntry += _stackTrace + "\n";
            }

            File.AppendAllText(LOG_FILE_PATH, logEntry);
        }
    }
}