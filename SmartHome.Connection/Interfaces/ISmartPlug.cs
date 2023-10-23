namespace SmartHome.Connection.Interfaces
{
    public interface ISmartPlug : ISmartDevice
    {
        public DateTime OnSince { get; }
    }
}
