using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MyScienceServiceWebRole
{
    [DataContract]
    public class User
    {
        [DataMember]
        public int ID;

        [DataMember]
        public String Name;

        //[DataMember]
        //public String Description;

        [DataMember]
        public int Score;

        //[DataMember]
        //public int[] projectIDs

        //[DataMember]
        //public String phoneID;

    }
}