using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;

namespace TelcobrightMediation
{
    public class StringExpressionGeneratorContainer
    {
        [ImportMany("StringExpressionGenerator", typeof(IStringExpressionGenerator))]
        private IEnumerable<IStringExpressionGenerator> Generators { get; set; }
        public Dictionary<string, IStringExpressionGenerator> ExpressionGenerators = new Dictionary<string, IStringExpressionGenerator>();
        public void Compose()
        {
            var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
            this.ExpressionGenerators= this.Generators.ToDictionary(a => a.RuleName);
        }
        public void ComposeFromPath(string path)
        {
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
        
    }
}