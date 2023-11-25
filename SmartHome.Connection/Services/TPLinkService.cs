using Serilog;
using SmartHome.Connection.Interfaces;
using SmartHome.Connection.Models;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using TPLinkSmartDevices;
using TPLinkSmartDevices.Messaging;
using TPLinkSmartBulb = TPLinkSmartDevices.Devices.TPLinkSmartBulb;
using TPLinkSmartDevice = TPLinkSmartDevices.Devices.TPLinkSmartDevice;
using TPLinkSmartPlug = TPLinkSmartDevices.Devices.TPLinkSmartPlug;

namespace SmartHome.Connection.Services
{
    public class TPLinkService : ISmartDeviceService
    {
        public List<ISmartDevice> AvailableDevices { get; set; } = new();

        public async Task DiscoverDevices()
        {
            Log.Logger.Information("Discovering devices...");
            this.AvailableDevices.Clear();
            List<TPLinkSmartDevice> discoveredDevices = new List<TPLinkSmartDevice>();

            if (Configuration.USE_BROADCAST_ADDRESS)
            {
                Log.Logger.Information("Using broadcast IP address (255.255.255.255)");

                discoveredDevices = await new TPLinkDiscovery().Discover();
            }
            else
            {

                //Get all Wifi IP Addresses which are not Local Area Connection.This may not work everywhere, but my bulbs are on my Wifi IP, not Local Area Connection.
                var addressesToSearch = NetworkInterface.GetAllNetworkInterfaces()
                                            .Where(ni => ni.Name.Contains("Local Area Connection", StringComparison.OrdinalIgnoreCase) == false
                                                      && ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                                            .SelectMany(ni => ni.GetIPProperties().UnicastAddresses)
                                            .Where(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                            .Select(ip => ip.Address.ToString());

                Log.Logger.Information($"Using IP address(es): {string.Join(", ", addressesToSearch)}");

                foreach (string address in addressesToSearch)
                {
                    string broadcastAddress = address.Substring(0, address.LastIndexOf('.') + 1) + "255";
                    discoveredDevices.AddRange(await new TPLinkDiscovery().Discover(target: broadcastAddress));
                }
            }

            Log.Logger.Information($"{discoveredDevices.Count} devices found.");

            foreach (var device in discoveredDevices)
            {
                device.MessageCache = new NoMessageCache();
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