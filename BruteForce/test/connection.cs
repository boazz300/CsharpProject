namespace test
{

    class connection
    {
        public string _ip;
        public string _user;
        public string _pass;
        public int _connectError;
        public string _os;
        public connection(string ip, string user, string pass, int connectError, string os)
        {
            _ip = ip;
            _user = user;
            _pass = pass;
            _connectError = connectError;
            _os = os;
        }
        public override string ToString()
        {
            return "Connection " + _os + ": " + _user + ":" + _pass + "@" + _ip;
        }
    }
}
