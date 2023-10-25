using Newtonsoft.Json.Linq;
using SmartHome.Connection.Interfaces;
using System.Drawing;
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

        public string Alias
        {
            get
            {
                return this._bulb.Alias;
            }
            set
            {
                this._bulb.Alias = value;
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
                this._bulb.PoweredOn = value;
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
                this._bulb.ColorTemperature = value;
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
                this._bulb.Brightness = value;
            }
        }

        public bool IsColor => this._bulb.PoweredOn;

        public Color Color
        {
            get
            {
                return BulbHSVToColor(_bulb.HSV);
            }
            set
            {
                this._bulb.HSV = ColorToBulbHSV(value);
            }
        }

        private Color BulbHSVToColor(BulbHSV bulbHSV)
        {
            // Thanks to https://stackoverflow.com/a/1626232 

            decimal hue = bulbHSV.Hue;
            // The BulbHSV Saturation ranges from 0-100. Need it to be 0-1.
            decimal saturation = Decimal.Divide(bulbHSV.Saturation, 100);
            // The "BulbHSV" value ranges from 0-255. Need it to be 0-1.
            decimal value = Decimal.Divide(bulbHSV.Value, 255);

            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            decimal f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            int v = Convert.ToInt32(value);
            int p = Convert.ToInt32(value * (1 - saturation));
            int q = Convert.ToInt32(value * (1 - f * saturation));
            int t = Convert.ToInt32(value * (1 - (1 - f) * saturation));

            switch (hi)
            {
                case 0:
                    return Color.FromArgb(255, v, t, p);
                case 1:
                    return Color.FromArgb(255, q, v, p);
                case 2:
                    return Color.FromArgb(255, p, v, t);
                case 3:
                    return Color.FromArgb(255, p, q, v);
                case 4:
                    return Color.FromArgb(255, t, p, v);
                default:
                    return Color.FromArgb(255, v, p, q);
            }
        }

        private BulbHSV ColorToBulbHSV(Color color)
        {
            // Thanks to https://stackoverflow.com/a/1626232 

            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            BulbHSV bulbHSV = new BulbHSV()
            {
                Hue = (byte)color.GetHue(), // I think this needs to change from 0-360 to 0-255
                Saturation = (byte)((max == 0) ? 0 : (1d - (1d * min / max))), // I think this needs to change from 0-1 to 0-255
                Value = (byte)(max / 255d),// I think this needs to change from 0-1 to 0-255
            };

            return bulbHSV;
        }


    }
}
