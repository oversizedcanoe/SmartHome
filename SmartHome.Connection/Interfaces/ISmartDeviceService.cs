namespace SmartHome.Connection.Interfaces
{
    public interface ISmartDeviceService
    {
        public Task DiscoverDevices();
     
        public List<ISmartDevice> AvailableDevices { get; set; }
    }
}
