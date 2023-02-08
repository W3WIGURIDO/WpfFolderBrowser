using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfFolderBrowser
{
    public class Main
    {

        public static FolderDialogResult ShowFolderDialog()
        {
            return ShowFolderDialog(null, false);
        }

        public static FolderDialogResult ShowFolderDialog(Window owner, bool accessForbiddenVisibility, string baseAddress = null)
        {
            Browser browser = new Browser()
            {
                AccessForbiddenVisibility = accessForbiddenVisibility
            };
            if (baseAddress != null && System.IO.Directory.Exists(baseAddress))
            {
                browser.Address = baseAddress;
            }
            if (owner != null)
            {
                browser.Owner = owner;
            }
            if (browser.ShowDialog() == true)
            {
                return new FolderDialogResult(browser.FolderName, browser.Address, browser.FullName);
            }
            else
            {
                return null;
            }
        }
    }

    public class FolderDialogResult
    {
        public string FolderName { get; }
        public string Address { get; }
        public string FullName { get; }

        public FolderDialogResult(string folderName, string address, string fullName)
        {
            FolderName = folderName;
            Address = address;
            FullName = fullName;
        }
    }
}
