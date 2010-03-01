
namespace StatLight.Core.Reporting.Providers.TeamCity
{
    public interface ICommandWriter
    {
        void Write(Command command);
        void Write(string message);
    }
}
