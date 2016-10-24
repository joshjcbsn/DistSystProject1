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
        public TCPConfig tcp; //tcp configuration of this node
        public TcpListener listener; //tcp listener for this node
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
            //set TCPConfig
            tcp = TCP;
            //start listener        
            try
            {
                listener = new TcpListener(IPAddress.Any, tcp.port);
                listener.Start();
            }
            catch (Exception ex) { Console.WriteLine(String.Format("error: {0}", ex.Message)); }
        }

        /// <summary>
        /// Listens for tcp connections, using a recursive method of accepting connections
        /// </summary>
        public void getConnections()
        {

            try
            {
                //AcceptTcpClient blocks this thread until it recieves a connection
                Console.WriteLine("Waiting for connection");
                using (TcpClient client = listener.AcceptTcpClient())
                {
                    //start new instance to accept next connection
                    Task newConnection = Task.Factory.StartNew(() => getConnections());
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
            catch (Exception ex) { Console.WriteLine(ex.Message); }            
                      
        }
        
        /// <summary>
        /// handles messages recieved from neighbors
        /// </summary>
        /// <param name="msg"></param>
        private void msgHandler(string msg)
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
                Created(args[1], Convert.ToInt32(args[2]));
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

        private void sendMsg(int P, string msg)
        {
            
            try
            {
                string host = neighbors[P].dns;
                IPAddress ip = IPAddress.Parse(neighbors[P].ip);
                int portNum = neighbors[P].port;

                Console.WriteLine("Sending to {0}: {1}", P, msg);
                using (TcpClient client = new TcpClient(host, portNum))
                {
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

        /// <summary>
        /// Initializes request for filename if it hasn't already been requested
        /// </summary>
        /// <param name="filename"></param>
        private void Req(string filename)
        {
            if (!(files[filename].asked))
            {
                files[filename].queue.Add(n);
                if (files[filename].queue[0] == n)
                {

                    sendMsg(files[filename].holder, String.Format("REQ {0} {1}", filename, n));
                    files[filename].asked = true;
                }
                files[filename].printInfo();
            }
        }

        /// <summary>
        /// Forwards request for filename from process p, or replies with privilege
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="p"></param>
        private void Req(string filename, int p)
        {
            if (!(files[filename].asked))
            {
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
                files[filename].printInfo();
            }

        }

        /// <summary>
        /// Sends file token for filename to the holder, or privileges current process if it is the holder
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="text"></param>
        private void Privilege(string filename, string text)
        {
            files[filename].text = text;
            files[filename].setAsked(false);
            files[filename].setHolder(files[filename].queue[0]);
            files[filename].queue.RemoveAt(0);
            if (files[filename].holder == n)
                files[filename].setUsing(true);
            else
                sendMsg(files[filename].holder, String.Format("PRIVILEGE {0} {1}", filename, text));
            files[filename].printInfo();
        }

        /// <summary>
        /// Releases use of filename, sends privilege to first process in queue
        /// </summary>
        /// <param name="filename"></param>
        private void Release(string filename)
        {
            Console.WriteLine("P{0} released {1}", n, filename);
            files[filename].setUsing(false);
            files[filename].setAsked(false);
            if ((files[filename].queue.Count) > 0)
                sendMsg(files[filename].queue[0], String.Format("PRIVILEGE {0} {1}", filename, files[filename].text));
            files[filename].printInfo();
        }

        /// <summary>
        /// Create file "filename" and alert neighbors that it has been created
        /// </summary>
        /// <param name="filename">File name.</param>
        public void Create(string filename)
        {
            FileToken file = new FileToken(filename, n);
            files.Add(filename, file);
            foreach (int P in neighbors.Keys)
                sendMsg(P, String.Format("CREATE {0} {1}", filename, n));
            printFiles();
        }

        /// <summary>
        /// Add filename with holder h to dictionary of file tokens and alert other neighbors that it has been created
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="h"></param>
        private void Created(string filename, int h)
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
                printFiles();
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
            printFiles();
        }
        /// <summary>
        /// Removes filetoken for filename from dictionary
        /// </summary>
        /// <param name="filename"></param>
        private void Deleted(string filename)
        {
            if (files.Remove(filename))
            {
                foreach (int P in neighbors.Keys)
                    sendMsg(P, String.Format("DELETED {0}", filename));
                printFiles();
            }             
        }

        /// <summary>
        /// Read the specified fileName.
        /// </summary>
        /// <param name="fileName">File name.</param>
        public void Read(string filename)
        {
            if (files.ContainsKey(filename))
            {
                if ((files[filename].holder != n) ||
              (files[filename].utilizing))
                {
                    Func<bool> hasToken = delegate () { return files[filename].utilizing; };
                    this.Req(filename);
                    SpinWait.SpinUntil(hasToken);
                }
                Console.WriteLine();
                Console.WriteLine(files[filename].text);
                Console.WriteLine();
                this.Release(filename);
            }
           
        }
        /// <summary>
        /// Append line to specified file
        /// </summary>
        /// <param name="filename">File name.</param>
        /// <param name="line">Line.</param>
        public void Append(string filename, string line)
        {
            if (files.ContainsKey(filename))
            {
                if ((files[filename].holder != n) ||
                    (files[filename].utilizing))
                {
                    Func<bool> hasToken = delegate () { return files[filename].utilizing; };
                    this.Req(filename);
                    SpinWait.SpinUntil(hasToken);
                }
                files[filename].text += line;
                Console.WriteLine(String.Format("Appended '{0}' to {1}", line, filename));
                this.Release(filename);
            }
        }

        /// <summary>
        /// Prints files stored in memory
        /// </summary>
        public void printFiles()
        {
            Console.WriteLine("Files:");
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
