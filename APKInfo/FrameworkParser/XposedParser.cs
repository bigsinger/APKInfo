using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKInfo.FrameworkParser {
    /// <summary>
    /// xposed插件解析类
    /// </summary>
    internal class XposedParser {
        public string plugName { get; set; }
        public string plugDesc { get; set; }
        public string plugPackage { get; set; }

        public bool parse(string zipFilePath) {
            bool success = star.ZipHelper.SharpZip.extractZipFile(zipFilePath, "assets/xposed_init", PathManager.unzipDir);
            if (success) {
                string localFile = Path.Combine(PathManager.unzipDir, "assets/xposed_init");
                plugPackage = File.ReadAllText(localFile);
            }
            return success;
        }

    }
}
