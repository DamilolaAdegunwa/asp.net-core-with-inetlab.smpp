using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Inetlab.SMPP;
using Inetlab.SMPP.Common;
using Inetlab.SMPP.Logging;

namespace TestLocalPerformance
{
    class Program
    {
        static void Main(string[] args)
        {

            LogManager.SetLoggerFactory(new ConsoleLogFactory(LogLevel.Info));

            StartApp().ConfigureAwait(false);
            Console.WriteLine("done!!");
            Console.ReadLine();
        }

        public static async Task StartApp()
        {

            using (SmppServer server = new SmppServer(new IPEndPoint(IPAddress.Any, 7777)))
            {
                server.evClientBind += (sender, client, data) => { /*accept all*/ };
                server.evClientSubmitSm += (sender, client, data) => {/*receive all*/ };
                server.Start();

                using (SmppClient client = new SmppClient())
                {
                    await client.Connect("localhost", 7777);

                    await client.Bind("username", "password");

                    Console.WriteLine("Performance: " + await RunTest(client, 50000) + " m/s");
                }
            }
        }

        public static async Task<int> RunTest(SmppClient client, int messagesNumber)
        {

            List<Task> tasks = new List<Task>();

            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < messagesNumber; i++)
            {
                tasks.Add(client.Submit(
                    SMS.ForSubmit()
                        .From("111")
                        .To("222")
                        .Coding(DataCodings.UCS2)
                        .Text("test")));
            }

            await Task.WhenAll(tasks);

            watch.Stop();

            return Convert.ToInt32(messagesNumber / watch.Elapsed.TotalSeconds);

        }
    }
}