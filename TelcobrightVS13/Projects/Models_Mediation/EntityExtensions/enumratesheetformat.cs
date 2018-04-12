using Newtonsoft.Json;

namespace MediationModel
{
    public partial class enumratesheetformat
    {
        public RateSheetImportParamJson JsonParam { get; set; }
        public void PopulateJsonParam()
        {
            this.JsonParam = new RateSheetImportParamJson();
            if (this.IdentifierTextJson != null && this.IdentifierTextJson.Trim() != "")
                this.JsonParam = JsonConvert.DeserializeObject<RateSheetImportParamJson>(this.IdentifierTextJson);
        }
    }
}