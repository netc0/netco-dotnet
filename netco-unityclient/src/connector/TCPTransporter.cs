using System;
using System.Net.Sockets;

namespace netco {
    public class TCPTransporter : Transporter {
        private Socket socket = null;

        byte[] receiveBuffer = new byte[1024];
        public TCPTransporter(Client client, Socket socket) {
            this.client = client;
            this.socket = socket;
            this.receive();
        }

        public override void Send(byte[] buffer) {
            socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(sendCallback), socket);
        }
        private void sendCallback(IAsyncResult arg) {
            try {
                socket.EndSend(arg);
            } catch (Exception e) {
                Console.WriteLine(e);
                client.ChangeNetworkState(NetworkState.Closed);
            }
        }

        private void receive() {
            socket.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(receiveCallback), socket);
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
                Console.WriteLine(e);
                client.ChangeNetworkState(NetworkState.Closed);
            } 
        }

    }
}
