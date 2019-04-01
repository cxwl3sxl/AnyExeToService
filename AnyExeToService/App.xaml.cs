using System.IO;
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
        }
    }
}
