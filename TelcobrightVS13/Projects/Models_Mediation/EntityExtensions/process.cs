using Newtonsoft.Json;

namespace MediationModel
{
    public partial class process
    {
        public ProcessParamater ProcessParameter { get; set; }
        public void DeserializeParameter()
        {
            this.ProcessParameter = JsonConvert.DeserializeObject<ProcessParamater>(this.ProcessParamaterJson);
        }
        public void ExecuteMainMethod()
        {
            //typeof(ITelcobrightProcess).GetMethod(this.processParameter.MainMethodName).Invoke(null, null);
            
        }
    }
}