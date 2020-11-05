using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rvt2Json.App.Models
{
    [DataContract]
    public class ObjectModel
    {
        [DataMember]
        public string uuid { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public double[] matrix { get; set; }
        [DataMember]
        public List<ObjectModel> children { get; set; }
        [DataMember]
        public string geometry { get; set; }
        [DataMember]
        public string material { get; set; }
        [DataMember]
        public Dictionary<string, string> userData { get; set; }
    }
}
