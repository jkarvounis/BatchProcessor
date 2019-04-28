using BatchProcessorUI.Util;

namespace BatchProcessorUI.Model
{
    public class BatchProcessorState : ModelBase
    {
        private BatchProcessor.ProcessorSettings _settings;
        private string text;

        public BatchProcessorState()
        {
            _settings = BatchProcessor.ProcessorSettings.LoadOrDefault(BatchProcessor.Program.SETTINGS_FILE);
        }

        public void Load()
        {
            BatchProcessor.ProcessorSettings settings = BatchProcessor.ProcessorSettings.LoadOrDefault(BatchProcessor.Program.SETTINGS_FILE);
            IsServer = settings.IsServer;
            LocalSlots = settings.LocalSlots;
            JobServerPort = settings.JobServerPort;
            WorkerPort = settings.WorkerPort;
            ServerAddress = settings.ServerAddress;
        }

        public void Save()
        {
            _settings.Save(BatchProcessor.Program.SETTINGS_FILE);
        }

        public string ConsoleText
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    RaisePropertyChanged("ConsoleText");
                }
            }
        }

        public bool IsServer
        {
            get { return _settings.IsServer; }
            set
            {                
                if (_settings.IsServer != value)
                {
                    _settings.IsServer = value;
                    RaisePropertyChanged("IsServer");
                    RaisePropertyChanged("IsWorker");
                }
            }
        }

        public bool IsWorker
        {
            get { return !_settings.IsServer; }
            set
            {
                if (_settings.IsServer != !value)
                {
                    _settings.IsServer = !value;
                    RaisePropertyChanged("IsServer");
                    RaisePropertyChanged("IsWorker");
                }
            }
        }

        public int LocalSlots
        {
            get { return _settings.LocalSlots; }
            set
            {
                if (_settings.LocalSlots != value)
                {
                    _settings.LocalSlots = value;
                    RaisePropertyChanged("LocalSlots");
                }
            }
        }

        public int JobServerPort
        {
            get { return _settings.JobServerPort; }
            set
            {
                if (_settings.JobServerPort != value)
                {
                    _settings.JobServerPort = value;
                    RaisePropertyChanged("JobServerPort");
                }
            }
        }

        public int WorkerPort
        {
            get { return _settings.WorkerPort; }
            set
            {
                if (_settings.WorkerPort != value)
                {
                    _settings.WorkerPort = value;
                    RaisePropertyChanged("WorkerPort");
                }
            }
        }

        public string ServerAddress
        {
            get { return _settings.ServerAddress; }
            set
            {
                if (_settings.ServerAddress != value)
                {
                    _settings.ServerAddress = value;
                    RaisePropertyChanged("ServerAddress");
                }
            }
        }
    }
}
