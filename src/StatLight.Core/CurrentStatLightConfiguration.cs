using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StatLight.Core.Configuration;

namespace StatLight.Core
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface ICurrentStatLightConfiguration : IEnumerable<StatLightConfiguration>
    {
        void SetCurrentTo(string xapPath);
        StatLightConfiguration Current { get; }
        bool MoveNext();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public sealed class CurrentStatLightConfiguration : ICurrentStatLightConfiguration
    {
        private readonly List<StatLightConfiguration> _configurations;
        private int _currentIndex;

        public CurrentStatLightConfiguration(IEnumerable<StatLightConfiguration> configurations)
        {
            _configurations = configurations.ToList();
            Reset();
        }

        public void SetCurrentTo(string xapPath)
        {
            for (int i = 0; i < _configurations.Count; i++)
            {
                var config = _configurations[i];
                if (config.Server.XapToTestPath.Equals(xapPath))
                {
                    _currentIndex = i;
                }
            }
        }

        public bool MoveNext()
        {
            if ((_currentIndex + 1 >= _configurations.Count))
            {
                return false;
            }
            ++_currentIndex;
            return true;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public StatLightConfiguration Current
        {
            get { return _configurations[_currentIndex]; }
        }

        public IEnumerator<StatLightConfiguration> GetEnumerator()
        {
            return _configurations.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}