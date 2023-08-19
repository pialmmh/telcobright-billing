using System;
using System.Collections.Generic;
using QuartzTelcobright;

namespace LibraryExtensions
{
    public class RouteDisplayClassData
	{
		public RouteDisplayClassComposer Composer = new RouteDisplayClassComposer();
		public IDictionary<string, IDisplayClass> DicExtensions = new Dictionary<string, IDisplayClass>();//key=id.tostring();
	}
}
