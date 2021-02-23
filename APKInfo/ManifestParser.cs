using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.Collections;

namespace APKInfo {
    class ManifestParser {
        private System.Xml.XmlDocument mXmlDoc = null;

        public string packageName { set; get; }
        public string versionCode { set; get; }
        public string versionName { set; get; }
        public string minSdkVer { set; get; }
        public string targetSdkVer { set; get; }
        public string appName { set; get; }
        public string appIcon { set; get; }
        public string applicationClass { set; get; }
        public string lauchActivity { set; get; }
        public string nativeCode { set; get; }
        public Dictionary<string, string> metaData{ set; get; }
        public ArrayList activityLists { set; get; }
        public ArrayList receiverLists { set; get; }
        public ArrayList serviceLists { set; get; }
        public ArrayList providerLists { set; get; }

        // 输入：axml格式的AndroidManifest.xml文件全路径
        public bool initFromBinaryXml(string manifestFile) {
            string xml = "";
            //Open the stream and read it back.
            using (FileStream fs = File.OpenRead(manifestFile)) {
                // https://archive.codeplex.com/?p=androidxmldotnet
                var reader = new AndroidXml.AndroidXmlReader(fs);
                //XDocument doc = XDocument.Load(reader);

                // 转换后的文本里有p1:这种命名空间，不好解析，转成字符串替换掉后重新解析
                xml = XDocument.Load(reader).ToString().Replace("p1:", "");
                //var root = doc.Root;
                //string packageName = root.Attribute("package").Value;
                //XName xVal = XName.Get("versionCode", "p1");
                //string versionName = root.Attribute(xVal).ToString();
            }

            // 重新解析
            mXmlDoc = new System.Xml.XmlDocument();
            mXmlDoc.LoadXml(xml);
            var root = mXmlDoc.DocumentElement;
            this.packageName = root.GetAttribute("package");
            this.versionCode = root.GetAttribute("versionCode");
            this.versionName = root.GetAttribute("versionName");
            var node = root.SelectSingleNode("uses-sdk");
            this.minSdkVer = node.Attributes["minSdkVersion"].Value;
            this.targetSdkVer = node.Attributes["targetSdkVersion"].Value;
            node = root.SelectSingleNode("application");
            this.applicationClass = node.Attributes["name"].Value;
            var nodes = node.SelectNodes("meta-data");
            if (metaData == null) { metaData = new Dictionary<string, string>(); }
            foreach (XmlNode item in nodes) {
                metaData[item.Attributes["name"].Value] = item.Attributes["value"].Value;
            }

            // 四大组件,从application节点下取
            if (activityLists == null) { activityLists = new ArrayList(); }
            if (receiverLists == null) { receiverLists = new ArrayList(); }
            if (serviceLists == null) { serviceLists = new ArrayList(); }
            if (providerLists == null) { providerLists = new ArrayList(); }

            nodes = node.SelectNodes("activity");
            foreach (XmlNode item in nodes) { activityLists.Add(item.Attributes["name"].Value); }
            nodes = node.SelectNodes("receiver");
            foreach (XmlNode item in nodes) { receiverLists.Add(item.Attributes["name"].Value); }
            nodes = node.SelectNodes("service");
            foreach (XmlNode item in nodes) { serviceLists.Add(item.Attributes["name"].Value); }
            nodes = node.SelectNodes("provider");
            foreach (XmlNode item in nodes) { providerLists.Add(item.Attributes["name"].Value); }

            return true;
        }

        // 输入：aapt dump badging 1.apk 的内容
        public bool initFromAapt(string text) {
            appName = Utils.findSubstr(text, @"application-label:'", @"'");
            appIcon = Utils.findSubstr(text, @"icon='", @"'");
            lauchActivity = Utils.findSubstr(text, @"launchable-activity: name='", @"'");
            nativeCode = Utils.findSubstr(text, @"native-code: '", "\n");
            return true;
        }
    }
}
