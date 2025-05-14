namespace Pustakalaya.Helpers
{
    public class PusherOptions
    {
        public string AppId { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string Cluster { get; set; } = string.Empty;
        public bool UseTLS { get; set; }
    }
}