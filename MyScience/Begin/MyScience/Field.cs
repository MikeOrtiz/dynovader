using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Runtime.Serialization;


namespace MyScience
{
    [DataContract]
    public class Field
    {
        [DataMember]
        public String type { get; set; }

        [DataMember]
        public String label { get; set; }
    }
}
