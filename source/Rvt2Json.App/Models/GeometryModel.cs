
using System.Runtime.Serialization;


namespace Rvt2Json.App.Models
{
    [DataContract]
    public class GeometryModel
    {
        [DataMember]
        public string uuid { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public GeometryDataModel data { get; set; }
    }


    [DataContract]
    public class GeometryDataModel
    {
        [DataMember]
        public AttributeModel attributes { get; set; }
        [DataMember]
        public IndexModel index { get; set; }
    }

    [DataContract]
    public class AttributeModel
    {
        public GeometryBasicModel position { get; set; }
        public GeometryBasicModel normal { get; set; }
        public GeometryBasicModel uv { get; set; }
    }

    [DataContract]
    public class IndexModel
    {
        [DataMember]
        public double[] array { get; set; }
        [DataMember]
        public string type { get; set; }
    }

    [DataContract]
    public class GeometryBasicModel
    {
        [DataMember]
        public int itemSize { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public double[] array { get; set; }
        [DataMember]
        public bool normalized { get; set; }
    }
}
