using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Json;

using MyScience.MyScienceService;

namespace MyScience
{
    public partial class Submission : PhoneApplicationPage
    {
        public Submission()
        {
            InitializeComponent();
        }

        private void SubmissionPage_Loaded(object sender, RoutedEventArgs e)
        {
            Project app = App.applist[App.currentIndex];
            PageTitle.Text = app.Name;
            List<Field> fields = GetFormField(app.Form);
            /*When submission page loaded, it will generate controls dynamically*/
            DynamicPanel.Children.Clear();
            for (int i = 0; i < fields.Count; i++)
            {
                switch (fields[i].type)
                {
                    case "Question":
                        var newTextBlock = new TextBlock { Name = "Question" + i.ToString(), Text = fields[i].label };
                        var newTextBox = new TextBox { Name = "Answer" + i.ToString() };
                        DynamicPanel.Children.Add(newTextBlock);
                        DynamicPanel.Children.Add(newTextBox);
                        break;
                }

            }
            //add button and event handler here
            var newButton = new Button { Name = "SubmitButton", Content = "Submit" };
            //newButton.Click += new System.EventHandler(newButton_click);


        }

        /*parsing Json to get fields required*/
        private List<Field> GetFormField(String form)
        {
            List<Field> fields = new List<Field>();
            byte[] byteArray = Encoding.Unicode.GetBytes(form);
            MemoryStream stream = new MemoryStream( byteArray );
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Field));
            stream.Position = 0;
            while (stream.Position < stream.Length)
            {
                fields.Add((Field)ser.ReadObject(stream));
            }
            return fields;
            
        }

    }
}