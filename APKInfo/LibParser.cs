using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace APKInfo {
    class LibParser {

        // 遍历获取文件后缀为.so的文件列表，注意非/lib目录下的也可以获取，例如assets/libjiagu.so
        public ICollection getSoLists(string zipFilePath) {
            Hashtable arr = new Hashtable();
            ICSharpCode.SharpZipLib.Zip.ZipInputStream zip = new ICSharpCode.SharpZipLib.Zip.ZipInputStream(File.OpenRead(zipFilePath));
            ICSharpCode.SharpZipLib.Zip.ZipEntry item = null;

            try {
                while ((item = zip.GetNextEntry()) != null) {
                    if (item.Name.Contains(".so")) {
                        int pos = item.Name.IndexOf("/lib");
                        if (pos != -1) {
                            arr.Add(item.Name.Substring(pos + 1), true);
                        }
                    }
                }
            } catch (Exception) {
            }

            return arr.Keys;
        }

        public void recognizeSoLists() {

        }
    }
}
