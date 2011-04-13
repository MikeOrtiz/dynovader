using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MyScienceServiceWebRole
{
    [DataContract]
    public class Submission
    {
        [DataMember]
        public int ID;
     
        [DataMember]
        public int ProjectID;
        
        [DataMember]
        public int UserID;
        
        [DataMember]
        public String Data;

        [DataMember]
        public String Location;
    }
}