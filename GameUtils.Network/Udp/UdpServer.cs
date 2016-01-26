using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Artentus.GameUtils.Network.Udp
{
    public class UdpServer : IServer
    {
        readonly UdpClient client;
        readonly List<IConnection> connections;
        readonly int port;
        bool active;
        readonly Thread listenThread;
        readonly Thread pingThread;

        public UdpServer(int port)
        {
            this.port = port;
            client = new UdpClient(port);
            connections = new List<IConnection>();
            active = true;
            listenThread = new Thread(Listen);
            listenThread.Start();
        }

        private void SendNotification(NotificationMessage message, IConnection[] connections)
        {
            var package = new SystemPackage(connections);
            package.SerializeContent(message);
            this.Send(package, connections);
        }

        private void Listen()
        {
            var endPoint = new IPEndPoint(IPAddress.Any, port);

            while (active)
            {
                if (client.Available > 0)
                {
                    byte[] data = client.Receive(ref endPoint);
                    using (var ms = new MemoryStream(data))
                    {
                        IPackage package = PackageSerializer.DeserializeFrom<IPackage>(ms);

                        SystemPackage systemPackage = package as SystemPackage;
                        if (systemPackage != null)
                        {
                            if (typeof(UdpMessage).IsAssignableFrom(systemPackage.ContentType))
                            {
                                UdpMessage udpMessage = systemPackage.DeserializeContent<UdpMessage>();

                                if (udpMessage.IsConnectMessage)
                                {
                                    this.SendNotification(new NotificationMessage(NotificationType.ClientConnect), connections.ToArray());

                                    this.SendNotification(new NotificationMessage(NotificationType.ClientList), connections.ToArray());
                                }
                                else
                                {
                                    
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Send<T>(T package) where T : IPackage
        {
            
        }

        public void Send<T>(T package, IConnection[] connections) where T : IPackage
        {

        }

        public void Shutdown()
        {
            active = false;
            listenThread.Join();
            pingThread.Join();
        }
    }
}
