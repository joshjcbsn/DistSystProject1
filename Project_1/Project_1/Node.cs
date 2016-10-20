using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Project_1
{


    class Node
    {
        public int n; //process number
        public TCPConfig tcp;
        public TcpListener listener;
        //public Node parent;
        //public List<Node> children = new List<Node>();
        public Dictionary<int, TCPConfig> neighbors = new Dictionary<int, TCPConfig>(); //neighboring nodes
        public Dictionary<string, FileToken> files = new Dictionary<string, FileToken>(); //info on files

        

        
        /// <summary>
        /// Initializes new process
        /// </summary>
        /// <param name="N">process number</param>
        /// <param name="P">port</param>
        public Node(int N, TCPConfig TCP)
        {
            //set process number
            n = N;
            tcp = TCP;
            
            try
            {
                listener = new TcpListener(IPAddress.Any, tcp.port);
                listener.Start();

            }
            catch (Exception ex) { Console.WriteLine(String.Format("error: {0}", ex.Message)); }

        }

        public void getConnections()
        {

            
            while (true)
            {
                try
                {
                    //Console.WriteLine("Waiting for connection");

                    using (TcpClient client = listener.AcceptTcpClient())
                    {
                        byte[] bytes = new byte[1024];
                        string data = null;
                        Console.WriteLine("Connected");
                        NetworkStream stream = client.GetStream();
                        int i;
                        // Loop to receive all the data sent by the client.
                        i = stream.Read(bytes, 0, bytes.Length);
                        while (i != 0)
                        {
                            // Translate data bytes to a ASCII string.
                            data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            Console.WriteLine(String.Format("Received: {0}", data));
                            // Process the data sent by the client.

                            i = stream.Read(bytes, 0, bytes.Length);

                        }
                        this.msgHandler(data);
                        // Shutdown and end connection
                    }
                }
                catch (Exception ex) { }// Console.WriteLine(ex.Message); }
                
            }
           
        }
        
        /// <summary>
        /// handles messages recieved from neighbors
        /// </summary>
        /// <param name="msg"></param>
        public void msgHandler(string msg)
        {
            char[] space = { ' ' };
            var args = msg.Split(space,3);
            if (args[0] == "REQ")
            {
                Req(args[1], Convert.ToInt32(args[2]));
            }
            else if (args[0] == "PRIVILEGE")
            {
                Privilege(args[1], args[2]);
            }
            else if (args[0] == "CREATE")
            {
                Create(args[1], Convert.ToInt32(args[2]));
            }
            else if (args[0] == "DELETE")
            {
                Delete(args[1]);
            }
            else if (args[0] == "DELETED")
            {
                Deleted(args[1]);
            }
        }

        public void sendMsg(int P, string msg)
        {
            
            try
            {
                string host = neighbors[P].dns;
                IPAddress ip = IPAddress.Parse(neighbors[P].ip);
                int portNum = neighbors[P].port;

                Console.WriteLine("Sending '{0}' to {1} on port {2}", msg, host, portNum);
                using (TcpClient client = new TcpClient(host, portNum))
                {
                   // client.Connect(ip, portNum);
                    try
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] msgBytes = Encoding.ASCII.GetBytes(msg);
                            stream.Write(msgBytes, 0, msgBytes.Length);
                        }
                    }
                    catch (Exception ex) { Console.WriteLine(ex.Message); }

                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
        }


        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <returns>The child.</returns>
        /// <param name="C">C.</param>
        public void addNeighbor(int N, TCPConfig TCP)
        {
            neighbors.Add(N, TCP);
        }

        public void Req(string filename)
        {
            if (!(files[filename].asked))
            {
                Console.WriteLine("P{0} Req({1})", n, filename);
                files[filename].queue.Add(n);
                if (files[filename].queue[0] == n)
                {

                    sendMsg(files[filename].holder, String.Format("REQ {0} {1}", filename, n));
                    files[filename].asked = true;
                }
            }
            else
            {
                Console.WriteLine("asked");
            }

        }

        private void Req(string filename, int p)
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
                        this.Privilege(filename, files[filename].text);
                    }
                    else
                    {
                        sendMsg(files[filename].holder, String.Format("REQ {0} {1}", filename, n));
                        files[filename].asked = true;
                    }

                }
            }
            else
            {
                Console.WriteLine("asked");
            }

        }

        public void Privilege(string filename, string text)
        {
            files[filename].text = text;
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
                sendMsg(files[filename].holder, String.Format("PRIVILEGE {0} {1}", filename, text));
            }


        }
        public void Release(string filename)
        {
            Console.WriteLine("P{0} released {1}", n, filename);
            files[filename].setUsing(false);
            files[filename].setAsked(false);
            if ((files[filename].queue.Count) > 0)
            {
                sendMsg(files[filename].queue[0], String.Format("PRIVILEGE {0} {1}", filename, files[filename].text));

            }
        }

        /// <summary>
        /// Create file "fileName".
        /// </summary>
        /// <param name="filename">File name.</param>
        public void Create(string filename)
        {
            FileToken file = new FileToken(filename, n);
            files.Add(filename, file);
            foreach (int P in neighbors.Keys)
            {
                sendMsg(P, String.Format("CREATE {0} {1}", filename, n));

            }
        }
        private void Create(string filename, int h)
        {
            if (!(files.ContainsKey(filename)))
            {
                FileToken file = new FileToken(filename, h);
                files.Add(filename, file);
                foreach (int P in neighbors.Keys)
                {
                    if (P != h)
                    {
                        sendMsg(P, String.Format("CREATE {0} {1}", filename, n));
                    }
                   
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

            files.Remove(filename);
            foreach (int P in neighbors.Keys)
                sendMsg(P, String.Format("DELETED {0}", filename));
        }
        private void Deleted(string filename)
        {
            if (files.Remove(filename))
                foreach (int P in neighbors.Keys)
                    sendMsg(P, String.Format("DELETED {0}", filename));
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
            Console.WriteLine(files[filename].text);
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
            files[filename].text += line;
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
