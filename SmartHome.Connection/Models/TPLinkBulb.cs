using Newtonsoft.Json.Linq;
using SmartHome.Connection.Interfaces;
using System.Drawing;
using TPLinkSmartDevices.Data;
using static TPLinkSmartDevices.Devices.TPLinkSmartBulb;
using TPLinkSmartBulb = TPLinkSmartDevices.Devices.TPLinkSmartBulb;

namespace SmartHome.Connection.Models
{
    public class TPLinkBulb : ISmartBulb
    {
        private TPLinkSmartBulb _bulb;

        public TPLinkBulb(TPLinkSmartBulb bulb)
        {
            this._bulb = bulb;
        }

        public int _bulbH => _bulb.HSV.Hue;
        public int _bulbS => _bulb.HSV.Saturation;
        public int _bulbV => _bulb.HSV.Value;

        public string Alias
        {
            get
            {
                return this._bulb.Alias;
            }
            set
            {
                this._bulb.SetAlias(value);
            }
        }

        public string Model => this._bulb.Model;
        public string Description => ""; // TODO

        public bool On 
        {
            get
            {
                return this._bulb.PoweredOn;
            }
            set
            {
                this._bulb.SetPoweredOn(value);
            }
        }

        public bool IsColorTemp => this._bulb.IsVariableColorTemperature;
        public int MinColorTemp => 2500; // TODO Get from dict or programmatically
        public int MaxColorTemp => 6500; // TODO Get from dict or programmatically
        public int ColorTemp
        {
            get
            {
                return this._bulb.ColorTemperature;
            }
            set
            {
                // TODO validate with min/max
                this._bulb.SetColorTemp(value);
            }
        }

        public bool IsDimmable => this._bulb.IsDimmable;
        public int Brightness
        {
            get
            {
                return this._bulb.Brightness;
            }
            set
            {
                this._bulb.SetBrightness(value);
            }
        }

        public bool IsColor => this._bulb.PoweredOn;

        public int Hue
        {
            get
            {
                return this._bulb.HSV.Hue;
            }
            set
            {
                BulbHSV desiredHSV = new BulbHSV()
                {
                    Hue = value,
                    Saturation = 100,
                    Value = 255
                };

                this._bulb.SetHSV(desiredHSV);
            }
        }
    }
}
