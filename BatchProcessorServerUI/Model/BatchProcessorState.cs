using BatchProcessorServerUI.Util;

namespace BatchProcessorServerUI.Model
{
    public class BatchProcessorState : ModelBase
    {
        private BatchProcessorServer.Server.Settings _settings;
        private string text;

        public BatchProcessorState()
        {
            _settings = BatchProcessorServer.Server.Settings.Load(BatchProcessorServer.Util.Paths.SETTINGS_FILE);
        }

        public void Load()
        {
            BatchProcessorServer.Server.Settings settings = BatchProcessorServer.Server.Settings.Load(BatchProcessorServer.Util.Paths.SETTINGS_FILE);

            Port = settings.Port;
            HeartbeatMs = settings.HeartbeatMs;
        }

        public void Save()
        {
            _settings.Save(BatchProcessorServer.Util.Paths.SETTINGS_FILE);
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

        public int Port
        {
            get { return _settings.Port; }
            set
            {
                if (_settings.Port != value)
                {
                    _settings.Port = value;
                    RaisePropertyChanged("Port");
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
