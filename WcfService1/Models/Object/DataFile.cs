using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace WS_Posdata_PMKT_Fotos.Models.Object
{
    [DataContract]
    public class DataFile
    {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string TypeOfEvidence { get; set; }

        [DataMember]
        public Byte[] File { get; set; }

        [DataMember]
        public DateTime FileCreationDate { get; set; }
    }
}