using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ModAssistant
{
    class Update
    {
        private static string ExePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;

        public static bool NeedsUpdate()
        {


            return false;
        }

        public static void Run()
        {
            //MessageBox.Show(Update.ExePath);
        }
    }
}
