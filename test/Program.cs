using System;
using System.Threading;
namespace test {
    public class Program {
        public static void Main(string[] args) {
            Console.WriteLine("Hello World!");
            (new TestTCPClient()).start();

            while(true) {
                Thread.Sleep(1000 * 1000);
            }
        }
    }
}
