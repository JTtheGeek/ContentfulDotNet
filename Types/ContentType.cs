using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContentfulNetAPI.Types
{
    public class ContentTypeAlias
    {
        public string id { get; set; }
    }

    public class ContentType
    {
        public string id { get; set; }
        public string displayField { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public List<ContentTypeField> fields { get; set; }
    }


    public class ContentTypeField
    {
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public bool localized { get; set; }
        public bool required { get; set; }
        public bool disabled { get; set; }
        public string linkType { get; set; }
    }

    
}
