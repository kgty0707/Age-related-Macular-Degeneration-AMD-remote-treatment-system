namespace Haply.HardwareAPI.Unity
{
    public interface IFrameRateProvider
    {
        int targetFrequency { get; }
        int actualFrequency { get; }
    }
}
