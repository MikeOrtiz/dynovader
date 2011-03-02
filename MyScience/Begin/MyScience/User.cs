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

namespace MyScience
{
    public class User
    {
        private string name;
        private int ID;
        private Project[] userProjects;

        public User(string name, int ID, Project[] projects)
        {
            this.name = name;
            this.ID = ID;
            userProjects = projects;
        }

        public string getName()
        {
            return name;
        }

        public int getID()
        {
            return ID;
        }

        public Project[] getUserProjects()
        {
            return userProjects;
        }
    }
}
