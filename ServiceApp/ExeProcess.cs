using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using log4net;

namespace ServiceApp
{
    class ExeProcess
    {
        private readonly ServiceConfig _config;
        private Process _mainProcess;
        private readonly ILog _log = LogManager.GetLogger(typeof(ExeProcess));
        private bool _stop = false;
        public ExeProcess(ServiceConfig config)
        {
            _config = config ?? throw new Exception("配置对象不能为空！");
        }

        public void Start()
        {
            StartProcess();
            ThreadPool.QueueUserWorkItem(ProcessEye);
            _log.Info("服务已经成功启动.");
        }

        private void _mainProcess_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            _log.Debug(e.Data);
        }

        void StartProcess()
        {
            _mainProcess?.Kill();
            _mainProcess?.Dispose();
            _log.Debug($"程序路径：{_config.Exe}");
            _log.Debug($"启动参数：{_config.Argument}");
            _mainProcess = new Process
            {
                StartInfo = new ProcessStartInfo(_config.Exe)
                {
                    WorkingDirectory = Path.GetDirectoryName(_config.Exe) ?? throw new InvalidOperationException(),
                    Arguments = _config.Argument,
                    RedirectStandardInput = _config.RedirectStandardInput,
                    RedirectStandardOutput = _config.RedirectStandardOutput,
                    UseShellExecute = _config.UseShellExecute,
                    CreateNoWindow = _config.CreateNoWindow
                }
            };
            if (_config.RedirectStandardOutput)
                _mainProcess.OutputDataReceived += _mainProcess_OutputDataReceived;
            _mainProcess.Start();
            _log.Info($"服务进程{Path.GetFileName(_config.Exe)}已经启动，进程ID：{_mainProcess.Id}");
        }

        void ProcessEye(object notUsed)
        {
            while (!_stop)
            {
                try
                {
                    if (_mainProcess == null)
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                    if (_mainProcess.HasExited)
                    {
                        _mainProcess.Dispose();
                        _mainProcess = null;
                        _log.Warn($"检测到服务进程结束，正在重启...");
                        StartProcess();
                    }
                    else
                    {
                        Thread.Sleep(500);
                    }
                }
                catch (Exception ex)
                {
                    _log.Warn($"监测出错", ex);
                }
            }
        }

        public void Stop()
        {
            _stop = true;
            _mainProcess?.Kill();
            _log.Info("服务已经成功停止.");
        }
    }
}
