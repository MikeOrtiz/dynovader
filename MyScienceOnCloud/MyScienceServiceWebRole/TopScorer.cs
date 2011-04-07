using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace MyScienceServiceWebRole
{
    [DataContract]
    public class TopScorer
    {
        [DataMember]
        public int ID;


        [DataMember]
        public String Name;


        [DataMember]
        public int Score;
    }
}