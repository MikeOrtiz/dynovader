using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.Collections.Specialized;

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

        public Uri SubmitData(int id, int projectid, int userid, String data, String location, int point, String contentType, byte[] imagedata)
        {
            EnsureContainerExists();
            DateTime time = DateTime.Now;
            String imagename = userid.ToString() + "-" + time.ToFileTime().ToString() + ".jpg";
            var blob = this.GetContainer().GetBlobReference(imagename);
            blob.Properties.ContentType = contentType;

            var metadata = new NameValueCollection();
            metadata["SubmissionID"] = id.ToString();
            metadata["ProjectID"] = projectid.ToString();
            metadata["UserID"] = userid.ToString();
            metadata["Time"] = DateTime.Now.ToString();

            blob.Metadata.Add(metadata);
            blob.UploadByteArray(imagedata);

            using (var db = new MyScienceEntities())
            {
                datum submission = datum.Createdatum(id, projectid, userid, data, DateTime.Now, location);
                db.data.AddObject(submission);
                user curUser = (from auser in db.users
                                where auser.ID == userid
                                select auser).First();
                curUser.score = curUser.score + point;
                int changes = db.SaveChanges();
                //return changes;
            }

            return blob.Uri;
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
            if (query.Count<User>() != 0)
                return null; //username already taken

            int idx = db.users.Count<user>() + 1;
            user userinfo = user.Createuser(idx, phoneid, name);
            userinfo.score = 0;
            db.users.AddObject(userinfo);
            int changes = db.SaveChanges();
            return userinfo;
        }

        public List<Submission> GetProjectData(int projectid)
        {
            MyScienceEntities db = new MyScienceEntities();
            var query = (from sub in db.data
                         where sub.projectid == projectid
                         select new Submission
                         {
                             ID = sub.ID,
                             ProjectID = sub.projectid,
                             UserID = sub.userid,
                             Data = sub.data,
                             Location = sub.location
                         });
            return query.ToList<Submission>();
        }

        private CloudBlobContainer GetContainer()
        {
            var account = CloudStorageAccount.FromConfigurationSetting("BlobConnection");
            var client = account.CreateCloudBlobClient();
            return client.GetContainerReference(RoleEnvironment.GetConfigurationSettingValue("ContainerName"));
        }

        private void EnsureContainerExists()
        {
            var container = GetContainer();
            container.CreateIfNotExist();
            var permissions = container.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            container.SetPermissions(permissions);
        }

        //public void UploadImage(int submissionid, int projectid, int userid, DateTime time, String contentType, byte[] data)
        //{
        //    string name = userid.ToString() + " " + time.ToString();
        //    var blob = this.GetContainer().GetBlobReference(name);
        //    blob.Properties.ContentType = contentType;

        //    var metadata = new NameValueCollection();
        //    metadata["SubmissionID"] = submissionid.ToString();
        //    metadata["ProjectID"] = projectid.ToString();
        //    metadata["UserID"] = userid.ToString();
        //    metadata["Time"] = time.ToString();

        //    blob.Metadata.Add(metadata);
        //    blob.UploadByteArray(data);
        //}
    }
}
