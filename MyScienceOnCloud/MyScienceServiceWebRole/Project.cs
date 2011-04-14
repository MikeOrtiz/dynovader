using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MyScienceServiceWebRole
{
    [DataContract]
    public class Project
    {
        [DataMember]
        public int ID;

        [DataMember]
        public String Name;

        [DataMember]
        public String Description;

        [DataMember]
        public String Form;

        [DataMember]
        public int Owner;

    }
}