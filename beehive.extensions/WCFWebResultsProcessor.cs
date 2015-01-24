using beehive.common.Contracts;
using beehive.common.Objects;
using beehive.extensions.Notifications;
using Fleck;
using log4net;
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
        private ILog log = LogManager.GetLogger(typeof(WCFWebResultsProcessor));

        private const string NOTIFICATION_SERVICE_URI = @"http://localhost:8335/";
        private const string SOCKET_SERVICE_URI = @"ws://0.0.0.0:8336/";

        private readonly ServiceEndpoint notificationEndpoint;
        private readonly WebServiceHost notificationHost;
        private readonly WebSocketServer server;
        private readonly List<IWebSocketConnection> allSockets;

        public WCFWebResultsProcessor(IDisk disk)
        {
            Uri notificationAddress = new Uri(NOTIFICATION_SERVICE_URI);
            notificationHost = new WebServiceHost(typeof(NotificationHost));
            notificationHost.Description.Behaviors.Add(new MyServiceBehavior(() => new NotificationHost(disk)));
            notificationEndpoint = notificationHost.AddServiceEndpoint(typeof(INotificationHost), new WebHttpBinding(), notificationAddress);
            notificationEndpoint.Behaviors.Add(new WebHttpBehavior());

            notificationHost.Open();

            server = new WebSocketServer(SOCKET_SERVICE_URI);
            allSockets = new List<IWebSocketConnection>();
            server.Start(socket =>
            {
                socket.OnOpen = () =>
                {
                    log.Debug("Open!");
                    allSockets.Add(socket);
                };
                socket.OnClose = () =>
                {
                    log.Debug("Close!");
                    allSockets.Remove(socket);
                };
                socket.OnMessage = message =>
                {
                    log.Debug(message);
                    allSockets.ToList().ForEach(s => s.Send("Echo: " + message));
                };
            });

        }
        public void Process(CommandResult result)
        {
            allSockets.ForEach(s => s.Send(result.Message));
        }

        public void Dispose()
        {
            if (notificationHost != null) notificationHost.Close();
            if (allSockets.Any()) allSockets.ForEach(s => s.Close());
            if (server != null) server.Dispose(); 
        }
    }
}
