﻿using System;
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
            //Get process number
            Console.WriteLine("Enter Process #");
            int N = Convert.ToInt32(Console.ReadLine());
            //Read info from tcp_config.txt
            Dictionary<int, TCPConfig> tcpConfig = new Dictionary<int, TCPConfig>();
            using (StreamReader tcpReader = new StreamReader("tcp_config.txt"))
            {

                string line;
                char[] comma = { ',' };
                while ((line = tcpReader.ReadLine()) != null)
                {
                    string[] words = line.Split(comma);
                    tcpConfig.Add(Convert.ToInt32(words[0]), new TCPConfig(words[1], words[2], Convert.ToInt32(words[3])));
                }
            }
            Node process = new Node(N, tcpConfig[N]);
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
                        process.addNeighbor(child, tcpConfig[child]);
                    else if (child == N)
                        process.addNeighbor(parent, tcpConfig[parent]);
                }
            }
            process.printNeighbors();
            Task listener = Task.Factory.StartNew(() => process.getConnections());
            while (true)
            {
                string line = Console.ReadLine();
                char[] space = { ' ' };
                string[] words = line.Split(space, 3);
                string command = words[0];
                if (command == "CREATE")
                    process.Create(words[1]);
                else if (command == "DELETE")
                    process.Delete(words[1]);
                else if (command == "APPEND")
                    process.Append(words[1], words[2]);
                else if (command == "READ")
                    process.Read(words[1]);
            }
        }

    }
}
