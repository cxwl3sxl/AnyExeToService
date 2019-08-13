using System;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using log4net;
using Newtonsoft.Json;

namespace ServiceApp
{
    partial class MainService : ServiceBase
    {
        private readonly ServiceConfig _config;
        private readonly ExeProcess _exeProcess;
        private readonly ILog _log = LogManager.GetLogger(typeof(MainService));
        public MainService()
        {
            InitializeComponent();
            var appName = Assembly.GetExecutingAssembly().Location;
            var configFile = $"{appName}.json";
            if (!File.Exists(configFile)) return;
            var configContent = File.ReadAllText(configFile);
            try
            {
                _config = JsonConvert.DeserializeObject<ServiceConfig>(configContent);
                if (!File.Exists(_config.Exe)) throw new Exception($"指定的启动程序{_config.Exe}不存在");
                _exeProcess = new ExeProcess(_config);
            }
            catch (Exception ex)
            {
                _log.Warn("读取服务配置文件出错", ex);
                _log.Warn($"配置文件地址为：{configFile}");
                _log.Warn($"配置文件内容为：{Environment.NewLine}{configContent}");
            }
        }

        protected override void OnStart(string[] args)
        {
            if (_config == null) return;
            try
            {
                _exeProcess?.Start();
            }
            catch (Exception ex)
            {
                _log.Warn("启动服务出错", ex);
                throw;
            }
        }

        protected override void OnStop()
        {
            if (_config == null) return;
            try
            {
                _exeProcess?.Stop();
            }
            catch (Exception ex)
            {
                _log.Warn("停止服务出错", ex);
            }
        }
    }
}
