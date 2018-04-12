using System.Collections.Generic;

namespace MediationModel
{
    public class RateSheetImportParamJson
    {
        public bool CountryWiseCodeEnd { get; set; }
        public List<List<string>> LstOrRuledIdentifierStrings { get; set; }
        public RateSheetImportParamJson()
        {
            this.LstOrRuledIdentifierStrings = new List<List<string>>();
        }
    }
}