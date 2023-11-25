using System.Drawing;

namespace SmartHome.Connection.Interfaces
{
    public interface ISmartBulb : ISmartDevice
    {
        public bool IsColorTemp { get; }
        public int MinColorTemp { get; }
        public int MaxColorTemp { get; }
        public int ColorTemp { get; set; }

        public bool IsDimmable { get; }
        public int Brightness { get; set; }

        public bool IsColor { get; }
        public int GetHue();
        public void SetHue(int hue, int transitionTime = 250);
    }
}
