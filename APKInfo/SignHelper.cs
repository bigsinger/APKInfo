using System;
using System.Collections.Generic;
using System.Text;

// keytool,只支持V1签名校验
namespace APKInfo {
    class SignHelper {

        // 获取签名信息
        public static string getApkSignInfo(string apksigner, string apkFilePath) {
            string res = Utils.runCmd("java", string.Format("-jar {0} verify -v --print-certs \"{1}\"", apksigner, apkFilePath));
            return Utils.findSubstr(res, null, "\nWARNING");
        }

        // 验证签名信息
        public static string verifyApkSign(string apksigner, string apkFilePath) {
            string res = Utils.runCmd("java", string.Format("-jar {0} verify -v \"{1}\"", apksigner, apkFilePath));
            return Utils.findSubstr(res, null, "\nWARNING");
        }
    }
}
