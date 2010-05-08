using Microsoft.Silverlight.Testing.Harness;
#if MSTestMarch2010
#else
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public class TestExecutionDoNotReportMessageMap : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {
            if (message.MessageType == LogMessageType.TestExecution)
            {
                if (message.Is(TestStage.Finishing)
                    && message.Is(TestGranularity.TestScenario)
                    && message.DecoratorMatches(UnitTestLogDecorator.TestMethodMetadata, v => v is ITestMethod)
                    )
                {
                    return true;
                }


                /*
MessageType=TestExecution
Message=Unit Testing
Decorators:
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, IsUnitTestMessage, ValueType=System.Boolean, True
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, NameProperty, ValueType=System.String, Unit Testing
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, TestGranularity, ValueType=Microsoft.Silverlight.Testing.Harness.TestGranularity, Harness
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, UnitTestHarness, ValueType=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestHarness, Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestHarness
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, TestStage, ValueType=Microsoft.Silverlight.Testing.Harness.TestStage, Starting
                */
                if (message.Is(TestStage.Starting)
                    && message.Is(TestGranularity.Harness)
                    )
                {
                    return true;
                }

                if (message.Is(TestStage.Starting)
                    && message.Is(TestGranularity.TestGroup)
                    )
                {
                    return true;
                }
            }

            /*
MessageType=Error
Message=Exception: Type "Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException" Message "Assert.IsTrue failed. "
Decorators:
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, IsUnitTestMessage, ValueType=System.Boolean, True
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, TestGranularity, ValueType=Microsoft.Silverlight.Testing.Harness.TestGranularity, TestScenario
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, ActualException, ValueType=Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException, Microsoft.VisualStudio.TestTools.UnitTesting.AssertFailedException: Assert.IsTrue failed.
       at Microsoft.VisualStudio.TestTools.UnitTesting.Assert.HandleFail(String assertionName, String message, Object[] parameters)
       at Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(Boolean condition, String message, Object[] parameters)
       at Microsoft.VisualStudio.TestTools.UnitTesting.Assert.IsTrue(Boolean condition)
       at StatLight.IntegrationTests.Silverlight.MSTestTests.this_should_be_a_Failing_test()
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, TestClassMetadata, ValueType=StatLight.Client.Harness.UnitTestProviders.MSTest.TestClass, StatLight.Client.Harness.UnitTestProviders.MSTest.TestClass
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, TestMethodMetadata, ValueType=StatLight.Client.Harness.UnitTestProviders.MSTest.TestMethod, StatLight.Client.Harness.UnitTestProviders.MSTest.TestMethod
             */

            if (message.MessageType == LogMessageType.Error)
            {
                if (message.DecoratorMatches(UnitTestLogDecorator.TestClassMetadata, v => v != null) &&
                    message.DecoratorMatches(UnitTestLogDecorator.TestMethodMetadata, v => v != null))
                {
                    return true;
                }
            }


            /*
MessageType=TestInfrastructure
Message=Unit Test Run
Decorators:
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, IsUnitTestMessage, ValueType=System.Boolean, True
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, TestRunFilter, ValueType=Microsoft.Silverlight.Testing.UnitTesting.Harness.TestRunFilter, Microsoft.Silverlight.Testing.UnitTesting.Harness.TestRunFilter
             */
            if (message.MessageType == LogMessageType.TestInfrastructure)
            {
                if (message.DecoratorMatches(UnitTestLogDecorator.IsUnitTestMessage, v => (bool)v) &&
                    message.DecoratorMatches(UnitTestLogDecorator.TestRunFilter, v => v != null))
                {
                    return true;
                }
            }


            /*
MessageType=TestExecution
Message=TestGroupStatLight.IntegrationTests.Silverlight.MSTest finishing
Decorators:
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, IsUnitTestMessage, ValueType=System.Boolean, True
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, NameProperty, ValueType=System.String, StatLight.IntegrationTests.Silverlight.MSTest
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, TestGranularity, ValueType=Microsoft.Silverlight.Testing.Harness.TestGranularity, TestGroup
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.UnitTesting.Harness.UnitTestLogDecorator, TestAssemblyMetadata, ValueType=StatLight.Client.Harness.UnitTestProviders.MSTest.UnitTestFrameworkAssembly, StatLight.Client.Harness.UnitTestProviders.MSTest.UnitTestFrameworkAssembly
    KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, TestStage, ValueType=Microsoft.Silverlight.Testing.Harness.TestStage, Finishing
             */
            if (message.MessageType == LogMessageType.TestExecution)
            {
                if (message.DecoratorMatches(LogDecorator.TestGranularity, v => ((TestGranularity)v) == TestGranularity.TestGroup) &&
                    message.DecoratorMatches(LogDecorator.TestStage, v => ((TestStage)v) == TestStage.Finishing)
                    )
                {
                    return true;
                }
            }

            /*
MessageType=TestExecution
Message=Unit Testing
Decorators:
KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.UnitTestLogDecorator, IsUnitTestMessage, ValueType=System.Boolean, True
KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, NameProperty, ValueType=System.String, Unit Testing
KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, TestGranularity, ValueType=Microsoft.Silverlight.Testing.Harness.TestGranularity, Harness
KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.UnitTestLogDecorator, UnitTestHarness, ValueType=Microsoft.Silverlight.Testing.Harness.UnitTestHarness, Microsoft.Silverlight.Testing.Harness.UnitTestHarness
KeyType(typeof,string)=Microsoft.Silverlight.Testing.Harness.LogDecorator, TestStage, ValueType=Microsoft.Silverlight.Testing.Harness.TestStage, Finishing
            */
            if (message.Is(TestStage.Finishing)
                && message.Is(TestGranularity.Harness)
                )
            {
                return true;
            }



            return false;
        }

        public ClientEvent Translate(LogMessage message)
        {
            return null;
        }
    }
}