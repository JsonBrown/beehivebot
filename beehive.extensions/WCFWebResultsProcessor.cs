using beehive.common.Contracts;
using beehive.common.Objects;
using beehive.extensions.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace beehive.extensions
{
    public class WCFWebResultsProcessor : IResultProcessor
    {
        private const string SERVICE_URI = @"http://localhost:8335/";
        private readonly ServiceEndpoint svcEndpoint;
        private readonly WebServiceHost svcHost;
        public WCFWebResultsProcessor(IDisk disk)
        {
            Uri baseAddress = new Uri(SERVICE_URI);
            svcHost = new WebServiceHost(typeof(NotificationHost));
            svcHost.Description.Behaviors.Add(new MyServiceBehavior(() => new NotificationHost(disk)));
            svcEndpoint = svcHost.AddServiceEndpoint(typeof(INotificationHost),new WebHttpBinding(), baseAddress);
            svcEndpoint.Behaviors.Add(new WebHttpBehavior());

            svcHost.Open();
        }
        public void Process(CommandResult result)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            if (svcHost != null) svcHost.Close();
        }
    }
}
