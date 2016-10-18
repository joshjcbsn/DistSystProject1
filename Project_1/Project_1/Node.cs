using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_1
{

    class Node
    {
        public int n; //process number
        //public Node parent;
        //public List<Node> children = new List<Node>();
        public Dictionary<int, Node> neighbors = new Dictionary<int, Node>(); //neighboring nodes
        public Dictionary<string, FileToken> files = new Dictionary<string, FileToken>(); //info on files

        /// <summary>
        /// Initializes new process
        /// </summary>
        /// <param name="N">process number</param>
        /// <param name="P">parent process, null if top of tree</param>
        public Node(int N, Node P)
        {
            //set process number
            n = N;
            //  parent = P;
            //add parent node to neighbors
            if (P != null)
            {

                neighbors.Add(P.getN(), P);
            }
         
        }

        public int getN()
        {
            return n;
        }

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <returns>The child.</returns>
        /// <param name="C">C.</param>
        public void addChild(Node C)
        {
            neighbors.Add(C.getN(), C);
        }

        public void Req(string filename)
        {
            if (!(files[filename].asked))
            {
                Console.WriteLine("P{0} Req({1})", n, filename);
                files[filename].queue.Add(n);
                if (files[filename].queue[0] == n)
                {

                    neighbors[files[filename].holder].Req(filename, n);
                    files[filename].asked = true;
                }
            }
            else
            {
                Console.WriteLine("asked");
            }

        }

        public void Req(string filename, int p)
        {
            if (!(files[filename].asked))
            {
                Console.WriteLine("P{0} Req({1})", n, filename);

                files[filename].queue.Add(p);
                if (files[filename].queue[0] == p)
                {
                    if ((files[filename].holder == n) &&
                        (!(files[filename].utilizing)))
                    {
                        this.Privilege(filename);
                    }
                    else
                    {
                        neighbors[files[filename].holder].Req(filename, n);
                        files[filename].asked = true;
                    }

                }
            }
            else
            {
                Console.WriteLine("asked");
            }

        }

        public void Privilege(string filename)
        {
            files[filename].setAsked(false);
            files[filename].setHolder(files[filename].queue[0]);
            files[filename].queue.RemoveAt(0);
            if (files[filename].holder == n)
            {
                Console.WriteLine("P{0} privileged", n);
                files[filename].setUsing(true);
            }
            else
            {
                Console.WriteLine("P{0} Privilege({1}, {2})", n, filename, files[filename].holder);

                neighbors[files[filename].holder].Privilege(filename);
            }


        }
        public void Release(string filename)
        {
            Console.WriteLine("P{0} released {1}", n, filename);
            files[filename].setUsing(false);
            files[filename].setAsked(false);
            if ((files[filename].queue.Count) > 0)
            {
                neighbors[files[filename].queue[0]].Privilege(filename);
            }
        }

        /// <summary>
        /// Create file "fileName".
        /// </summary>
        /// <param name="filename">File name.</param>
        public void Create(string filename)
        {
            FileStream fs = File.Create(filename);
            FileToken file = new FileToken(filename, n);
            files.Add(filename, file);
            fs.Close();
            foreach (Node N in neighbors.Values)
            {
                N.Create(filename, n);
            }
        }
        public void Create(string filename, int h)
        {
            if (!(files.ContainsKey(filename)))
            {
                FileToken file = new FileToken(filename, h);
                files.Add(filename, file);
                foreach (Node N in neighbors.Values)
                {
                    N.Create(filename, n);
                }
            }
        }

        /// <summary>
        /// Deletes the specified file if it exists, and tells neighbors to delete it
        /// </summary>
        /// <param name="filename">File name.</param>
        public void Delete(string filename)
        {
            if ((files[filename].holder != n) ||
                (files[filename].utilizing))
            {
                Func<bool> hasToken = delegate () { return files[filename].utilizing; };
                this.Req(filename);
                SpinWait.SpinUntil(hasToken);
            }

            File.Delete(filename);
            files.Remove(filename);
            foreach (Node N in neighbors.Values)
                N.Deleted(filename);
        }
        public void Deleted(string filename)
        {
            if (files.Remove(filename))
                foreach (Node N in neighbors.Values)
                    N.Deleted(filename);
        }

        /// <summary>
        /// Read the specified fileName.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void Read(string filename)
        {
            if ((files[filename].holder != n) ||
               (files[filename].utilizing))
            {
                Func<bool> hasToken = delegate () { return files[filename].utilizing; };
                this.Req(filename);
                SpinWait.SpinUntil(hasToken);
            }
            using (StreamReader fReader = new StreamReader(filename))
            {
                string line;
                while ((line = fReader.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
            }
            this.Release(filename);
        }
        /// <summary>
        /// Append line to specified file
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <param name="line">Line.</param>
        public void Append(string filename, string line)
        {
            if ((files[filename].holder != n) ||
              (files[filename].utilizing))
            {
                Func<bool> hasToken = delegate () { return files[filename].utilizing; };
                this.Req(filename);
                SpinWait.SpinUntil(hasToken);
            }
            using (StreamWriter fWriter = new StreamWriter(filename))
            {
                fWriter.WriteLine(line);
            }
            this.Release(filename);
        }

        public void printFiles()
        {
            Console.WriteLine("Process {0}", n);
            foreach (FileToken file in files.Values)
            {
                file.printInfo();
            }
        }

        public void printNeighbors()
        {
            Console.Write("P{0}: ", n);
            foreach (int p in neighbors.Keys)
            {
                Console.Write("{0} ", p);
            }
            Console.WriteLine();
        }
    }
}
