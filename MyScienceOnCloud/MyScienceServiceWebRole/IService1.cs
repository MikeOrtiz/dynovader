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
        Uri SubmitData(int id, int projectid, int userid, String data, String location, int point, String contentType, byte[] imagedata);

        [OperationContract]
        List<TopScorer> GetTopScorers();

        [OperationContract]
        List<User> GetUserProfile(String username, String phoneID);

        [OperationContract]
        User RegisterUser(int id, String phoneid, String name);

        [OperationContract]
        User RegisterUserWithImage(int id, String phoneid, String name, String contentType, byte[] imagedata);

        [OperationContract]
        int GetProjectDataNum(int projectid);

        [OperationContract]
        List<Submission> GetUserSubmission(int userid);

        [OperationContract]
        byte[] GetUserImage(String username, String contentType);

        [OperationContract]
        int UploadUserImage(String username, String contentType, byte[] imagedata);
    }
}
