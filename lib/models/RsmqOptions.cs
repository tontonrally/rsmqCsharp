using StackExchange.Redis;

namespace RsmqCsharp
{
    public class RsmqOptions
    {
        private string _host = "127.0.0.1";
        /// <summary>
        /// (optional) (Default: "127.0.0.1") The Redis server
        /// </summary>
        public string Host
        {
            get
            {
                return _host;
            }
            set
            {
                _host = value;
            }
        }

        private int _port = 6379;
        /// <summary>
        /// (optional) (Default: 6379) The Redis port
        /// </summary>
        public int Port
        {
            get
            {
                return _port;
            }
            set
            {
                _port = value;
            }
        }

        /// <summary>
        /// The additional Redis options string as described at https://stackexchange.github.io/StackExchange.Redis/Configuration.html
        /// </summary>
        private string _options = null;
        public string Options
        {
            get
            {
                return _options;
            }
            set
            {
                _options = value;
            }
        }

        // public Client { get; set; }

        private string _namespace = "rsmq";
        public string Namespace
        {
            get
            {
                return _namespace;
            }
            set
            {
                _namespace = value.TrimEnd(':');
            }
        }

        private bool _realtime = false;
        public bool Realtime
        {
            get
            {
                return _realtime;
            }
            set
            {
                _realtime = value;
            }
        }

        private string _password = null;
        public string Password
        {
            get
            {
                return _password;
            }
            set
            {
                _password = value;
            }
        }
    }
}