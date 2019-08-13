using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.ExceptionServices;
using System.ServiceProcess;
using System.Windows;
using Newtonsoft.Json;

namespace AnyExeToService
{
    public class ServiceInfo : INotifyPropertyChanged
    {
        private string _serviceName;
        private string _exePath;
        private string _arguments;
        private string _logs;
        private string _desc;
        private bool _advModel;
        private Visibility _advModelVisibility = Visibility.Collapsed;
        private ServiceStartType _serviceStartType = ServiceStartType.Auto;
        private ServiceAccount _serviceAccount = ServiceAccount.LocalService;
        private string _displayName;

        public ServiceInfo()
        {
            try
            {
                var sysSvrs = ServiceController.GetServices();
                AllServices = new SystemServiceInfo[sysSvrs.Length];
                for (var i = 0; i < sysSvrs.Length; i++)
                {
                    AllServices[i] = new SystemServiceInfo()
                    {
                        DisplayName = sysSvrs[i].DisplayName,
                        Name = sysSvrs[i].ServiceName
                    };
                }
            }
            catch (Exception ex)
            {
                _logs += $"获取服务清单出错：{ex.Message}{Environment.NewLine}";
            }
        }

        public string DisplayName
        {
            get => _displayName;
            set
            {
                if (_displayName == value) return;
                _displayName = value;
                OnPropertyChanged(nameof(DisplayName));
            }
        }

        public string ServiceName
        {
            get => _serviceName;
            set
            {
                if (_serviceName == value) return;
                _serviceName = value;
                OnPropertyChanged(nameof(ServiceName));
            }
        }

        public string ExePath
        {
            get => _exePath;
            set
            {
                if (_exePath == value) return;
                _exePath = value;
                OnPropertyChanged(nameof(ExePath));
            }
        }

        public string Arguments
        {
            get => _arguments;
            set
            {
                if (_arguments == value) return;
                _arguments = value;
                OnPropertyChanged(nameof(Arguments));
            }
        }

        public string Desc
        {
            get => _desc;
            set
            {
                if (_desc == value) return;
                _desc = value;
                OnPropertyChanged(nameof(Desc));
            }
        }

        [JsonIgnore]
        public string Logs
        {
            get => _logs;
            set
            {
                if (_logs == value) return;
                _logs = value;
                OnPropertyChanged(nameof(Logs));
            }
        }

        public bool AdvModel
        {
            get => _advModel;
            set
            {
                if (_advModel == value) return;
                _advModel = value;
                OnPropertyChanged(nameof(AdvModel));
                AdvModelVisibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        [JsonIgnore]
        public Visibility AdvModelVisibility
        {
            get => _advModelVisibility;
            set
            {
                if (_advModelVisibility == value) return;
                _advModelVisibility = value;
                OnPropertyChanged(nameof(AdvModelVisibility));
            }
        }

        public ServiceStartType ServiceStartType
        {
            get => _serviceStartType;
            set
            {
                if (_serviceStartType == value) return;
                _serviceStartType = value;
                OnPropertyChanged(nameof(ServiceStartType));
            }
        }

        public ServiceAccount ServiceAccount
        {
            get => _serviceAccount;
            set
            {
                if (_serviceAccount == value) return;
                _serviceAccount = value;
                OnPropertyChanged(nameof(ServiceAccount));
            }
        }

        [JsonIgnore]
        public ServiceStartType[] ServiceStartTypes => new[]
        {
            ServiceStartType.Auto,
            ServiceStartType.Manual,
            ServiceStartType.Disabled
        };

        [JsonIgnore]
        public ServiceAccount[] ServiceAccounts => new[]
        {
            ServiceAccount.LocalService,
            ServiceAccount.LocalSystem,
            ServiceAccount.NetworkService
        };


        [JsonIgnore]
        public SystemServiceInfo[] AllServices { get; set; }

        [JsonIgnore]
        public SystemServiceInfo CurrentAddService
        {
            get => null;
            set
            {
                if (DepService.Contains(value)) return;
                DepService.Add(value);
            }
        }

        public ObservableCollection<SystemServiceInfo> DepService { get; set; } = new ObservableCollection<SystemServiceInfo>();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class SystemServiceInfo
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
