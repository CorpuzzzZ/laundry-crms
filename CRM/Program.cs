using System;
using System.Windows.Forms;
using CRM.WinForms.Forms;

namespace CRM.WinForms
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }
    }
}