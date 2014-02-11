using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FourOver
{
    public static class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            FourOverClient fourOverClient = new FourOverClient();
            fourOverClient.Rip("business-cards","BusinessCards");
        }
    }
}
