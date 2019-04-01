using System.ComponentModel;

namespace AnyExeToService
{
    public class ServiceInfo : INotifyPropertyChanged
    {
        private string _serviceName;
        private string _exePath;
        private string _arguments;
        private string _logs;
        private string _desc;

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
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
