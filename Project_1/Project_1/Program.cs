using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1
{
 
    class Program
    {
        static void Main(string[] args)
        {
            Node P = new Node()
            while (true)
            {
                string line = Console.ReadLine();
                char[] space = { ' ' };
                string[] words = line.Split(space);
                string command = words[0];
                if (command == "create")


            }
           
            /*
            Tree tree = new Tree(new List<string>());
            tree.printTree();
            tree.Nodes[1].Create("test.txt");
            tree.printNodes();
            tree.Nodes[5].Append("test.txt", "test line");
            tree.printNodes();
            tree.Nodes[6].Read("test.txt");
            tree.printNodes();
            tree.Nodes[3].Delete("test.txt");
            tree.printNodes();
            System.Console.Read();
            */
        }

    }
}
