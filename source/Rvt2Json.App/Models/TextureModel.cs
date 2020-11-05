
using System.Runtime.Serialization;

namespace Rvt2Json.App.Models
{
    [DataContract]
    public class TextureModel
    {
        [DataMember]
        public string uuid { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public double mapping { get; set; }
        [DataMember]
        public double[] repeat { get; set; }
        [DataMember]
        public double[] offset { get; set; }
        [DataMember]
        public double[] wrap { get; set; }
        [DataMember]
        public double minFilter { get; set; }
        [DataMember]
        public double magFilter { get; set; }
        [DataMember]
        public double anisotropy { get; set; }
        [DataMember]
        public bool flipY { get; set; }
        [DataMember]
        public string image { get; set; }
    }
}
