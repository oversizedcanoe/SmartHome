using SmartHome.Connection.Interfaces;

namespace SmartHome.Connection.Services
{
    public class ModeService
    {
        // TODO: I should make one single method called "RunMode" which takes the same params as below plus an enum value for ModeType.
        // This way the logic of creating tasks etc can be generic.
        public async void RunLavaLampMode(List<ISmartBulb> bulbs, CancellationToken cancellationToken)
        {
            Task[] tasks = new Task[bulbs.Count];

            for (int i = 0; i < bulbs.Count; i++)
            {
                tasks[i] = FlowThroughColors(bulbs[i], cancellationToken, Configuration.LAVA_LAMP_HUE_STEP, Configuration.LAVA_LAMP_DELAY_TIME_MS);
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);
        }

        private async Task FlowThroughColors(ISmartBulb bulb, CancellationToken cancellationToken, int step, int sleepTimeMs)
        {
            // We want to change the "direction" of color change randomly, and differently for each bulb.
            int maxNumberOfChangesPerCycle = 360 / step;

            Random random = new Random();

            // To prevent accidentally generating a really small random interation count, set the min to one quarter of the max.
            int cyclesPerDirectionChange = random.Next(maxNumberOfChangesPerCycle / 4, maxNumberOfChangesPerCycle);
            
            bool increasing = true;
            int currentIterationCount = 0;

            while (cancellationToken.IsCancellationRequested == false)
            {
                if (currentIterationCount > cyclesPerDirectionChange)
                {
                    increasing = !increasing;
                    currentIterationCount = 0;
                }

                if (increasing)
                {
                    // Hue can be between 0-360.
                    if (bulb.Hue <= (360 - step))
                    {
                        bulb.Hue += step;
                    }
                    else
                    {
                        bulb.Hue = 0;
                    }
                }
                else
                {
                    // Hue can be between 0-360.
                    if (bulb.Hue >= step)
                    {
                        bulb.Hue -= step;
                    }
                    else
                    {
                        bulb.Hue = 0;
                    }
                }

                currentIterationCount++;
                await Task.Delay(sleepTimeMs);
            }
        }
    }
}
