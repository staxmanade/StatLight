
namespace StatLight.Core.Reporting.Providers.TeamCity
{
	using System.Collections.Specialized;
	using System.Text;

	public class Command
	{
		public const string CommandStart = "##teamcity[";
		public const string CommandEnd = "]";

		private readonly CommandType commandType;

		internal Command(CommandType commandType)
		{
			this.commandType = commandType;
		}

		public override string ToString()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(CommandStart);
			builder.Append(commandType.ToString());
			for (int i = 0; i < messages.Count; i++)
			{
				WrapCommandKeyValue(builder, messages.GetKey(i), messages[i]);
			}
			builder.Append(CommandEnd);
			return builder.ToString();
		}

		private static void WrapCommandKeyValue(StringBuilder builder, string key, string value)
		{
			builder.Append(" ");
			builder.Append(key);
			builder.Append("=");
			builder.Append("'");
			builder.Append(value);
			builder.Append("'");
		}

		private NameValueCollection messages = new NameValueCollection();

		internal Command AddMessage(string key, string value)
		{
			messages.Add(key, CommandFormatter.FormatValue(value));
			return this;
		}
	}
}
