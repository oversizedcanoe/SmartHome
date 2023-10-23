namespace SmartHome.Connection.Interfaces
{
    public interface ISmartDevice
    {
        public string Alias { get; set; }
        public string Model { get; }
        public string Description { get; }

        public bool On { get; set; }
    }
}
