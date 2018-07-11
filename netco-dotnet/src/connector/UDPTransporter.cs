using System;
using System.Net.Sockets;

namespace netco {
    public class UDPTransporter : Transporter {
        private Socket socket = null;

        byte[] receiveBuffer = new byte[1024];
        public UDPTransporter(Client client, Socket socket) {
            this.client = client;
            this.socket = socket;
            this.receive();
        }

        public override void Send(byte[] buffer) {
            try {
                if (client.GetNetworkState() != NetworkState.Connected)
                    return;
                socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(sendCallback), socket);
            } catch (Exception e) {
                client.ChangeNetworkState(NetworkState.Error);
                NDebug.Log(e);
            }
        }
        private void sendCallback(IAsyncResult arg) {
            try {
                socket.EndSend(arg);
            } catch (Exception e) {
                NDebug.Log(e);
                client.ChangeNetworkState(NetworkState.Closed);
            }
        }

        private void receive() {
            try {
                socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
            } catch(Exception e) {
                client.ChangeNetworkState(NetworkState.Error);
                NDebug.Log(e);
            }
        }

        private void receiveCallback(IAsyncResult arg) {
            try {
                int len = socket.EndReceive(arg);
                if (len > 0) {
                    // parse
                    var data = new byte[len];
                    Buffer.BlockCopy(receiveBuffer, 0, data, 0, len);
                    protocol.OnReadBytes(data);
                    receive();
                } else {
                    // close conn
                    client.ChangeNetworkState(NetworkState.Error);
                }
            } catch (Exception e) {
                NDebug.Log(e);
                client.ChangeNetworkState(NetworkState.Closed);
            }
        }

    }
}
