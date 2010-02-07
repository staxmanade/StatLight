using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Model.DTO;
using StatLight.Client.Model.Events;
using System.Collections.Generic;
using StatLight.Core.Serialization;

namespace StatLight.Client.Silverlight.Tests.Messaging
{
    [TestClass]
    public class When_creating_an_ExceptionInfo : FixtureBase
    {
        private ExceptionInfo _exceptionInfo;
        private Exception _exception;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            try
            {
                Exception innerEx;
                try
                {
                    throw new Exception("Inner ex!");
                }
                catch (Exception i)
                {
                    innerEx = i;
                }

                throw new Exception("Test message!", innerEx);
            }
            catch (Exception e)
            {
                _exception = e;
                _exceptionInfo = new ExceptionInfo(e);
            }
        }

        [TestMethod]
        public void Should_be_able_to_create_the_ExceptionInfo()
        {
            _exceptionInfo.ShouldNotBeNull();
        }

        [TestMethod]
        public void Should_set_the_Message_correctly()
        {
            _exceptionInfo.Message.ShouldBeEqualTo(_exception.Message);
        }

        [TestMethod]
        public void Should_set_the_StackTrace()
        {
            _exceptionInfo.StackTrace.ShouldBeEqualTo(_exception.StackTrace);
        }

        [TestMethod]
        public void Should_set_the_inner_exception()
        {
            _exceptionInfo.InnerException.Message.ShouldBeEqualTo(_exception.InnerException.Message);
        }

        [TestMethod]
        public void Should_set_the_FullMessage()
        {
            _exceptionInfo.FullMessage.ShouldBeEqualTo(_exception.ToString());
        }

        [TestMethod]
        public void Should_override_the_toString_and_return_FullMessage()
        {
            _exceptionInfo.ToString().ShouldBeEqualTo(_exceptionInfo.FullMessage);
        }
    }

    [TestClass]
    public class When_creating_default_instances_of_all_ClientEvent_types : FixtureBase
    {
        [TestMethod]
        public void Should_be_able_to_serialize_all_types()
        {
            //(new TraceEvent()).Serialize().ShouldNotBeNull();
            //System.Windows.MessageBox.s() Activator.CreateInstance(typeof(TraceEvent)).Serialize();

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