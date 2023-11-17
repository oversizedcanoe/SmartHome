using SmartHome.Connection.Interfaces;
using TPLinkSmartPlug = TPLinkSmartDevices.Devices.TPLinkSmartPlug;
namespace SmartHome.Connection.Models
{
    public class TPLinkPlug : ISmartPlug
    {
        private TPLinkSmartPlug _plug;

        public TPLinkPlug(TPLinkSmartPlug plug)
        {
            _plug = plug;
        }

        public string Alias
        {
            get
            {
                return this._plug.Alias;
            }
            set
            {
                this._plug.SetAlias(value);
            }
        }

        public string Model => this._plug.Model;
        public string Description => ""; // TODO
        public string IPAddress => this._plug.Hostname;
        public string MACAddress => this._plug.MacAddress;

        public bool On
        {
            get
            {
                return this._plug.OutletPowered;
            }
            set
            {
                this._plug.SetPoweredOn(value);
                OnPowerStateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public DateTime OnSince
        {
            get
            {
                return this._plug.PoweredOnSince;
            }
        }

        public event EventHandler OnPowerStateChanged;

        public async Task RefreshAsync()
        {
            await this._plug.Refresh();
        }

    }
}
