using InfluxDB.Net;
using InfluxDB.Net.Models;
using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ubntmpowerreader
{
    class Program
    {
        static void Main(string[] args)
        {
            string servers = ConfigurationManager.AppSettings["servers"];
            string username = ConfigurationManager.AppSettings["username"];
            string password = ConfigurationManager.AppSettings["password"];

            InfluxDb db = new InfluxDb("http://192.168.1.36:8086", "root", "root");

            bool cont = true;
            while (cont)
            {
                decimal totalWatts = 0;
                foreach (string server in servers.Split(','))
                {
                    ConnectionInfo ConnNfo = new ConnectionInfo(server, 22, username,
                        new AuthenticationMethod[]{
                        // Pasword based Authentication
                        new PasswordAuthenticationMethod(username,password)
                        }
                        );

                    string[] items = { "active_pwr", "energy_sum", "v_rms", "pf", "enabled" };

                    using (var sshclient = new SshClient(ConnNfo))
                    {
                        Console.WriteLine("Connecting to {0}", server);
                        sshclient.Connect();
                        Dictionary<int, Dictionary<string, string>> values = new Dictionary<int, Dictionary<string, string>>();

                        // quick way to use ist, but not best practice - SshCommand is not Disposed, ExitStatus not checked...
                        foreach (string itemName in items)
                        {
                            for (int port = 1; port < 7; port++)
                            {
                                string outputFileName = string.Format("{0}-{1}-{2}.txt", server, itemName, port);
                               
                                string result = sshclient.CreateCommand(string.Format("cat /proc/power/{0}{1}", itemName, port)).Execute();
                                if(itemName == "active_pwr")
                                {
                                    decimal power = decimal.Parse(result);
                                    totalWatts += power;
                                }

                                if (!values.ContainsKey(port))
                                {
                                    values.Add(port, new Dictionary<string, string>());
                                }

                                values[port].Add(itemName, result);
                            }
                        }

                        foreach (var k in values.Keys)
                        {
                            var data = values[k];

                            Point p = new Point()
                            {
                                Measurement = "power",
                                Tags = new Dictionary<string, object>()
                                {
                                    { "server", server },
                                    { "port", k }
                                },
                                Fields = new Dictionary<string, object>()
                            };

                            foreach (var x in data)
                            {
                                p.Fields.Add(x.Key, x.Value);
                            }

                            var writeResult = db.WriteAsync("mfi", p);
                            Console.WriteLine(writeResult.Result.Success);
                        }
                        sshclient.Disconnect();
                    }
                }


                Point p1 = new Point()
                {
                    Measurement = "power",
                    Tags = new Dictionary<string, object>()
                                {
                                    { "server", "overall" },
                                    { "port", "all" }
                                },
                    Fields = new Dictionary<string, object>()
                };

                
                    p1.Fields.Add("totalwatts", totalWatts);
                

                var writeResult1 = db.WriteAsync("mfi", p1);
                Console.WriteLine(writeResult1.Result.Success);
                Thread.Sleep(60000);
            }
        }
    }
}
