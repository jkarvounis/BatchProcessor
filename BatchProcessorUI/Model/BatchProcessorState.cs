using BatchProcessorUI.Util;

namespace BatchProcessorUI.Model
{
    public class BatchProcessorState : ModelBase
    {
        private BatchProcessor.Server.Settings _settings;
        private string text;

        public BatchProcessorState()
        {
            _settings = BatchProcessor.Server.Settings.Load(BatchProcessor.Util.Paths.SETTINGS_FILE);
        }

        public void Load()
        {
            BatchProcessor.Server.Settings settings = BatchProcessor.Server.Settings.Load(BatchProcessor.Util.Paths.SETTINGS_FILE);

            LocalSlots = settings.LocalSlots;
            ServerPort = settings.ServerPort;
            ServerAddress = settings.ServerAddress;
            HeartbeatMs = settings.HeartbeatMs;
        }

        public void Save()
        {
            _settings.Save(BatchProcessor.Util.Paths.SETTINGS_FILE);
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

        public int ServerPort
        {
            get { return _settings.ServerPort; }
            set
            {
                if (_settings.ServerPort != value)
                {
                    _settings.ServerPort = value;
                    RaisePropertyChanged("ServerPort");
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

        public int HeartbeatMs
        {
            get { return _settings.HeartbeatMs; }
            set
            {
                if (_settings.HeartbeatMs != value)
                {
                    _settings.HeartbeatMs = value;
                    RaisePropertyChanged("HeartbeatMs");
                }
            }
        }
    }
}
