
using System.Runtime.Serialization;

namespace Rvt2Json.App.Models
{
    [DataContract]
    public class MaterialModel
    {
        [DataMember]
        public string uuid { get; set; }
        [DataMember]
        public string name { get; set; }
        [DataMember]
        public string type { get; set; }
        [DataMember]
        public int color { get; set; }
        [DataMember]
        public int ambient { get; set; }
        [DataMember]
        public int emissive { get; set; }
        [DataMember]
        public int specular { get; set; }
        [DataMember]
        public int shininess { get; set; }
        [DataMember]
        public double opacity { get; set; }
        [DataMember]
        public bool transparent { get; set; }
        [DataMember]
        public bool wireframe { get; set; }
    }
}
