﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TaskDialogInterop;
using tman0.Launcher.UI;
using tman0.Launcher.Utilities;

namespace tman0.Launcher.Minecraft
{
    public static class Launcher
    {
        public static async Task LaunchSequence(this MainWindow window,string username, string password)
        {
            window.LoadingBox.Visibility = Visibility.Visible;
            window.LoadingText.Content = "Logging in...";
            string[] AuthenticationString = await AuthenticatePlayer(username, password);
            MessageBox.Show(AuthenticationString.ToString());
            if (!(AuthenticationString.Length >= 3))
            {
                TaskDialogOptions o = new TaskDialogOptions();
                o.Title = "Unexpected Login Server Response";
                o.Owner = window;
                o.MainInstruction = "A login error has occurred.";
                o.Content = String.Join(":", AuthenticationString);
                o.MainIcon = VistaTaskDialogIcon.Warning;

                TaskDialog.Show(o);
                //MessageBox.Show(AuthenticationString[0], "Unexpected Login Server Response", MessageBoxButton.OK);
                window.LoadingBox.Visibility = Visibility.Collapsed;
                return;
            }
            await window.BeginDownloadMinecraft();
            await Run(window, AuthenticationString[2], AuthenticationString[3]);
        }

        public static async Task<Process> Run(MainWindow window, string Username, string AuthCode)
        {
            Process p = new Process();
            ProcessStartInfo i = new ProcessStartInfo();
            i.FileName = /*LauncherSettings.Default.JavaLocation;*/ @"C:\Program Files\Java\jre7\bin\java.exe";
            i.Arguments += "-cp \"" + Globals.LauncherDataPath + @"\Minecraft\bin\LWJGL\jinput.jar;" + Globals.LauncherDataPath + @"\Minecraft\bin\LWJGL\lwjgl.jar;" + Globals.LauncherDataPath + @"\Minecraft\bin\LWJGL\lwjgl_util.jar;" + Globals.LauncherDataPath + @"\Minecraft\bin\minecraft.jar;" + "\"";
            i.Arguments += " -Xms" + LauncherSettings.Default.InitialMemory;
            i.Arguments += " -Xmx" + LauncherSettings.Default.MaxMemory;
            i.Arguments += " -Djava.library.path=\"" + Globals.LauncherDataPath + @"\Minecraft\bin\LWJGL\natives" + "\"";
            if (LauncherSettings.Default.UseXincgc) i.Arguments += " -Xincgc";
            if (LauncherSettings.Default.UseServer) i.Arguments += " -server";
            i.Arguments += LauncherSettings.Default.VMArgs;
            i.Arguments += " net.minecraft.client.Minecraft";
            i.Arguments += " " + Username + " " + AuthCode;

            i.UseShellExecute = false;
            i.RedirectStandardError = true;
            i.RedirectStandardInput = true;
            i.RedirectStandardOutput = true;
            i.CreateNoWindow = true;
            p.StartInfo = i;
            p.Start();
            return p;
        }

        public static async Task<string[]> AuthenticatePlayer(string username, string password)
        {
            WebClient w = new WebClient();
            string res = w.DownloadString("https://login.minecraft.net/?user=" + username + "&password=" + password + "&version=9001");
            return res.Split(':');
        }
    }
}
