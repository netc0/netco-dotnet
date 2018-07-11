using System;
using System.Net;
using System.Net.Sockets;
using netco;
namespace test {
    public class TestTCPClient {
        
        public void start() {

            //netco.NDebug.LogPrinter += (obj) => {
            //    Console.WriteLine(obj);
            //};
            testTCP();
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

                client.Request("game.join", "helloTCP".StringToBytes(), (r) => {
                    var msg = r.BytesToString();
                    Console.WriteLine("收到:{0}", msg);
                });

                client.Request("game.login", "login".StringToBytes(), (r) => {
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

                client.Request("game.join", "helloUDP".StringToBytes(), (r) => {
                    var msg = r.BytesToString();
                    Console.WriteLine("收到:{0}", msg);
                });

                client.Request("game.login", "login".StringToBytes(), (r) => {
                    var msg = r.BytesToString();
                    Console.WriteLine("收到:{0}", msg);
                });
                int eid = 0;
                eid = client.AddEvent("game.OnPush", (r) => {
                    var msg = r.BytesToString();
                    if (msg == "推送消息 3") {
                        client.RemoveEvent(eid);
                    }
                    Console.WriteLine("收到推送的数据:{0}", msg);
                });
            });


        }
    }
}
