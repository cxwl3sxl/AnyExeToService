using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using log4net;

namespace ServiceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.log4net1)))
            {
                log4net.Config.XmlConfigurator.Configure(ms);
            }

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
            return null;
        }
    }
}
