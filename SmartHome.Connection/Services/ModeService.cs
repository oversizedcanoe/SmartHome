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

        private int[] _waveModeSleepTimesMS = new int[Configuration.WAVE_MODE_WAVE_LENGTH];

        private int _averageSleepTime;

        public ModeService()
        {
            this.InitializeWaveModeSleepTimes();
        }

        private void InitializeWaveModeSleepTimes()
        {
            // Create a sinusoidal wave of sleep times which will modulate the sleep time of the FlowThroughColors method.
            // y= (coefficient * sin[(2*pi*x)/WAVE_MODE_WAVE_LENGTH]) + offset

            double offset = (Configuration.WAVE_MODE_MAX_SLEEP_TIME_MS+ Configuration.WAVE_MODE_MIN_SLEEP_TIME_MS) / 2D;
            double coefficient = (Configuration.WAVE_MODE_MIN_SLEEP_TIME_MS - Configuration.WAVE_MODE_MAX_SLEEP_TIME_MS) / 2D;

            // Total time to complete one cycle is 
            // WAVE_MODE_WAVE_LENGTH * Average Sleep time, aka WAVE_MODE_WAVE_LENGTH * Offset
            // This is because wave length is number of points taken
            // Could change WAVE_MODE_WAVE_LENGTH to be a new Config for "Time for one cycle" as this is more meaningful to the user.

            for (int i = 0; i < Configuration.WAVE_MODE_WAVE_LENGTH; i++)
            {
                int y = (int)Math.Round((coefficient * Math.Sin((2 * Math.PI * i) / Configuration.WAVE_MODE_WAVE_LENGTH)) + offset);
                this._waveModeSleepTimesMS[i] = y;
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
                    int modulatedSleepTimeMs = this._waveModeSleepTimesMS[currentIterationCount % Configuration.WAVE_MODE_WAVE_LENGTH];

                    //Console.WriteLine(new string('.', modulatedSleepTimeMs/2));
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
