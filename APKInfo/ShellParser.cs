using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// ref: http://note.mimaz.org/?post=14
namespace APKInfo {
    static class ShellParser {
        private static Dictionary<string, string> shellDic = new Dictionary<string, string>{
            ["libchaosvmp.so"] = "娜迦",
            ["libedog.so"] = "娜迦",
            ["libddog.so"] = "娜迦",
            ["libfdog.so"] = "娜迦",
            ["libexec.so"] = "爱加密",
            ["libexecmain.so"] = "爱加密",
            ["libexecgame.so"] = "爱加密",
            ["libkdpdata.so"] = "几维",
            ["libkdp.so"] = "几维",
            ["libkwscmm.so"] = "几维",
            ["libsecexe.so"] = "梆梆",
            ["libsecmain.so"] = "梆梆",
            ["libSecShell.so"] = "梆梆",
            ["libDexHelper.so"] = "梆梆企业版",
            ["libprotectClass.so"] = "360",
            ["libjiagu.so"] = "360",
            ["libjiagu_art.so"] = "360",
            ["libegis.so"] = "通付盾",
            ["libNSaferOnly.so"] = "通付盾",
            ["libegis.so"] = "通付盾",
            ["libegisboot.so"] = "通付盾",
            ["libegismain.so"] = "通付盾",
            ["libbaiduprotect.so"] = "百度",
            ["libtup.so"] = "腾讯",
            ["libshellx"] = "腾讯",   // libshellx-2.10.6.0.so
            ["libtosprotection.so"] = "腾讯御安全",
            ["libshell.so"] = "腾讯",
            ["libtxRes.so"] = "腾讯",
            ["libnesec.so"] = "网易易盾",
            ["libdexfix.so"] = "网易易盾",
            ["libx3g.so"] = "顶象",
            ["libfakejni.so"] = "阿里",
            ["libzuma.so"] = "阿里",
            ["libzumadata.so"] = "阿里",
            ["libpreverify1.so"] = "阿里",
            ["libmobisec"] = "阿里",
            ["libaliutils"] = "阿里",
            ["libitsec.so"] = "海云安",
            ["libapktoolplus_jiagu.so"] = "海云安",
            ["libapssec.so"] = "盛大",
            ["librsprotect.so"] = "瑞星",
            ["libnqshield.so"] = "网秦",
            ["libuusafe.jar.so"] = "uu安全",
            ["libuusafe.so"] = "uu安全",
            ["libuusafeempty.so"] = "uu安全",
            ["libcmvmp.so"] = "中国移动",
            ["libmogosec"] = "中国移动",     //libmogosec_dex.so libmogosec_sodecrypt.so libmogosecurity.so
            ["libreincp.so"] = "珊瑚灵御",
            ["libAPKProtect"] = "APKProtect加固",
        };

        public static string whichShell(ICollection list) {
            foreach (var item in shellDic) {
                bool found = false;
                foreach (string so in list) {
                    if (so.Contains(item.Key)) {
                        found = true;
                        break;
                    }
                }

                if (found) {
                    return item.Value;
                }
            }

            return "no shell";
        }
    }
}
