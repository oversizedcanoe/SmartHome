namespace SmartHome.Connection.Interfaces
{
    public interface ISmartDevice
    {
        public string Alias { get; set; }
        public string Model { get; }
        public string Description { get; }
        public string IPAddress { get; }
        public string MACAddress { get; }

        public bool On { get; set; }

        public event EventHandler OnPowerStateChanged;

        public Task RefreshAsync();
    }
}
