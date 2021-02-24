using System;
using System.Collections;
using System.IO;
using System.IO.Compression;

namespace APKInfo {
    class Program {
        static void Main(string[] args) {
            if (args.Length < 1) {
                Console.WriteLine("usage: this apkFile");
                Console.ReadKey();
                return;
            }

            // 参数为apk文件全路径
            string inputFilePath = args[0];
            string zipFilePath = inputFilePath;

            // 初始化路径管理器：工具路径，工作目录
            PathManager.toolDir = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../tools")).LocalPath;
            PathManager.aaptPath = Path.Combine(PathManager.toolDir, "aapt.exe");
            PathManager.apksignerPath = Path.Combine(PathManager.toolDir, "apksigner.jar");
            PathManager.workDir = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../workDir")).LocalPath;
            if (!Directory.Exists(PathManager.workDir)) { Directory.CreateDirectory(PathManager.workDir); }
            
            // 每个apk处理时都创建一个子目录，作为临时处理目录
            string subWorkDirName = Utils.getSubWorkDirName(zipFilePath);
            PathManager.workDir = Path.Combine(PathManager.workDir, subWorkDirName);
            if (!Directory.Exists(PathManager.workDir)) { Directory.CreateDirectory(PathManager.workDir); }

            // 把apk解压到workDir\xxxMD5xxx\unzip目录下备用
            PathManager.unzipDir = Path.Combine(PathManager.workDir, "unzip");
            if (!Directory.Exists(PathManager.unzipDir)) {
                // 不再全部解压，比较费时
                Directory.CreateDirectory(PathManager.unzipDir);
                //ZipFile.ExtractToDirectory(zipFilePath, PathManager.unzipDir);

                Utils.extractZipFile(zipFilePath, "AndroidManifest.xml", PathManager.unzipDir);
            }

            string manifestFile = Path.Combine(PathManager.unzipDir, "AndroidManifest.xml");

            ManifestParser parser = new ManifestParser();
            parser.initFromBinaryXml(manifestFile);


            // aapt的方式获取apk信息

            // aapt不支持中文件路径，需要做下检查，如有中文创建临时文件
            if (Utils.hasChinese(zipFilePath)) {
                zipFilePath = PathManager.workDir + ".apk";
                if (!File.Exists(zipFilePath)) {
                    File.Copy(inputFilePath, zipFilePath, false);
                }
            }
            string res = Utils.runCmd(PathManager.aaptPath, string.Format("dump badging \"{0}\"", zipFilePath));
            zipFilePath = inputFilePath;
            parser.initFromAapt(res);

            Console.WriteLine(parser.appName + "\t" + parser.packageName + "\tversionName: " + parser.versionName + "\tversionCode: " + parser.versionCode);
            Console.WriteLine(string.Format("SDK: {0}({1})\ttargetSDK: {2}({3})\n", parser.minSdkVer, SdkOsMap.getSDKOSDesc(parser.minSdkVer), parser.targetSdkVer, SdkOsMap.getSDKOSDesc(parser.targetSdkVer)));
            Console.WriteLine(parser.applicationClass);
            Console.WriteLine(parser.lauchActivity);
            Console.WriteLine("nativeCode: " + parser.nativeCode);

            // 显示apk的图标文件
            Utils.extractZipFile(zipFilePath, parser.appIcon, PathManager.unzipDir);
            string iconFile = new Uri(Path.Combine(PathManager.unzipDir, parser.appIcon)).LocalPath;
            System.Diagnostics.Process.Start("explorer.exe", "/select," + iconFile);


            // so列表
            LibParser libParser = new LibParser();
            var soLists = libParser.getSoLists(zipFilePath);

            // 查壳
            Console.WriteLine("加壳信息：" + ShellParser.whichShell(soLists));

            // 签名信息
            Console.WriteLine("\n签名信息：\n" + SignHelper.getApkSignInfo(PathManager.apksignerPath, zipFilePath));

            //foreach (var item in soLists) {
            //    Console.WriteLine(item);
            //}

            // meta-data信息
            Console.WriteLine("\nmeta-data:");
            foreach (var item in parser.metaData) { Console.WriteLine(item.Key + " = " + item.Value); }

            Console.WriteLine("\npress Enter to Continue or exit...");
            var key = Console.ReadKey();
            if (key.Key != ConsoleKey.Enter) {
                return;
            }

            // 四大组件信息
            Console.WriteLine("\n四大组件:"); 
            Console.WriteLine("\nactivity:"); 
            foreach (var item in parser.activityLists) { Console.WriteLine(item); }
            Console.WriteLine("\nreceiver:"); 
            foreach (var item in parser.receiverLists) { Console.WriteLine(item); }
            Console.WriteLine("\nservice:"); 
            foreach (var item in parser.serviceLists) { Console.WriteLine(item); }
            Console.WriteLine("\nprovider:"); 
            foreach (var item in parser.providerLists) { Console.WriteLine(item); }
            

            Console.ReadKey();
        }
    }
}
