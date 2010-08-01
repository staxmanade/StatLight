using System.ServiceModel;
using System.ServiceModel.Activation;

namespace StatLight.RemoteIntegration.Web
{
    [ServiceContract(Namespace = "")]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class Service1
    {
        [OperationContract]
        public int DoWork()
        {
            return 42;
        }
    }
}
