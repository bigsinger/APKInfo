using System;
using System.Collections.Generic;
using System.Text;

namespace APKInfo
{
    static class PathManager
    {
        // 工作目录（MD5名称的文件夹）
        static public string workDir;

        // 工具目录
        static public string toolDir;
        static public string aaptPath;


        // 解压缩目录
        static public string unzipDir;

        // 存放结果的目录
        static public string resultDir = null;
    }
}
