using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using log4net;

namespace ServiceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var processName = Process.GetCurrentProcess().ProcessName;
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), processName);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            Trace.WriteLine($"当前正在使用的日志路径为：{dir}");
            var logConfig = $"{dir}\\{processName}.log.config";
            if (!File.Exists(logConfig))
            {
                File.WriteAllText(logConfig, Properties.Resources.log4net1.Replace("$DIR$", dir).Replace("\\", "\\\\"));
            }

            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            log4net.Config.XmlConfigurator.Configure(new Uri(logConfig));

            var log = LogManager.GetLogger(typeof(Program));
            try
            {
                var servicesToRun = new ServiceBase[] { new MainService() };
                ServiceBase.Run(servicesToRun);
            }
            catch (Exception ex)
            {
                log.Fatal("启动服务出错", ex);
            }
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (args.Name.Contains("Newtonsoft.Json"))
                return Assembly.Load(Properties.Resources.Newtonsoft_Json);
            if (args.Name.Contains("log4net"))
                return Assembly.Load(Properties.Resources.log4net_dll);
            return null;
        }
    }
}
