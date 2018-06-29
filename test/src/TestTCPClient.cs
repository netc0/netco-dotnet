using System;
using System.Net;
using System.Net.Sockets;
using netco;
namespace test {
    public class TestTCPClient {
        
        public void start() {
            testUDP();
        }

        void testTCP() {
            Debug.Log("测试 TCP Client");

            TCPClient client = new TCPClient();
            client.OnNetworkStateChanged += (state) => {
                Debug.Log("网络状态改变:" + state);
            };

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            client.Connect(ipAddress, 9000, () => {
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
        void testUDP() {
            Debug.Log("测试 UDP Client");

            UDPClient client = new UDPClient();
            client.OnNetworkStateChanged += (state) => {
                Debug.Log("网络状态改变:" + state);
            };

            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");

            client.Connect(ipAddress, 9001, () => {
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
