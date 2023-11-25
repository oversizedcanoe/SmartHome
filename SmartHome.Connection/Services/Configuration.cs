namespace SmartHome.Connection.Services
{
    public static class Configuration
    {
        public static int LAVA_LAMP_DELAY_TIME_MS = 400;
        public static int LAVA_LAMP_HUE_STEP = 5;
        
        public static int RAVE_DELAY_TIME_MS = 200;
        public static int RAVE_HUE_STEP = 10;
        
        public static int MODULATE_SLEEP_TIME_MS = -1;

        public static int WAVE_MODE_WAVE_LENGTH = 90;
        public static int WAVE_MODE_MIN_SLEEP_TIME_MS = 150;
        public static int WAVE_MODE_MAX_SLEEP_TIME_MS = 600;

        public static int FLASH_SLEEP_TIME_MS = 250;

        public static bool USE_BROADCAST_ADDRESS = false;

    }
}
