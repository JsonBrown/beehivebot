using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace beehive.extensions.Notifications
{
    [ServiceContract]
    public interface INotificationHost
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "/{*arguments}", Method = "GET", BodyStyle = WebMessageBodyStyle.Bare)]
        Stream Get(string arguments);      
    }
}
