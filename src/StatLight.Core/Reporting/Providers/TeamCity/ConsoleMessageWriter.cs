namespace StatLight.Core.Reporting.Providers.TeamCity
{
    internal sealed class ConsoleCommandWriter : ICommandWriter
    {
        public void Write(Command command)
        {
            Write(command.ToString());
        }

        public void Write(string message)
        {
            if (!string.IsNullOrEmpty(message))
                System.Console.WriteLine(message);
        }
    }
}