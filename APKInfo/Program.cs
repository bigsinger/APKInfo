using APKInfo.FrameworkParser;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace APKInfo {
    class Program {
        static void Main(string[] args) {
            string cmdTag = "all";
            if (args.Length < 1) {
                Console.WriteLine("usage: this apkFile");
                Console.ReadKey();
                return;
            }
            if (args.Length>=2) {
                cmdTag = args[1];
            }

            // 判断文件是否存在
            string filePath = args[0];
            if (!File.Exists(filePath)) {
                Console.WriteLine("file not exists: " + filePath);
                Console.ReadKey();
                return;
            }

            try {
                preWork(filePath);
                Parse(filePath, cmdTag);
            } catch (Exception e) {
                Console.WriteLine(args[0]);
                Console.WriteLine(e.Message);
            }
            Console.ReadKey();
        }


        static private void Parse(string filePath, string cmdTag) {
            switch (cmdTag) {
                case "sdk":
                    parseSDK(filePath);
                    break;
                case "all":
                default:
                    ParseAll(filePath);
                    break;
            }
        }

        static private void preWork(string inputFilePath) {
            string ext = Path.GetExtension(inputFilePath).ToLower();
            if (ext == ".apk") {
                PathManager.zipFilePath = inputFilePath;

                // 初始化路径管理器：工具路径，工作目录
                PathManager.toolDir = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./tools")).LocalPath;
                PathManager.aaptPath = Path.Combine(PathManager.toolDir, "aapt.exe");
                PathManager.apksignerPath = Path.Combine(PathManager.toolDir, "apksigner.jar");
                PathManager.workDir = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./workDir")).LocalPath;
                if (!Directory.Exists(PathManager.workDir)) { Directory.CreateDirectory(PathManager.workDir); }

                // 每个apk处理时都创建一个子目录，作为临时处理目录
                string subWorkDirName = Utils.getSubWorkDirName(PathManager.zipFilePath);
                PathManager.workDir = Path.Combine(PathManager.workDir, subWorkDirName);
                if (!Directory.Exists(PathManager.workDir)) { Directory.CreateDirectory(PathManager.workDir); }

                // 把apk解压到workDir\xxxMD5xxx\unzip目录下备用
                PathManager.unzipDir = Path.Combine(PathManager.workDir, "unzip");
                if (!Directory.Exists(PathManager.unzipDir)) {
                    // 不再全部解压，比较费时
                    Directory.CreateDirectory(PathManager.unzipDir);
                    //ZipFile.ExtractToDirectory(zipFilePath, PathManager.unzipDir);
                }

                // aapt不支持中文件路径，需要做下检查，如有中文创建临时文件
                if (Utils.hasChinese(PathManager.zipFilePath)) {
                    PathManager.zipFilePath = PathManager.workDir + ".apk";
                    if (!File.Exists(PathManager.zipFilePath)) {
                        File.Copy(inputFilePath, PathManager.zipFilePath, false);
                    }
                }
            } else if (ext == ".dex") {

            }

        }

        /// <summary>
        /// 参数为APK文件全路径
        /// </summary>
        /// <param name="inputFilePath"></param>
        static private void ParseAll(string inputFilePath) {
            string zipFilePath = PathManager.zipFilePath;
            bool noAMFile = false;

            string manifestFile = Path.Combine(PathManager.unzipDir, "AndroidManifest.xml");
            ManifestParser parser = new ManifestParser();
            try {
                if (!File.Exists(manifestFile)) {
                    Utils.extractZipFile(zipFilePath, "AndroidManifest.xml", PathManager.unzipDir);
                }
                parser.initFromBinaryXml(manifestFile);
            } catch (Exception e) {
                noAMFile = true;
                Console.Write("解压AndroidManifest.xml文件失败，文件可能存在伪加密或者非法APK包：");
                Console.WriteLine(e.Message);
            }


            // aapt的方式获取apk信息
            string res = Utils.runCmd(PathManager.aaptPath, string.Format("dump badging \"{0}\"", zipFilePath));
            zipFilePath = inputFilePath;
            parser.initFromAaptBadging(res);

            Console.WriteLine(parser.appName + "\t" + parser.packageName + "\tversionName: " + parser.versionName + "\tversionCode: " + parser.versionCode);
            Console.WriteLine(string.Format("SDK: {0}({1})\ttargetSDK: {2}({3})\n", parser.minSdkVer, SdkOsMap.getSDKOSDesc(parser.minSdkVer), parser.targetSdkVer, SdkOsMap.getSDKOSDesc(parser.targetSdkVer)));
            Console.WriteLine(parser.applicationClass);
            Console.WriteLine(parser.lauchActivity);
            Console.WriteLine("nativeCode: " + parser.nativeCode);

            // 显示apk的图标文件
            try {
                Utils.extractZipFile(zipFilePath, parser.appIcon, PathManager.unzipDir);
                string iconFile = new Uri(Path.Combine(PathManager.unzipDir, parser.appIcon)).LocalPath;
                System.Diagnostics.Process.Start("explorer.exe", "/select," + iconFile);
            } catch (Exception e) {
                Console.Write("appIcon: " + parser.appIcon + " 解压失败，文件可能存在伪加密或者非法APK包：");
                Console.WriteLine(e.Message);
            }


            // so列表
            LibParser libParser = new LibParser();
            var soLists = libParser.getSoLists(zipFilePath);

            // 查壳
            Console.WriteLine("加壳信息：" + ShellParser.whichShell(soLists));

            // 是否是xposed插件
            XposedParser xposedParser = new XposedParser();
            if (xposedParser.parse(zipFilePath)) {
                Console.WriteLine("APP为Xposed插件，包名: " + xposedParser.plugPackage);
            }

            // 签名信息
            try {
                Console.WriteLine("\n签名信息：\n" + SignHelper.getApkSignInfo(PathManager.apksignerPath, zipFilePath));
            } catch (Exception e) {
                Console.WriteLine(e.Message +  " 本地缺少Java环境");
            }

            //foreach (var item in soLists) {
            //    Console.WriteLine(item);
            //}

            if (noAMFile) {
                parser.initFromAaptXmlTree(Utils.runCmd(PathManager.aaptPath, string.Format("dump xmltree \"{0}\" AndroidManifest.xml", zipFilePath)));
            }

            // meta-data信息
            if (parser.metaData != null) {
                Console.WriteLine("\nmeta-data:");
                foreach (var item in parser.metaData) { Console.WriteLine(item.Key + " = " + item.Value); }
            }

            Console.WriteLine("\n按下 Enter 按键继续分析四大组件, 或直接退出...");
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


            // 解析资源文件中的字符串
            Console.WriteLine("\n按下 Enter 按键继续分析资源情况, 或直接退出...");
            key = Console.ReadKey();
            if (key.Key != ConsoleKey.Enter) {
                return;
            }

            string resourceFile = Path.Combine(PathManager.unzipDir, "resources.arsc");
            try {
                if (!File.Exists(resourceFile)) {
                    Utils.extractZipFile(zipFilePath, "resources.arsc", PathManager.unzipDir);
                }
                readResource(resourceFile);
            } catch (Exception e) {
                noAMFile = true;
                Console.Write("解压 resources.arsc 文件失败，文件可能存在伪加密或者非法APK包：");
                Console.WriteLine(e.Message);
            }


            Console.WriteLine("\n按下 Enter 按键继续分析SDK情况, 或直接退出...");
            key = Console.ReadKey();
            if (key.Key != ConsoleKey.Enter) {
                return;
            }
            parseSDK(zipFilePath);
        }


        /*
         参考：https://github.com/DKorablin/ApkReader
        先下载release的ApkReader.xml并添加到工程引用
         */
        static void readResource(string resourceFilePath) {
            //Byte[] resourceBytes = File.ReadAllBytes(resourceFilePath);
            //ArscFile resources = new ArscFile(resourceBytes);

            //foreach (var item in resources.ResourceMap) {
            //    Console.WriteLine(item.Key + "\t\t\t" + string.Join("; ", item.Value.Select(p => p.Value).ToArray()));
            //}

            ////OldResourceFile resources2 = new OldResourceFile(resourceBytes);
            ////var table2 = resources2.ResourceMap;
            //foreach (var item in resources.ResourceMap) {
            //    string key = "@" + item.Key.ToString("X4");
            //    string value1 = string.Join(";", item.Value.Select(p => p.Value).ToArray());
            //    //string value2 = string.Join(";", table2[key].ToArray());
            //    //if(!string.Equals(value1, value2))
            //    //	throw new Exception("Not equal");
            //}
        }

        static void parseSDK(string filePath) {
            if (!File.Exists(PathManager.sdkPath)) {
                Console.WriteLine("SDK配置文件不存在: " + PathManager.sdkPath);
                Console.ReadKey();
                return;
            }

            string classLists = "";
            string ext = Path.GetExtension(filePath).ToLower();
            if (ext == ".apk") {
                var dexes = star.ZipHelper.SharpZip.getFilesUnderZipRoot(filePath, ".dex");
                foreach (var dex in dexes) {
                    Console.WriteLine("parsing: " + dex.ToString());
                    string dexFile = Path.Combine(PathManager.unzipDir, dex.ToString());
                    if (!File.Exists(dexFile)) {
                        Utils.extractZipFile(filePath, dex.ToString(), PathManager.unzipDir);
                    }
                    classLists += Utils.runCmd("java", string.Format("-jar \"{0}\" list classes  \"{1}\"", PathManager.baksmaliPath, dexFile)) + "\n";
                }
            } else if (ext == ".dex") {
                string dexFile = filePath;
                Console.WriteLine("parsing: " + dexFile);
                classLists += Utils.runCmd("java", string.Format("-jar \"{0}\" list classes  \"{1}\"", PathManager.baksmaliPath, dexFile));
            } else {
                Console.WriteLine("invalid file");
                Console.ReadKey();
                return;
            }

            if (!string.IsNullOrEmpty(classLists)) {
                classLists = classLists.Replace('/', '.');
                List<Model.SDKItem> hitSDKs = new();
                try {
                    string json = File.ReadAllText(PathManager.sdkPath);
                    Model.SDKItems sdks = Newtonsoft.Json.JsonConvert.DeserializeObject<Model.SDKItems>(json);
                    foreach (var item in sdks.items) {
                        if (classLists.Contains(item.uid)) {
                            hitSDKs.Add(item);
                            Console.WriteLine(string.Format("{0}\t{1}\t{2}", item.uid, item.title, item.serviceProvider));
                        }
                    }
                } catch (Exception) {
                    throw;
                }
            }
        }

    }
}
