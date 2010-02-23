using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Harness.Events;
using StatLight.Client.Tests;
using StatLight.Core.Serialization;

namespace StatLight.Client.Model
{
    namespace ClientEventTests
    {
        [TestClass]
        public class When_creating_default_instances_of_all_ClientEvent_types : FixtureBase
        {
            [TestMethod]
            public void Should_be_able_to_serialize_all_types()
            {
                //(new TraceClientEvent()).Serialize().ShouldNotBeNull();
                //System.Windows.MessageBox.s() Activator.CreateInstance(typeof(TraceClientEvent)).Serialize();

                foreach (var type in GetAllClientEventTypes())
                {
                    object instance = Activator.CreateInstance(type);
                    instance.Serialize();
                }
            }

            public IEnumerable<Type> GetAllClientEventTypes()
            {
                return (typeof(ClientEvent).Assembly.GetTypes()
                    .Where(w => typeof(ClientEvent).IsAssignableFrom(w)))
                    .Where(w => !w.IsAbstract);
            }
        }
    }
}