using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace netco {
    public class TCPClient : Client{
        
        private Socket socket;
        private ManualResetEvent timeoutEvent = new ManualResetEvent(false);
        private int timeoutMSec = 8000;

        private Protocol protocol = null;
        private TCPTransporter transporter = null;

        public void Connect(IPAddress ipAddress, int port, Action onFinishedCallback = null) {
            ThreadPool.QueueUserWorkItem(state => {
                timeoutEvent.Reset();

                ChangeNetworkState(NetworkState.Connecting);
                socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                socket.SendTimeout = timeoutMSec;
                var ie = new IPEndPoint(ipAddress, port);
                socket.BeginConnect(ie, new AsyncCallback((result)=>{
                    try {
                        socket.EndConnect(result);
                        ChangeNetworkState(NetworkState.Connected);

                        // conn ok
                        transporter = new TCPTransporter(this, socket);
                        protocol = new Protocol(this, transporter);
                        transporter.SetProtocol(protocol);

                        onFinishedCallback?.Invoke();
                    } catch (Exception e) {
                        if (networkState != NetworkState.Timeout) {
                            ChangeNetworkState(NetworkState.Error);
                        }
                        Console.WriteLine(e);
                    } finally {
                        timeoutEvent.Set();
                    }
                }), socket);

                if (timeoutEvent.WaitOne(timeoutMSec, false)) {
                    if (networkState != NetworkState.Connected && networkState != NetworkState.Error) {
                        ChangeNetworkState(NetworkState.Timeout);
                    }
                }
            });
        }

        public void Request(string route, byte[] data, Action<byte[]> callback = null) {
            if (protocol == null) { return; }
            protocol.Request(route, data, callback);
        }

        public void OnPushData(byte[] data) {
            Console.WriteLine("收到:{0}", data.BytesToString());
        }
    }
}
