using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Win32;
using IniParser;
using IniParser.Model;
using System.Windows.Forms;

class A
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool B(IntPtr hWnd, string lpString);

    private static async Task<string> C(string d)
    {
        using (HttpClient e = new HttpClient())
        {
            try
            {
                HttpResponseMessage f = await e.GetAsync(d);
                if (f.IsSuccessStatusCode)
                {
                    return await f.Content.ReadAsStringAsync();
                }
            }
            catch
            {
                // Handle errors
            }
        }
        return null;
    }

    private static async Task<bool> G(string h)
    {
        using (HttpClient i = new HttpClient())
        {
            try
            {
                HttpResponseMessage j = await i.GetAsync(h);
                return j.IsSuccessStatusCode;
            }
            catch
            {
                // Handle errors
                return false;
            }
        }
    }

    [STAThread]
    static async Task Main()
    {
        string k = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
        string l = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "version.txt");

        var m = new FileIniDataParser();
        IniData n = m.ReadFile(k);

        string o = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');

        string p = Environment.Is64BitOperatingSystem ? n["Registry"]["64bits"] : n["Registry"]["32bits"];
        string q = n["Registry"]["FolderKey"];

        string r = Path.Combine(o, n["Main"]["ExeName"]);
        string s = n["Main"]["IpAddress"];
        string t = n["Main"]["Port"];
        string u = n["Main"]["UpdateURL"];

        bool v = await G(u);
        if (!v)
        {
            MessageBox.Show("The version file is not available. Please try again later or contact support.", "Version File Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            Environment.Exit(0);
        }

        string w = File.Exists(l) ? File.ReadAllText(l).Trim() : null;
        string x = await C(u);

        if (w != x)
        {
            string y = Path.Combine(o, n["Main"]["Updater"]);
            if (File.Exists(y))
            {
                Process.Start(y);
            }
            else
            {
                MessageBox.Show($"Updater file not found. Please ensure that '{y}' is present in the application directory.", "Updater Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            Environment.Exit(0);
        }

        try
        {
            using (RegistryKey z = Registry.LocalMachine.CreateSubKey(p))
            {
                z.SetValue(q, o, RegistryValueKind.String);
            }
        }
        catch
        {
            // Silently handle errors
        }

        Process aa = Process.Start(r, $"{s} {t}");

        if (aa != null)
        {
            System.Threading.Thread.Sleep(5000);

            IntPtr ab = aa.MainWindowHandle;
            if (ab != IntPtr.Zero)
            {
                B(ab, "Login");
            }
        }

        Environment.Exit(0);
    }
}
