
namespace StatLight.Core.Reporting.Providers.TeamCity
{
	internal sealed class ConsoleCommandWriter : ICommandWriter
	{
		public void Write(Command command)
		{
			System.Console.WriteLine(command.ToString());
		}
	}
}