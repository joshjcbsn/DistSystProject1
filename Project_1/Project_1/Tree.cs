using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1
{
    class Tree
    {
        public Dictionary<int, Node> Nodes = new Dictionary<int, Node>();
        public Tree(List<string> filenames)
        {
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
                    if (Nodes.ContainsKey(parent))
                    {
                        Node childNode = new Node(child, Nodes[parent], filenames);
                        Nodes[parent].addChild(childNode);
                        Nodes[child] = childNode;
                    }
                    else
                    {
                        Node parentNode = new Node(parent, null, filenames);
                        Node childNode = new Node(child, parentNode, filenames);
                        parentNode.addChild(childNode);
                        Nodes[parent] = parentNode;
                        Nodes[child] = childNode;
                    }
                }
            }
        }
        public void printNodes()
        {
            foreach (Node N in Nodes.Values)
            {
                N.printFiles();
            }
        }
        
        public void printTree()
        {
            foreach(Node node in Nodes.Values)
            {
                node.printNeighbors();
            }
        }

    }
}
