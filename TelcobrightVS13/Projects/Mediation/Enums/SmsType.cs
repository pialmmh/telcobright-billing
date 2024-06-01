namespace TelcobrightMediation
{
    public enum SmsType
    {
        Empty=0,
        InvokeSendRoutingInfoForSm = 1,
        ReturnResultLastSendRoutingInfoForSm = 2,
        InvokeMtForwardSm = 3,
        ReturnResultLastMtForwardSm = 4,
        ReturnError = 5,
    }
}