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
            Wave, // Modulate sleep time to make slower and faster changes
            Normal // set to standard color/brightness
        }

        public Mode CurrentMode { get; set; } = Mode.None;

        public double[] WaveModeSleepCoefficients = new double[Configuration.WAVE_MODE_CYCLE_COUNT];

        public ModeService()
        {
            this.InitializeWaveModeValues();
        }

        private void InitializeWaveModeValues()
        {
            // TODO could use improvement, I want it to modulate faster, also should use constant from Configuration instead of 270 hardcard

            // Create a sinusoidal wave of cofficients which will modulate the sleep time of the FlowThroughColors method.
            // The coefficients will be applied on a constant equal to Configuration.LAVA_LAMP_DELAY_TIME_MS
            // The range of coefficients should be from 0.1 to 2; meaning our values range from 10% of the normal delay time to 100%
            // i.e. slowest sleep time = Normal * 0.1
            // i.e. longest sleep time = Normal * 2
            // y= 0.95sin[(2*pi*x)/270] + 1.05

            for (int i = 0; i < Configuration.WAVE_MODE_CYCLE_COUNT; i++)
            {
                double y = (0.95 * Math.Sin((2 * Math.PI * i) / 270)) + 1.05;
                this.WaveModeSleepCoefficients[i] = y;
            }
        }

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
                    case Mode.Wave:
                        tasks[i] = FlowThroughColors(bulbs[i], cancellationToken, Configuration.LAVA_LAMP_HUE_STEP, Configuration.MODULATE_SLEEP_TIME_MS);
                        break;
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
                if (currentIterationCount % cyclesPerDirectionChange == 0)
                {
                    increasing = !increasing;
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
                        bulb.Hue = 360;
                    }
                }

                currentIterationCount++;

                if (sleepTimeMs == Configuration.MODULATE_SLEEP_TIME_MS)
                {
                    double coefficient = this.WaveModeSleepCoefficients[currentIterationCount % Configuration.WAVE_MODE_CYCLE_COUNT];

                    int modulatedSleepTimeMs = (int)Math.Round(Configuration.LAVA_LAMP_DELAY_TIME_MS * coefficient);

                    Console.WriteLine(modulatedSleepTimeMs);
                    await Task.Delay(modulatedSleepTimeMs);
                }
                else
                {
                    await Task.Delay(sleepTimeMs);
                }

            }
        }

        private void OnCancellationTokenCancelled()
        {
            Console.WriteLine($"{this.CurrentMode} canceled");
            this.CurrentMode = Mode.None;
        }
    }
}
