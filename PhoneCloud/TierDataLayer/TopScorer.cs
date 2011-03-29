using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;


namespace TierDataLayer
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