using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1
{
 
    public struct TCPConfig
    {
        public string dns;
        public string ip;
        public int port;
        public TCPConfig(string DNS, string IP, int P)
        {
            dns = DNS;
            ip = IP;
            port = P;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            int N = Convert.ToInt32(args[1]);
            Dictionary<int, TCPConfig> tcpConfig = new Dictionary<int, TCPConfig>();
            using (StreamReader dnsReader = new StreamReader("tcp_config.txt"))
            {

                string line;
                char[] comma = { ',' };
                while ((line = dnsReader.ReadLine()) != null)
                {
                    string[] words = line.Split(comma);
                    tcpConfig.Add(Convert.ToInt32(words[0]), new TCPConfig(words[1], words[2], Convert.ToInt32(words[3])));
                }
            }
            using (StreamReader treeReader = new StreamReader("tree.txt"))
            {
                string line;
                char[] parens = { '(', ')' };
                char[] comma = { ',' };
                while ((line = treeReader.ReadLine()) != null)
                {
                    line = line.Trim(parens);
                    string[] pair = line.Split(comma);
                    int parent = Convert.ToInt32(pair[0]);
                    int child = Convert.ToInt32(pair[1]);
                    if (parent == N)
                    {

                    }
                }
            }
            while (true)
            {
                string line = Console.ReadLine();
                char[] space = { ' ' };
                string[] words = line.Split(space);
                string command = words[0];
              //  if (command == "create")


            }
        }

    }
}
