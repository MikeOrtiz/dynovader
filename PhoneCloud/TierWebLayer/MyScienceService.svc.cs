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
        public int SubmitData(int id, int projectid, int userid, String data, String location)
        {

            using (var db = new MyScienceEntities())
            {
                datum submission = datum.Createdatum(id, projectid, userid, data, DateTime.Now, location);
                db.data.AddObject(submission);
                int changes = db.SaveChanges();
                return changes;
            }
        }

    }
}
