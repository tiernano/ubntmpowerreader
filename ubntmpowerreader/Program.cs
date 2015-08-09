using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.Configuration;
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
                string[] items = { "active_pwr", "energy_sum" };

                using (var sshclient = new SshClient(ConnNfo))
                {
                    Console.WriteLine("Connecting to {0}", server);
                    sshclient.Connect();
                    // quick way to use ist, but not best practice - SshCommand is not Disposed, ExitStatus not checked...
                    foreach (string s in items)
                    {
                        for (int i = 1; i < 7; i++)
                        {
                            Console.WriteLine(string.Format("{0}{1}", s, i));
                            Console.WriteLine(sshclient.CreateCommand(string.Format("cd /proc/power && cat {0}{1}", s, i)).Execute());
                        }
                    }
                    sshclient.Disconnect();
                }
            }
            Console.ReadLine();
        }
    }
}
