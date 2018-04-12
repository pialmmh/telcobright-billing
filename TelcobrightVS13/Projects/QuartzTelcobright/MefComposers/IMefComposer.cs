using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace QuartzTelcobright.MefComposer
{
    public interface IMefComposer
    {
        string Type { get; }
        Dictionary<string, object> Compose(CompositionContainer container);
    }
}
