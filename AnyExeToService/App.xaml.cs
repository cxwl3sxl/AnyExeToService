using System;
using System.IO;
using System.Reflection;
using System.Windows;

namespace AnyExeToService
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            File.WriteAllBytes("MaterialDesignThemes.Wpf.dll", AnyExeToService.Properties.Resources.MaterialDesignThemes_Wpf);
            File.WriteAllBytes("MaterialDesignColors.dll", AnyExeToService.Properties.Resources.MaterialDesignColors);
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return args.Name.Contains("Newtonsoft.Json") ? Assembly.Load(AnyExeToService.Properties.Resources.Newtonsoft_Json) : null;
        }
    }
}
