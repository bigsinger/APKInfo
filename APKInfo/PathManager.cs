using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace APKInfo {
    static class PathManager {
        // 待处理的文件全路径
        static public string inputFilePath;

        // 待处理的压缩包文件全路径
        static public string zipFilePath;

        // 工作目录（MD5名称的文件夹）
        static public string workDir;

        // 工具目录
        static public string toolDir;
        static public string aaptPath;
        static public string apksignerPath;

        // 解压缩目录
        static public string unzipDir;

        // 存放结果的目录
        static public string resultDir = null;

        /// <summary>
        /// tool目录
        /// </summary>
        static private string toolPath;
        static public string ToolPath {
            get {
                if (toolPath == null) { toolPath = Path.Combine(AppContext.BaseDirectory, "tools"); }
                return toolPath;
            }

            set {
                toolPath = value;
            }
        }

        // smali 路径
        static private string _smali;
        static public string smaliPath {
            get {
                if (_smali == null) { _smali = Path.Combine(ToolPath, "smali\\smali.jar"); }
                return _smali;
            }
        }
        // baksmali 路径
        static private string _baksmali;
        static public string baksmaliPath {
            get {
                if (_baksmali == null) { _baksmali = Path.Combine(ToolPath, "smali\\baksmali.jar"); }
                return _baksmali;
            }
        }

        // 三方SDK配置信息 路径
        static private string _sdkPath;
        static public string sdkPath {
            get {
                if (_sdkPath == null) { _sdkPath = Path.Combine(ToolPath, "android_SDK.txt"); }
                return _sdkPath;
            }
        }
    }
}
