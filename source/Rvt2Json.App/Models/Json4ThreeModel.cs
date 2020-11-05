
using Rvt2Json.App.Models;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rvt2Json.App.Model
{
    [DataContract]
    public class Json4ThreeModel
    {
        [DataMember]
        public MetadataModel metadata { get; set; }
        [DataMember]
        public List<GeometryModel> geometries;
        [DataMember]
        public List<TextureModel> textures;
        [DataMember]
        public List<ImageModel> images;
        [DataMember]
        public List<MaterialModel> materials;
        [DataMember(Name = "object")]
        public ObjectModel obj { get; set; }
    }
}
