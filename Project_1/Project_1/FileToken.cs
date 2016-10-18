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

        public void setHolder(int h)
        {
            holder = h;
        }
        public void setUsing(bool u)
        {
            utilizing = u;
        }
        public void setAsked(bool a)
        {
            asked = a;
        }

        public void printInfo()
        {
            Console.WriteLine("{0}\t{1}", name, holder);
        }
    }
}
