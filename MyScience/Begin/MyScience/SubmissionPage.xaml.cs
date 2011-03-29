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
            newButton.Click += new RoutedEventHandler(newButton_Click);
            DynamicPanel.Children.Add(newButton);

        }

        void newButton_Click(object sender, RoutedEventArgs e)
        {
            Project app = App.applist[App.currentIndex];
            List<Field> fields = GetFormField(app.Form);
            /*Get the values of all the fields*/
            int cur = 0;
            for (int i = 0; i < fields.Count; i++)
            {
                switch (fields[i].type)
                {
                    case "Question":
                        TextBox newTextBox = DynamicPanel.Children[cur+1] as TextBox;
                        fields[i].value = newTextBox.Text;
                        cur += 2;
                        break;
                }

            }
            /*Parse the fields list into Json String*/
            String data = GetJsonString(fields);
            MyScienceServiceClient client = new MyScienceServiceClient();
            client.SubmitDataCompleted += new EventHandler<SubmitDataCompletedEventArgs>(client_SubmitDataCompleted);
            client.SubmitDataAsync(0, App.applist[App.currentIndex].ID, 0, data, "location");
        }

        void client_SubmitDataCompleted(object sender, SubmitDataCompletedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        /*Parse the fields list into Json String*/
        private String GetJsonString(List<Field> fields)
        {
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<Field>));
            MemoryStream ms = new MemoryStream();
            for (int i = 0; i < fields.Count; i++)
            {
                serializer.WriteObject(ms, fields[i]);
            }
            byte[] stream = ms.ToArray();
            ms.Close();
            return Encoding.UTF8.GetString(stream, 0, stream.Length);
        }

        /*parsing Json to get fields required*/
        private List<Field> GetFormField(String form)
        {
            //List<Field> fields = new List<Field>();
            byte[] byteArray = Encoding.Unicode.GetBytes(form);
            MemoryStream stream = new MemoryStream( byteArray );
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(List<Field>));
            stream.Position = 0;
            //while (stream.Position < stream.Length)
            //{
            //    fields.Add((Field)(ser.ReadObject(stream)));
            //}
            var fields = ser.ReadObject(stream);
            stream.Close();
            return (List<Field>)fields;
            
        }

    }
}