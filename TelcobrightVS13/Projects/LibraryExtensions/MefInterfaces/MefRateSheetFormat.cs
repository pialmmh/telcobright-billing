using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
namespace LibraryExtensions
{
    public class ComposeRateSheetFormat
    {
        [ImportMany("RateSheetFormat", typeof(IRateSheetFormat))]
        public IEnumerable<IRateSheetFormat> RateSheetFormats { get; set; }

        public void Compose(string path)
        {
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
    public class RateSheetFormatContainer
    {
        public ComposeRateSheetFormat Composer = new ComposeRateSheetFormat();
        public IDictionary<string, IRateSheetFormat> DicExtensions = new Dictionary<string, IRateSheetFormat>();//key=id.tostring();
    }
}
