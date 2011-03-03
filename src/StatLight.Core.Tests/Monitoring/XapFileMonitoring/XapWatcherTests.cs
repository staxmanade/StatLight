using System.IO;
using NUnit.Framework;
using StatLight.Core.Events;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Monitoring;

namespace StatLight.Core.Tests.Monitoring.XapFileMonitoring
{
    namespace XapMonitorTests
    {
        [TestFixture]
        public class when_initializing_the_XapWatcher : using_a_random_temp_file_for_testing
        {

            [Test]
            public void Should_be_able_to_initialize_the_XapWatcher_by_specifying_the_xap_path()
            {
                var xapWatcher = new XapFileBuildChangedMonitor(base.TestEventPublisher, PathToTempXapFile);
            }

            [Test]
            public void Should_throw_a_FileNotFoundException_if_the_given_file_does_not_exist()
            {
                typeof(FileNotFoundException).ShouldBeThrownBy(() => new XapFileBuildChangedMonitor(base.TestEventPublisher, PathToTempXapFile + "badpath"));
            }

        }

        public class when_the_XapWatcher_has_been_initialized_with_a_file_and_it_has_been_loaded : using_a_random_temp_file_for_testing
        {
            protected XapFileBuildChangedMonitor _xapWatcher;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                _xapWatcher = new XapFileBuildChangedMonitor(base.TestEventPublisher, PathToTempXapFile);
            }
        }

        [TestFixture]
        public class should_1 :
            when_the_XapWatcher_has_been_initialized_with_a_file_and_it_has_been_loaded
        {
            [Test]
            public void Should_raise_file_refreshed_event_when_existing_file_changes()
            {
                bool wasXapFileRefreshed = false;

                TestEventSubscriptionManager.AddListener<XapFileBuildChangedServerEvent>(e => wasXapFileRefreshed = true);

                base.replace_test_file();

                // refresh event doesn't fire in time so we wait
                System.Threading.Thread.Sleep(50);

                wasXapFileRefreshed.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class should_2 :
            when_the_XapWatcher_has_been_initialized_with_a_file_and_it_has_been_loaded
        {
            [Test]
            public void when_simulating_a_build_the_changed_event_should_only_be_raised_once_in_a_short_amount_of_time()
            {

                int raisedCount = 0;
                TestEventSubscriptionManager.AddListener<XapFileBuildChangedServerEvent>(e =>
                {
                    raisedCount++;
                });

                // when building in Visual Studio, it appeared that there were
                // 5 different changed events thrown.
                base.replace_test_file();
                base.replace_test_file();
                base.replace_test_file();
                base.replace_test_file();
                base.replace_test_file();

                // refresh event doesn't fire in time so we wait
                System.Threading.Thread.Sleep(50);

                raisedCount.ShouldEqual(1);
            }
        }
    }
}
