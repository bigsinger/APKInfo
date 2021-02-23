using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace APKInfo {
    static class Utils {

        public static string runCmd(string toolFile, string args) {
            string result = "";

            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = toolFile,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                }
            };

            proc.Start();
            while (!proc.StandardOutput.EndOfStream) {
                result += proc.StandardOutput.ReadLine() + "\n";
            }

            return result;
        }

        ////Process.StandardOutput使用注意事项 http://blog.csdn.net/zhangweixing0/article/details/7356841
        //private void runCmd(string toolFile, string args) {
        //    Process p;
        //    ProcessStartInfo psi;
        //    psi = new ProcessStartInfo(toolFile);
        //    psi.WorkingDirectory = System.AppDomain.CurrentDomain.BaseDirectory;
        //    psi.Arguments = args;
        //    psi.CreateNoWindow = true;
        //    psi.WindowStyle = ProcessWindowStyle.Hidden;
        //    psi.UseShellExecute = false;
        //    psi.RedirectStandardOutput = true;
        //    psi.RedirectStandardError = true;
        //    psi.StandardOutputEncoding = Encoding.UTF8;
        //    psi.StandardErrorEncoding = Encoding.UTF8;

        //    p = Process.Start(psi);

        //    p.OutputDataReceived += new DataReceivedEventHandler(OnDataReceived);
        //    p.BeginOutputReadLine();

        //    p.WaitForExit();

        //    if (p.ExitCode != 0) {
        //        LOGE(p.StandardError.ReadToEnd());
        //    }
        //    p.Close();
        //}
        public static string getMD5HashFromFile(string fileName) {
            using (var md5 = MD5.Create()) {
                using (var stream = File.OpenRead(fileName)) {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", string.Empty);
                }
            }
        }

        // 获取一个临时工作目录名
        public static string getSubWorkDirName(string filePath) {
            string res = "";
            //res = Utils.getMD5HashFromFile(filePath);  // 文件MD5计算比较耗时，暂时废弃
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if (Utils.hasChinese(fileName)) {
                res = Convert.ToBase64String(Encoding.UTF8.GetBytes(fileName));
            } else {
                res = fileName;
            }

            return res;
        }

        // 路径规范化
        public static string NormalizePath(string p) {
            return new Uri(p).LocalPath;
        }

        // 判断字符串中是否包含中文
        public static bool hasChinese(string str) {
            return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        }

        // 在一个字符串中根据pre和tail字符串查找并截取子串
        public static string findSubstr(string text, string pre, string tail) {
            string res = null;
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(pre)) {
                return res;
            }

            int p1 = text.IndexOf(pre);
            if (p1 >= 0) {
                int p2 = text.IndexOf(tail, p1 + pre.Length);
                if (p2 == -1) {
                    p2 = text.Length;
                }
                res = text.Substring(p1 + pre.Length, p2 - p1 - pre.Length);
            }
            return res;
        }

        // 从zip文件中解压出指定的文件
        public static bool extractZipFile(string zipPath, string entryName, string localDir) {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath)) {
                foreach (ZipArchiveEntry entry in archive.Entries) {
                    if (entry.FullName.Equals(entryName)) {
                        // Gets the full path to ensure that relative segments are removed.
                        string destinationPath = Path.GetFullPath(Path.Combine(localDir, entry.FullName));
                        string dir = Directory.GetParent(destinationPath).FullName;
                        if (!Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }
                        entry.ExtractToFile(destinationPath, true);
                        break;
                    }
                }
            }
            return true;
        }
    }
}
