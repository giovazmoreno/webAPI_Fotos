using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using WS_Posdata_PMKT_Fotos.Models.Object;
using System.Web;

namespace WS_Posdata_PMKT_Fotos.Models.Request
{
    [DataContract]
    public class FileReceptionRequest
    {
        [DataMember]
        public string IdSync { get; set; }
        [DataMember]
        public string IdStore { get; set; }
        [DataMember]
        public string TypeFile { get; set; }
        [DataMember]
        public List<DataFile> Files { get; set; }
       
    }
}