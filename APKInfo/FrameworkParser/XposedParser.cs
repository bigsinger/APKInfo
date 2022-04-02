using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKInfo.FrameworkParser {
    internal class XposedParser {
        public string plugName { get; set; }
        public string plugDesc { get; set; }
        public string plugPackage { get; set; }

        public bool parse(string zipFilePath) {
            bool success = Utils.extractZipFile(zipFilePath, "assets/xposed_init", PathManager.unzipDir);
            if (success) {
                string localFile = Path.Combine(PathManager.unzipDir, "assets/xposed_init");
                plugPackage = File.ReadAllText(localFile);
            }
            return success;
        }

    }
}
