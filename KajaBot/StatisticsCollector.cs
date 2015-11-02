using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace KajaBot
{
    [Serializable]
    internal class StatisticsCollector : INotifyPropertyChanged
    {
        [field: NonSerialized] private static readonly object _lock = new object();
        [field: NonSerialized] private static volatile StatisticsCollector _instance;

        [field: NonSerialized] private static readonly string DataFileName = AppDomain.CurrentDomain.BaseDirectory +
                                                                             "stats.bin";

        private int _executedCommandCount;
        private int _executedRestaurantMenuRequestCount;
        private int _incomingMessageCount;
        private int _ocredPageCount;
        private int _outgoingWebRequestCount;
        private int _unknownCommandCount;

        public int IncomingMessageCount
        {
            get { return _incomingMessageCount; }
            private set
            {
                _incomingMessageCount = value;
                OnPropertyChanged("IncomingMessageCount");
            }
        }

        public int ExecutedCommandCount
        {
            get { return _executedCommandCount; }
            private set
            {
                _executedCommandCount = value;
                OnPropertyChanged("ExecutedCommandCount");
            }
        }

        public int OutgoingWebRequestCount
        {
            get { return _outgoingWebRequestCount; }
            private set
            {
                _outgoingWebRequestCount = value;
                OnPropertyChanged("OutgoingWebRequestCount");
            }
        }

        public int ExecutedRestaurantMenuRequestCount
        {
            get { return _executedRestaurantMenuRequestCount; }
            private set
            {
                _executedRestaurantMenuRequestCount = value;
                OnPropertyChanged("ExecutedRestaurantMenuRequestCount");
            }
        }

        public int OcredPageCount
        {
            get { return _ocredPageCount; }
            private set
            {
                _ocredPageCount = value;
                OnPropertyChanged("OcredPageCount");
            }
        }

        public int UnknownCommandCount
        {
            get { return _unknownCommandCount; }
            private set
            {
                _unknownCommandCount = value;
                OnPropertyChanged("UnknownCommandCount");
            }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public static StatisticsCollector GetInstance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        FileStream fileStream;
                        try
                        {
                            fileStream = File.OpenRead(DataFileName);
                        }
                        catch (FileNotFoundException)
                        {
                            _instance = new StatisticsCollector();
                            return _instance;
                        }

                        var serializer = new BinaryFormatter();
                        _instance = (StatisticsCollector) serializer.Deserialize(fileStream);
                        fileStream.Close();

                        return _instance;
                    }
                }
            }

            return _instance;
        }

        public void Reset()
        {
            IncomingMessageCount = 0;
            ExecutedCommandCount = 0;
            OutgoingWebRequestCount = 0;
            OcredPageCount = 0;
            ExecutedRestaurantMenuRequestCount = 0;
        }

        public void IncrementIncomingMessageCount()
        {
            IncomingMessageCount++;
        }

        public void IncrementExecutedCommandCount()
        {
            ExecutedCommandCount++;
        }

        public void IncrementOutgoingWebRequestCount()
        {
            OutgoingWebRequestCount++;
        }

        public void IncrementExecutedRestaurantMenuRequestCount()
        {
            ExecutedRestaurantMenuRequestCount++;
        }

        public void IncrementOcredPageCount()
        {
            OcredPageCount++;
        }

        public void IncrementUnknownCommandCount()
        {
            UnknownCommandCount++;
        }

        public void SaveToFile()
        {
            FileStream fileStream = File.OpenWrite(DataFileName);
            var serializer = new BinaryFormatter();
            serializer.Serialize(fileStream, this);
            fileStream.Close();
        }

        private void OnPropertyChanged(string name)
        {
            SaveToFile();

            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(name));
        }
    }
}