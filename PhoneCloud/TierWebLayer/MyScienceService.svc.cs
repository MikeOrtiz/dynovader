using System;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.Collections.Generic;
using TierDataLayer;

namespace TierWebLayer
{
    [ServiceContract(Namespace = "")]
    [SilverlightFaultBehavior]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class MyScienceService
    {
        [OperationContract]
        public List<TierDataLayer.Project> GetProjects()
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
            return query.ToList();
        }

        // Add more operations here and mark them with [OperationContract]
    }
}
