namespace ServiceApp
{
    class ServiceConfig
    {
        public string Exe { get; set; }
        public string Argument { get; set; }
        public bool RedirectStandardInput { get; set; } = true;
        public bool RedirectStandardOutput { get; set; } = true;
        public bool UseShellExecute { get; set; } = false;
        public bool CreateNoWindow { get; set; } = true;
    }
}
