using System.Linq;
using NUnit.Framework;
using StatLight.Core.Events;
using Microsoft.Practices.Composite.Events;

namespace StatLight.Core.Tests._ProjectEnforcements
{
	[TestFixture]
	public class EventTests
	{
		[Test]
		public void All_types_inheriting_from_EventBase_should_end_with_Event()
		{
			var allEventTypes = (from type in typeof (CompositeEvent<>).Assembly.GetTypes()
			                     where type.IsSubclassOf(typeof (EventBase))
								 && !type.Name.EndsWith("Event")
								 && !type.Name.EndsWith("CompositeEvent`1")
								 select type).ToList();

			if(allEventTypes.Count > 0)
			{
				var badEventTypes = string.Join("   - \n", allEventTypes.Select(s=>s.FullName).ToArray());
				Assert.Fail("Event types don't have a name that end with Event: \n" + badEventTypes);
			}
		}
	}
}
