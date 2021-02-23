using System;
using System.Collections.Generic;
using System.Text;

namespace APKInfo {
    static class SdkOsMap {
        private static Dictionary<string, string> sdkOsDic = new Dictionary<string, string> {
            ["31"] = "12 S",
            ["30"] = "11 R",
            ["29"] = "10 Q",
            ["28"] = "9.0 P",
            ["27"] = "8.1 O",
            ["26"] = "8.0 O",
            ["25"] = "7.1 N",
            ["24"] = "7.0 N",
            ["23"] = "6.0 M",
            ["22"] = "5.1 L",
            ["21"] = "5.0 L",
            ["20"] = "4.4",
            ["19"] = "4.4",
            ["18"] = "4.3",
            ["17"] = "4.2",
            ["16"] = "4.1",
            ["15"] = "4.0.3 4.0.4",
            ["14"] = "4.0 4.01 4.02",
        };

        public static string getSDKOSDesc(string sdkVer) {
            string value = "";
            sdkOsDic.TryGetValue(sdkVer, out value);
            return value;
        }
    }
}
