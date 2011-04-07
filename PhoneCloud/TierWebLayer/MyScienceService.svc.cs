using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using TierDataLayer;
using System.Collections.Generic;

namespace TierWebLayer
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class MyScienceService
    {
        [OperationContract]
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


        // Add more operations here and mark them with [OperationContract]

        [OperationContract]
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

        [OperationContract]
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


        [OperationContract]
        public List<User> GetUserProfile(String username, String phoneid)
        {
            MyScienceEntities db = new MyScienceEntities();
            var query = (from userobj in db.users
                         where userobj.name.ToLower() == username.ToLower() && userobj.phoneid == phoneid
                         select new User
                         {
                             ID = userobj.ID,
                             Name = userobj.name,
                             Score = (int)userobj.score
                         });
            return query.ToList<User>();
        }

        [OperationContract]
        public user RegisterUser(int id, String phoneid, String name)
        {
            //check to see if the user is in the database
            MyScienceEntities db = new MyScienceEntities();
            var query = (from userobj in db.users
                         where userobj.name.ToLower() == name.ToLower() && userobj.phoneid == phoneid
                         select new User
                         {
                             ID = userobj.ID,
                             Name = userobj.name,
                             Score = (int)userobj.score
                         });
            if(query.Count<User>() != 0)
                return null; //username already taken

            int idx = db.users.Count<user>() + 1;
            user userinfo = user.Createuser(idx, phoneid, name);
            userinfo.score = 0;
            db.users.AddObject(userinfo);
            int changes = db.SaveChanges();
            return userinfo;
        }
    }

}
