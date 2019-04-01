using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using Path = System.IO.Path;

namespace AnyExeToService
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ServiceInfo _serviceInfo;
        private readonly Process _cmdProcess;
        public MainWindow()
        {
            InitializeComponent();
            _serviceInfo = new ServiceInfo();
            DataContext = _serviceInfo;
            _cmdProcess = new Process();
            _cmdProcess.StartInfo.FileName = "cmd.exe";
            _cmdProcess.StartInfo.UseShellExecute = false;
            _cmdProcess.StartInfo.RedirectStandardInput = true;
            _cmdProcess.StartInfo.RedirectStandardOutput = true;
            _cmdProcess.StartInfo.RedirectStandardError = true;
            _cmdProcess.StartInfo.CreateNoWindow = true;
            _cmdProcess.OutputDataReceived += _cmdProcess_OutputDataReceived;
            _cmdProcess.Start();
            _cmdProcess.BeginOutputReadLine();
        }

        private void _cmdProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            WriteLog(e.Data);
        }

        void WriteLog(string msg)
        {
            _serviceInfo.Logs += $"{msg}{Environment.NewLine}";
            Dispatcher.Invoke(() =>
            {
                TextBoxLogs.ScrollToEnd();
            });
        }

        private async void InstallService_OnClick(object sender, RoutedEventArgs e)
        {
            var install = true;
            if (string.IsNullOrWhiteSpace(_serviceInfo.ServiceName))
            {
                ShowError<RequiredValidationRule>(TextBoxServiceName, TextBox.TextProperty,
                    RequiredValidationRule.Message);
                install = false;
            }
            else if (_serviceInfo.ServiceName.Contains(" ") ||
                     _serviceInfo.ServiceName.Contains("\n") ||
                     _serviceInfo.ServiceName.Contains("\r"))
            {
                ShowError<NoBlankSpaceValidationRule>(TextBoxServiceName, TextBox.TextProperty,
                    NoBlankSpaceValidationRule.Message);
                install = false;
            }

            if (string.IsNullOrWhiteSpace(_serviceInfo.ExePath))
            {
                ShowError<RequiredValidationRule>(TextBoxExePath, TextBox.TextProperty, RequiredValidationRule.Message);
                install = false;
            }

            if (!install) return;
            WriteLog("正在释放相关帮助程序...");
            var dir = Path.GetDirectoryName(_serviceInfo.ExePath);
            if (string.IsNullOrWhiteSpace(dir))
            {
                WriteLog("无法读取待安装程序的目录");
                return;
            }

            var installExe = Path.Combine(dir, "install.exe");
            var bridgeExe = Path.Combine(dir, "ServiceBridge.exe");
            File.WriteAllBytes(installExe, Properties.Resources.install);
            File.WriteAllBytes(bridgeExe, Properties.Resources.ServiceBridge);
            WriteLog("正在安装服务...");
            _cmdProcess.StandardInput.WriteLine($"\"{installExe}\" {_serviceInfo.ServiceName} \"{bridgeExe}\"");

            var installSuccessed = false;
            await Task.Run(() =>
            {
                var i = 0;
                while (i < 10)
                {
                    if (_serviceInfo.Logs.Contains("The service was successfuly added"))
                    {
                        installSuccessed = true;
                        break;
                    }
                    Thread.Sleep(1000);
                    i++;
                }
            });
            if (!installSuccessed)
            {
                WriteLog("服务安装失败.");
                return;
            }

            WriteLog("正在配置服务...");
            var serviceKey = Registry.LocalMachine.OpenSubKey(
                $@"SYSTEM\CurrentControlSet\Services\{_serviceInfo.ServiceName}", true);
            if (serviceKey == null)
            {
                WriteLog("无法找到注册表中的相关服务信息，安装失败(E1).");
                return;
            }

            var serviceParameter = serviceKey.OpenSubKey("Parameters", true) ?? serviceKey.CreateSubKey("Parameters");
            if (serviceParameter == null)
            {
                WriteLog("无法找到注册表中的相关服务信息，安装失败(E2).");
                return;
            }
            serviceParameter.SetValue("Application", _serviceInfo.ExePath, RegistryValueKind.String);
            serviceParameter.SetValue("AppDirectory", dir, RegistryValueKind.String);
            if (!string.IsNullOrWhiteSpace(_serviceInfo.Arguments))
            {
                serviceParameter.SetValue("AppParameters", _serviceInfo.Arguments, RegistryValueKind.String);
            }

            if (!string.IsNullOrWhiteSpace(_serviceInfo.Desc))
            {
                serviceKey.SetValue("Description", _serviceInfo.Desc);
            }

            serviceKey.Close();
            WriteLog("服务配置成功，正在启动...");
            _cmdProcess.StandardInput.WriteLine($"net start {_serviceInfo.ServiceName}");
            WriteLog("正在清理...");
            try
            {
                File.Delete(installExe);
            }
            catch (Exception ex)
            {
                WriteLog($"清理失败：{ex.Message}");
            }
        }

        private void ChooseExeFile_OnClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog
            {
                Filter = "exe文件|*.exe"
            };
            if (opf.ShowDialog(this) != true) return;
            _serviceInfo.ExePath = opf.FileName;
        }

        void ShowError<TRule>(FrameworkElement ele, DependencyProperty dependencyProperty, string message) where TRule : ValidationRule
        {
            var bindingExpress = ele.GetBindingExpression(dependencyProperty);
            var rule = Activator.CreateInstance<TRule>();
            var error = new ValidationError(rule, bindingExpress, message, null);
            Validation.MarkInvalid(bindingExpress, error);
        }

        private void UnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_serviceInfo.ServiceName)) return;
            LabelDialogContent.Content = $"确定要删除服务[{_serviceInfo.ServiceName}]么？";
            DialogHostA.IsOpen = true;
        }

        private void DoUnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            DialogHostA.IsOpen = false;
            if (string.IsNullOrWhiteSpace(_serviceInfo.ServiceName)) return;
            _cmdProcess.StandardInput.WriteLine($"sc delete {_serviceInfo.ServiceName}");
        }
    }

    public class RequiredValidationRule : ValidationRule
    {
        public const string Message = "该字段必须填写";
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            return string.IsNullOrWhiteSpace(value?.ToString()) ? new ValidationResult(false, Message) : ValidationResult.ValidResult;
        }
    }

    public class NoBlankSpaceValidationRule : ValidationRule
    {
        public const string Message = "该字段不能包含空格字符";
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value == null) return ValidationResult.ValidResult;
            if (string.IsNullOrWhiteSpace(value.ToString())) return ValidationResult.ValidResult;
            if (value.ToString().Contains(" ") || value.ToString().Contains("\n") || value.ToString().Contains("\r"))
                return new ValidationResult(false, Message);
            return ValidationResult.ValidResult;
        }
    }

}
