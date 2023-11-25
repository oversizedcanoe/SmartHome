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
            Christmas, // Green and Red
            Flash
        }

        public Mode CurrentMode { get; set; } = Mode.None;

        private int[] _waveModeSleepTimesMS = new int[Configuration.WAVE_MODE_WAVE_LENGTH];

        public ModeService()
        {
            this.InitializeWaveModeSleepTimes();
        }

        private void InitializeWaveModeSleepTimes()
        {
            // Create a sinusoidal wave of sleep times which will modulate the sleep time of the FlowThroughColors method.
            // y= (coefficient * sin[(2*pi*x)/WAVE_MODE_WAVE_LENGTH]) + offset

            double offset = (Configuration.WAVE_MODE_MAX_SLEEP_TIME_MS + Configuration.WAVE_MODE_MIN_SLEEP_TIME_MS) / 2D;
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

            // For randomized colors, we start each bulb an equal hue away from eachother.
            int hueDifferencePerBulb = 360 / bulbs.Count;
            int hueOffset = 0;

            // For two-color option modes, we want to randomize whether it is 1/2/1 or 2/1/2.
            // We will either set the odd indices to 1, or the even.
            int oddEvenDecider = new Random().Next(0, 2);

            for (int i = 0; i < bulbs.Count; i++)
            {
                switch (mode)
                {
                    case Mode.Lavalamp:
                        SetBulbHSB(bulbs[i], hueOffset).Wait();
                        hueOffset += hueDifferencePerBulb;
                        tasks[i] = FlowThroughColors(bulbs[i], cancellationToken, Configuration.LAVA_LAMP_HUE_STEP, Configuration.LAVA_LAMP_DELAY_TIME_MS);
                        break;
                    case Mode.Rave:
                        SetBulbHSB(bulbs[i], hueOffset).Wait();
                        hueOffset += hueDifferencePerBulb;
                        tasks[i] = FlowThroughColors(bulbs[i], cancellationToken, Configuration.RAVE_HUE_STEP, Configuration.RAVE_DELAY_TIME_MS);
                        break;
                    case Mode.Wave:
                        SetBulbHSB(bulbs[i], hueOffset).Wait();
                        hueOffset += hueDifferencePerBulb;
                        tasks[i] = FlowThroughColors(bulbs[i], cancellationToken, Configuration.LAVA_LAMP_HUE_STEP, Configuration.MODULATE_SLEEP_TIME_MS);
                        break;
                    case Mode.Christmas:
                        int hue = i % 2 == oddEvenDecider ? 120 : 0;
                        tasks[i] = SetBulbHSB(bulbs[i], hue);
                        break;
                    case Mode.Flash:
                        tasks[i] = FlashRandomColors(bulbs[i], cancellationToken);
                        break;
                    default:
                        break;
                }
            }

            this.CurrentMode = mode;

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Tasks which go on forever will never hit here and instead will have OnCancellationTokenCancelled called.
            // Tasks which just set hue will need to have CurrentMode set back to None.
            this.CurrentMode = Mode.None;
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
                    if (bulb.GetHue() <= (360 - step))
                    {
                        bulb.SetHue(bulb.GetHue() + step);
                    }
                    else
                    {
                        bulb.SetHue(0);
                    }
                }
                else
                {
                    // Hue can be between 0-360.
                    if (bulb.GetHue() >= step)
                    {
                        bulb.SetHue(bulb.GetHue() - step);
                    }
                    else
                    {
                        bulb.SetHue(360);
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

        private async Task FlashRandomColors(ISmartBulb bulb, CancellationToken cancellationToken)
        {
            Random random = new Random();

            while (cancellationToken.IsCancellationRequested == false)
            {
                int randomHue = random.Next(0, 360);
                bulb.SetHue(randomHue, 0);
                bulb.On = true;
                await Task.Delay(Configuration.FLASH_SLEEP_TIME_MS);
                bulb.On = false;
            }
        }

        private Task SetBulbHSB(ISmartBulb bulb, int hue, int brightness = 100)
        {
            bulb.SetHue(hue);
            bulb.Brightness = brightness;
            return Task.CompletedTask;
        }

        private void OnCancellationTokenCancelled()
        {
            Console.WriteLine($"{this.CurrentMode} canceled");
            this.CurrentMode = Mode.None;
        }
    }
}
