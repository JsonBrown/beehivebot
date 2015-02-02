using beehive.common.Contracts;
using beehive.common.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace beehive.extensions.Notifications
{
    public class NotificationHost : INotificationHost
    {
        private const string HTML_PATH = @"\html\{0}";
        private const string INDEX_FILE = @"index.html";

        private static Dictionary<string, string> contentTypes = new Dictionary<string, string>
        {
            {".jpg", "image/jpeg"},
            {".jpeg", "image/jpeg"},
            {".png", "image/png"},
            {".gif", "image/gif"},
            {".css", "text/css"},
            {".js", "text/javascript"},
            {".html", "text/html"}
        };

        private readonly IDisk disk;
        public NotificationHost(IDisk disk)
        {
            this.disk = disk;
        }
        public Stream Get(string arguments)
        {
            arguments = String.IsNullOrEmpty(arguments) ? INDEX_FILE : arguments;
            if (arguments.Contains("..") || !disk.Exists(String.Format(HTML_PATH, arguments))) throw new WebFaultException(HttpStatusCode.NotFound);

            UriTemplateMatch uriInfo = WebOperationContext.Current.IncomingRequest.UriTemplateMatch;
            WebOperationContext.Current.OutgoingResponse.ContentType = contentTypes.Get(Path.GetExtension(arguments).ToLower()) ?? "text/html";
            Stream rawResponse = disk.Read(String.Format(HTML_PATH, arguments));
            rawResponse.Position = 0;
            return rawResponse;            
        }
    }
    public class MyInstanceProvider : IInstanceProvider
    {
        Func<NotificationHost> serviceCreator;
        public MyInstanceProvider(Func<NotificationHost> serviceCreator)
        {
            this.serviceCreator = serviceCreator;
        }

        public object GetInstance(InstanceContext instanceContext, Message message)
        {
            return this.serviceCreator();
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            return this.serviceCreator();
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
        }
    }
    public class MyServiceBehavior : IServiceBehavior
    {
        Func<NotificationHost> serviceCreator;
        public MyServiceBehavior(Func<NotificationHost> serviceCreator)
        {
            this.serviceCreator = serviceCreator;
        }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters)
        {
        }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
            foreach (ChannelDispatcher cd in serviceHostBase.ChannelDispatchers)
            {
                foreach (EndpointDispatcher ed in cd.Endpoints)
                {
                    ed.DispatchRuntime.InstanceProvider = new MyInstanceProvider(this.serviceCreator);
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase)
        {
        }
    }
}
