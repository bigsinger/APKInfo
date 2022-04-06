using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace APKInfo.Model {

    // convert by https://www.bejson.com/convert/json2csharp/
    public class SDKItem {

        public int installCount { get; set; }

        public string uid { get; set; }

        public string serviceProvider { get; set; }

        public int id { get; set; }

        public string category { get; set; }

        public string title { get; set; }

        public string logoURL { get; set; }

        public int order { get; set; }
    }
    //public class SDKItems {
    //    public List<SDKItem> items { get; set; }
    //}

}
