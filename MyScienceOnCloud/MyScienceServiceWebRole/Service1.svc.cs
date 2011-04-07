﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace MyScienceServiceWebRole
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    public class Service1 : IService1
    {
        public List<Project> GetProjects()
        {
            MyScienceEntities db = new MyScienceEntities();
            var query = (from app in db.projects
                         select new Project
                         {
                             ID = app.ID,
                             Name = app.name,
                             Description = app.description,
                             Form = app.form,
                             Owner = app.owner
                         });
            return query.ToList<Project>();
        }

        public int SubmitData(int id, int projectid, int userid, String data, String location, int point)
        {

            using (var db = new MyScienceEntities())
            {
                datum submission = datum.Createdatum(id, projectid, userid, data, DateTime.Now, location);
                db.data.AddObject(submission);
                user curUser = (from auser in db.users
                                where auser.ID == userid
                                select auser).First();
                curUser.score = curUser.score + point;
                int changes = db.SaveChanges();
                return changes;
            }
        }

        
        public List<TopScorer> GetTopScorers()
        {
            MyScienceEntities db = new MyScienceEntities();
            //var query = @"SELECT * from db.users ORDER BY Score DESC LIMIT 0,10";
            var query = (from tscorer in db.users
                         orderby tscorer.score descending
                         select new TopScorer
                         {
                             ID = tscorer.ID,
                             Name = tscorer.name,
                             Score = (int)tscorer.score,
                         }
                         );
            return query.ToList<TopScorer>();
        }


     
        public List<User> GetUserProfile(String username, String phoneID)
        {
            MyScienceEntities db = new MyScienceEntities();
            var query = (from user in db.users
                         where user.name.ToLower() == username.ToLower()// && user.phoneid == phoneID
                         select new User
                         {
                             ID = user.ID,
                             Name = user.name,
                             Score = (int)user.score
                         });
            return query.ToList<User>();
        }

        //[OperationContract]
        //public void UpdateScore(int userID, int point)
        //{
        //    MyScienceEntities db = new MyScienceEntities();
        //    user curUser = (from auser in db.users
        //                    where auser.ID == userID
        //                    select auser).First();
        //    curUser.score = curUser.score + point;
        //    db.SaveChanges();
        //}
    }
}
