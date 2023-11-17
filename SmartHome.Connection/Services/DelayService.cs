namespace SmartHome.Connection.Services
{
    public class DelayService
    {
        public void WaitForDeviceToUpdate(int delayTimeMs = 125)
        {
            Task.Delay(delayTimeMs).Wait();
        }
    }
}
