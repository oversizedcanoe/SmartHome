using SmartHome.Connection.Interfaces;

namespace SmartHome.Connection.Services
{
    public class ModeService
    {
        public async void RunLavaLampMode(List<ISmartBulb> bulbs, CancellationToken cancellationToken)
        {
            while (cancellationToken.IsCancellationRequested == false)
            {
                foreach(ISmartBulb bulb in bulbs)
                {
                    // Get current hue
                    int hue = bulb.Hue;

                    Console.WriteLine($"Hue: {hue}");

                    // Hue can be between 0-360.
                    if (hue <= 340)
                    {
                        bulb.Hue += 20;
                    }
                    else
                    {
                        bulb.Hue = 20;
                    }
                }

                await Task.Delay(1000);
            }
        }
    }
}
