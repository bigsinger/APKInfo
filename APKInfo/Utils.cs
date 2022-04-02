using System;
using System.Collections;
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
            if (string.IsNullOrEmpty(text)) {
                return res;
            }

            int p1 = 0;
            int preLen = 0;
            if (!string.IsNullOrEmpty(pre)) { p1 = text.IndexOf(pre); preLen = pre.Length; }
            if (p1 >= 0) {
                int p2 = text.IndexOf(tail, p1 + preLen);
                if (p2 == -1) {
                    p2 = text.Length;
                }
                res = text.Substring(p1 + preLen, p2 - p1 - preLen);
            }
            return res;
        }

        /// <summary>
        /// 获取文本中pre和tail包含的所有子串，但是要先找到tag1和tag2
        /// </summary>
        /// <param name="text"></param>
        /// <param name="pre"></param>
        /// <param name="tail"></param>
        /// <returns></returns>
        public static ArrayList findAllTags(string text, string tag1, string tag2, string pre, string tail) {
            if (string.IsNullOrEmpty(text)) { return null; }
            ArrayList res = new();

            int p1 = 0;
            int p2 = 0;
            while (true) {
                p1 = text.IndexOf(tag1, p2);
                if (p1 == -1) { break; }
                p1 = text.IndexOf(tag2, p1 + tag1.Length);
                if (p1 == -1) { break; }


                p1 = text.IndexOf(pre, p1 + tag2.Length);
                if (p1 == -1) { break; }
                p2 = text.IndexOf(tail, p1 + pre.Length);
                if (p2 == -1) { break; }
                res.Add(text.Substring(p1 + pre.Length, p2 - p1 - pre.Length));
            }

            return res;
        }

        public static Dictionary<string, string> findAllDictionary(string text, string tag, string tag1, string pre1, string tail1, string tag2, string pre2, string tail2) {
            if (string.IsNullOrEmpty(text)) { return null; }
            Dictionary<string, string> res = new();

            int p1 = 0;
            int p2 = 0;
            string k = null;
            string v = null;
            while (true) {
                p1 = text.IndexOf(tag, p2);
                if (p1 == -1) { break; }

                p1 = text.IndexOf(tag1, p1 + tag.Length);
                if (p1 == -1) { break; }
                p1 = text.IndexOf(pre1, p1 + tag1.Length);
                if (p1 == -1) { break; }
                p2 = text.IndexOf(tail1, p1 + pre1.Length);
                if (p2 == -1) { break; }
                k = text.Substring(p1 + pre1.Length, p2 - p1 - pre1.Length);

                p1 = text.IndexOf(tag2, p2 + tail1.Length);
                if (p1 == -1) { break; }
                p1 = text.IndexOf(pre2, p1 + tag2.Length);
                if (p1 == -1) { break; }
                p2 = text.IndexOf(tail2, p1 + pre2.Length);
                if (p2 == -1) { break; }
                v = text.Substring(p1 + pre2.Length, p2 - p1 - pre2.Length);

                res[k] = v;
            }

            return res;
        }

        // 从zip文件中解压出指定的文件
        public static bool extractZipFile(string zipPath, string entryName, string localDir) {
            bool success = false;
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
                        success = true;
                        break;
                    }
                }
            }
            return success;
        }
    }
}
