
#if July2009 || October2009 || November2009
#else

using System;
using Microsoft.Silverlight.Testing.Harness;
using CompositeWorkItem = Microsoft.Silverlight.Testing.Harness.CompositeWorkItem;
using UnitTestHarness = Microsoft.Silverlight.Testing.Harness.UnitTestHarness;

namespace StatLight.Client.Harness.Hosts.MSTest
{
    public class StatLightUnitTestHarness : UnitTestHarness
    {
        private CompositeWorkItem _harnessTasks;

        public StatLightUnitTestHarness()
        {
            CreateHarnessTasks();
        }

        private void CreateHarnessTasks()
        {
            _harnessTasks = new CompositeWorkItem();
            _harnessTasks.Complete += HarnessComplete;
        }

        private void HarnessComplete(object sender, EventArgs e)
        {
            _harnessTasks = null;
        }

        protected override bool RunNextStep()
        {
            ProcessLogMessages();
            if (RootCompositeWorkItem == null)
            {
                throw new InvalidOperationException("UnitTestHarness_RunNextStep_NoCompositeWorkItemsExist");
            }
            return RootCompositeWorkItem.Invoke();
        }

        public override void RestartRunDispatcher()
        {
            if (this._harnessTasks == null)
                this.CreateHarnessTasks();

            //this.RunDispatcher = new WebBrowserTick(RunNextStep);
            this.RunDispatcher = new FastRunDispatcher(this.RunNextStep, this.Dispatcher);
            this.RunDispatcher.Complete += this.RunDispatcherComplete;
            this.RunDispatcher.Run();
        }
    }
}
#endif
