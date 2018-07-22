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

        public override void Send(byte[] buf) {
            try {
                if (client.GetNetworkState() != NetworkState.Connected)
                    return;
                socket.BeginSend(buf, 0, buf.Length, SocketFlags.None, new AsyncCallback(sendCallback), socket);
            } catch (Exception e) {
                client.LastError = e.ToString();
                client.ChangeNetworkState(NetworkState.Error);
            }
        }
        private void sendCallback(IAsyncResult arg) {
            try {
                socket.EndSend(arg);
            } catch (Exception e) {
                client.LastError = e.ToString();
                client.ChangeNetworkState(NetworkState.Error);
            }
        }

        private void receive() {
            try {
                if (client.GetNetworkState() != NetworkState.Connected) return;
                socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
            } catch(Exception e) {
                client.LastError = e.ToString();
                client.ChangeNetworkState(NetworkState.Error);
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
                    if (this.client.GetNetworkState() == NetworkState.Connected)
                        receive();
                } else {
                    // close conn
                    if (len == 0) {
                        client.Close();
                    } else {
                        client.ChangeNetworkState(NetworkState.Error);
                    }
                }
            } catch (Exception e) {
                client.LastError = e.ToString();
                client.ChangeNetworkState(NetworkState.Error);
            }
        }

    }
}
