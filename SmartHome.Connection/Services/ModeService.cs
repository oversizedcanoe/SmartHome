using SmartHome.Connection.Interfaces;

namespace SmartHome.Connection.Services
{
    public class ModeService
    {
        public enum Mode
        {
            None,
            Lavalamp,
            Rave,
            Waves, // Modulate sleep time to make slower and faster changes
            Normal // set to standard color/brightness
        }

        public Mode CurrentMode { get; set; } = Mode.None;

        public async void RunMode(Mode mode, List<ISmartBulb> bulbs, CancellationToken cancellationToken)
        {
            Console.WriteLine($"{mode} starting for {bulbs.Count} bulbs");

            cancellationToken.Register(OnCancellationTokenCancelled);

            Task[] tasks = new Task[bulbs.Count];

            int hueDifferencePerBulb = 360 / bulbs.Count;
            int hueOffset = 0;

            for (int i = 0; i < bulbs.Count; i++)
            {
                bulbs[i].Hue = hueOffset;
                hueOffset += hueDifferencePerBulb;

                switch (mode)
                {
                    case Mode.Lavalamp:
                        tasks[i] = FlowThroughColors(bulbs[i], cancellationToken, Configuration.LAVA_LAMP_HUE_STEP, Configuration.LAVA_LAMP_DELAY_TIME_MS);
                        break;
                    case Mode.Rave:
                        tasks[i] = FlowThroughColors(bulbs[i], cancellationToken, Configuration.RAVE_HUE_STEP, Configuration.RAVE_DELAY_TIME_MS);
                        break;
                    case Mode.Waves:
                        throw new NotImplementedException();
                    default:
                        break;
                }
            }

            this.CurrentMode = mode;

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

        private void OnCancellationTokenCancelled()
        {
            Console.WriteLine($"{this.CurrentMode} canceled");
            this.CurrentMode = Mode.None;
        }
    }
}
