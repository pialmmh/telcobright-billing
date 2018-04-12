namespace TelcobrightMediation
{
    public enum EventListenerType
    {
        FileBasedListener = 1,
        RadiusListener = 2,
        DiameterListener = 3
    }
    public enum NetworkEventType
    {
        VoiceCallCdr,
        SmsCdr,
        DataConsumptionRecord
    }

}
