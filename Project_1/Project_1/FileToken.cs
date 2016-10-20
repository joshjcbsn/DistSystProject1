using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project_1
{
    class FileToken
    {
        public string name;
        public string text;
        public List<int> queue;
        public int holder;
        public bool utilizing;
        public bool asked;
        public FileToken(string n, int h)
        {
            name = n;
            holder = h;
            text = null;
            queue = new List<int>();
            utilizing = false;
            asked = false;
        }
        /// <summary>
        /// sets the holder
        /// </summary>
        /// <param name="h"></param>
        public void setHolder(int h)
        {
            holder = h;
        }
        /// <summary>
        /// sets using
        /// </summary>
        /// <param name="u"></param>
        public void setUsing(bool u)
        {
            utilizing = u;
        }
        /// <summary>
        /// sets asked
        /// </summary>
        /// <param name="a"></param>
        public void setAsked(bool a)
        {
            asked = a;
        }

        /// <summary>
        /// Prints file info
        /// </summary>
        public void printInfo()
        {
            Console.Write("{0}\tholder={1}\tqueue=[ ", name, holder);
            if (queue.Count > 0)
            {
                foreach (int n in queue)
                {
                    Console.Write(String.Format("{0} ", n));
                }
            }
            Console.WriteLine("]");
        }
    }
}
