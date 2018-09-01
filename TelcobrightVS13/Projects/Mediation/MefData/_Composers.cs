using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using QuartzTelcobright;
using TelcobrightMediation.MefData.GenericAssignment;

namespace TelcobrightMediation
{
	public class AutoCreateJobComposer
	{
		[ImportMany("AutoCreateJob", typeof(IAutoCreateJob))]
		public IEnumerable<IAutoCreateJob> AutoCreateJobs { get; set; }
		public void Compose()
		{
			var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
	}

	public class MefJobComposer
	{
		[ImportMany("Job", typeof(ITelcobrightJob))]
		public IEnumerable<ITelcobrightJob> Jobs { get; set; }

		public void Compose()
		{
			var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}

	}

	public class MefNerCalculationRuleComposer
	{
		[ImportMany("NerCalculationRule", typeof(INerCalculationRule))]
		public IEnumerable<INerCalculationRule> NerRules { get; set; }

		public void Compose()
		{
			var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
	}

	public class DecoderComposer
	{
		[ImportMany("Decoder", typeof(IFileDecoder))]
		public IEnumerable<IFileDecoder> Decoders { get; set; }

		public void Compose()
		{
			var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
		public void ComposeFromPath(string path)
		{
			var catalog = new DirectoryCatalog(path);
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
	}
	public class CdrRuleComposer
	{
		[ImportMany("CdrRule", typeof(ICdrRule))]
		public IEnumerable<ICdrRule> CdrRules { get; set; }

		public void Compose()
		{
			var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
		public void ComposeFromPath(string path)
		{
			var catalog = new DirectoryCatalog(path);
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
	}
	public class ServiceGroupComposer
	{
		[ImportMany("ServiceGroup", typeof(IServiceGroup))]
		public IEnumerable<IServiceGroup> ServiceGroups { get; set; }

		public void Compose()
		{
			var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
		public void ComposeFromPath(string path)
		{
			var catalog = new DirectoryCatalog(path);
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
	}
    public class InvoiceGenerationRuleComposer
    {
        [ImportMany("InvoiceGenerationRule", typeof(IInvoiceGenerationRule))]
        public IEnumerable<IInvoiceGenerationRule> InvoiceGenerationRules { get; set; }

        public void Compose()
        {
            var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
        public void ComposeFromPath(string path)
        {
            var catalog = new DirectoryCatalog(path);
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);
        }
    }
    public class PartnerRuleComposer
	{
		[ImportMany("Partner", typeof(IPartnerRule))]
		public IEnumerable<IPartnerRule> Partners { get; set; }

		public void Compose()
		{
			var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
	}

	public class ServiceFamilyComposer
	{
		[ImportMany("ServiceFamily", typeof(IServiceFamily))]
		public IEnumerable<IServiceFamily> ServiceFamilys { get; set; }

		public void Compose()
		{
			var catalog = new DirectoryCatalog(@"..\..\bin\Extensions\");
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
		public void ComposeFromPath(string catalogPath)
		{
			var catalog = new DirectoryCatalog(catalogPath);
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
	}

	


	public class RouteDisplayClassData
	{
		public RouteDisplayClassComposer Composer = new RouteDisplayClassComposer();
		public IDictionary<string, IDisplayClass> DicExtensions = new Dictionary<string, IDisplayClass>();//key=id.tostring();
	}

	public class RouteDisplayClassComposer
	{
		[ImportMany("DisplayClass", typeof(IDisplayClass))]
		public IEnumerable<IDisplayClass> DisplayClasses { get; set; }

		public void Compose(string path)
		{
			var catalog = new DirectoryCatalog(path);
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}
	}
	public class MefGenericAssignmentFactoryComposer
	{
		[ImportMany("IGenericAssignmentFactory", typeof(IGenericParameterAssignmentFactory))]
		public IEnumerable<IGenericParameterAssignmentFactory> GenericAssignmentFactories { get; set; }

		public void Compose(string path = "")
		{
			string assemblyPath = (path == "" ? @"..\..\bin\Extensions\" : path);
			var catalog = new DirectoryCatalog(assemblyPath);
			var container = new CompositionContainer(catalog);
			container.ComposeParts(this);
		}

	}

}
