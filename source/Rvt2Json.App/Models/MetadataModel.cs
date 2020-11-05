
using System.Runtime.Serialization;

namespace Rvt2Json.App.Models
{
    [DataContract]
    public class MetadataModel
    {
        [DataMember]
        public string version { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public string generator { get; set; }
    }
}
