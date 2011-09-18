using System.Globalization;

namespace StatLight.Core.WebServer
{
    public class TestPage
    {
        const string KeyInstanceId = "BB86D193-AD39-494A-AEB7-58F948BA5D93";
        const string KeyWindowless = "9b3a069e-cf56-4338-9bb2-903927acae94";

        private readonly int _instanceId;
        private readonly string _windowless;

        public TestPage(int instanceId, string windowless)
        {
            _instanceId = instanceId;
            _windowless = windowless;
        }

        public override string ToString()
        {
            return Properties.Resources.TestPage
                .Replace(KeyInstanceId, _instanceId.ToString(CultureInfo.InvariantCulture))
                .Replace(KeyWindowless, _windowless)
                ;
        }
    }
}