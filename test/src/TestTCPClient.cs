using System;
using System.Net;
using System.Net.Sockets;
using netco;
namespace test {
    public class TestTCPClient {
        TCPClient client;
        public void start() {
            Debug.Log("测试 TCP Client");

            client = new TCPClient();
            client.OnNetworkStateChanged += (state) => {
                Debug.Log("网络状态改变:" + state);
            };

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            client.Connect(ipAddress, 9000, ()=> {
                Debug.Log("连接成功");

                client.Request("Example.Test", "hello".StringToBytes(), (r) => {
                    var msg = r.BytesToString();
                    Console.WriteLine("收到:{0}", msg);
                });

                client.Request("Example.Login", "login".StringToBytes(), (r) => {
                    var msg = r.BytesToString();
                    Console.WriteLine("收到:{0}", msg);
                });

            });


        }
    }
}
