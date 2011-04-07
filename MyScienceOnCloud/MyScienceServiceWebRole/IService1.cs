using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace MyScienceServiceWebRole
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        List<Project> GetProjects();

        [OperationContract]
        int SubmitData(int id, int projectid, int userid, String data, String location, int point);

        [OperationContract]
        List<TopScorer> GetTopScorers();

        [OperationContract]
        List<User> GetUserProfile(String username, String phoneID);

    }
}
