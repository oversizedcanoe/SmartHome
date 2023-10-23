using SmartHome.Connection.Interfaces;
using SmartHome.Connection.Models;
using TPLinkSmartBulb =  TPLinkSmartDevices.Devices.TPLinkSmartBulb;
using TPLinkSmartDevice = TPLinkSmartDevices.Devices.TPLinkSmartDevice;
using TPLinkSmartPlug = TPLinkSmartDevices.Devices.TPLinkSmartPlug;

namespace SmartHome.Connection.Services
{
    public class TPLinkService : ISmartDeviceService
    {
        public List<ISmartDevice> AvailableDevices { get; set; } = new();

        public async Task DiscoverDevices()
        {
            List<TPLinkSmartDevice> discoveredDevices = await new TPLinkSmartDevices.TPLinkDiscovery().Discover();

            foreach (var device in discoveredDevices)
            {
                this.AvailableDevices.Add(MapToSmartDevice(device));
            }
        }

        private ISmartDevice MapToSmartDevice(TPLinkSmartDevice tpLinkDevice)
        {
            if (tpLinkDevice is TPLinkSmartBulb bulb)
            {
                return new TPLinkBulb(bulb);
            }
            else if (tpLinkDevice is TPLinkSmartPlug plug)
            {
                return new TPLinkPlug(plug);
            }

            throw new Exception($"Unsupported {nameof(TPLinkSmartDevice)}, only Bulb and Plug types supported");
        }

    }
}