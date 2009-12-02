
namespace StatLight.Core.Reporting.Providers.TeamCity
{
	internal interface ICommandWriter
	{
		void Write(Command command);
	}
}
