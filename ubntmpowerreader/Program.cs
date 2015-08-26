using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
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
            foreach (string server in servers.Split(','))
            {
                ConnectionInfo ConnNfo = new ConnectionInfo(server, 22, username,
                    new AuthenticationMethod[]{
                        // Pasword based Authentication
                        new PasswordAuthenticationMethod(username,password)
                    }
                    );

                string[] items = { "active_pwr", "energy_sum", "v_rms", "pf","enabled" };

                using (var sshclient = new SshClient(ConnNfo))
                {
                    Console.WriteLine("Connecting to {0}", server);
                    sshclient.Connect();
                    // quick way to use ist, but not best practice - SshCommand is not Disposed, ExitStatus not checked...
                    foreach (string itemName in items)
                    {
                        for (int port = 1; port < 7; port++)
                        {
                            string outputFileName = string.Format("{0}-{1}-{2}.txt", server, itemName, port);
                            Console.WriteLine(string.Format("{0}{1}", itemName, port));
                            string result = sshclient.CreateCommand(string.Format("cat /proc/power/{0}{1}", itemName, port)).Execute();
                            if (!File.Exists(outputFileName))
                            {
                                using (var writer = File.CreateText(outputFileName))
                                {
                                    writer.WriteLine(string.Format("{0}, {1}", DateTime.Now.Ticks, result));
                                }
                            }
                            else
                            {
                                using (var writer = File.AppendText(outputFileName))
                                {
                                    writer.WriteLine(string.Format("{0}, {1}", DateTime.Now.Ticks, result));
                                }
                            }
                            Console.WriteLine(result);
                        }
                    }
                    sshclient.Disconnect();
                }
            }
        }
    }
}
