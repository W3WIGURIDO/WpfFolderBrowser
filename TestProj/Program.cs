using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfFolderBrowser;

namespace TestProj
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var folderDialogResult = WpfFolderBrowser.Main.ShowFolderDialog(null, true);
            if (folderDialogResult != null)
            {
                Console.WriteLine(string.Format("Address:{0}\nFolderName:{1}\nFullName:{2}", folderDialogResult.Address, folderDialogResult.FolderName, folderDialogResult.FullName));
            }
            EndCountDown(7000);
            Console.ReadKey();
            //Browser browser = new Browser();
            //browser.ShowDialog();
        }

        static void EndCountDown(int milliSeconds)
        {
            Task delay = new Task(() =>
            {
                Console.WriteLine(milliSeconds / 1000 + "秒経過か任意のキーで終了");
                System.Threading.Thread.Sleep(milliSeconds);
                Environment.Exit(0);
            });
            delay.Start();
        }
    }
}
