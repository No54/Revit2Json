using System;
using System.Collections.Generic;

using System.Runtime.Serialization;

namespace Rvt2Json.App.Models
{
    [DataContract]
    public class ImageModel
    {
        [DataMember]
        public string uuid { get; set; }
        [DataMember]
        public string url { get; set; }
    }
}
