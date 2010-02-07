using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using StatLight.Core.Common;

namespace StatLight.Core.Tests.Common
{
	public class TestLogger : LoggerBase, ILogger
	{
		public TestLogger()
			: base(LogChatterLevels.None)
		{ }

		public bool InformationMessageLogged { get; private set; }
		public override void Information(string message)
		{
			if (ShouldLog(LogChatterLevels.Information))
				InformationMessageLogged = true;
		}

		public bool WarningMessageLogged { get; private set; }
		public override void Warning(string message)
		{
			if (ShouldLog(LogChatterLevels.Warning))
				WarningMessageLogged = true;
		}

		public bool ErrorMessageLogged { get; private set; }
		public override void Error(string message)
		{
			if (ShouldLog(LogChatterLevels.Error))
				ErrorMessageLogged = true;
		}

		public bool DebugMessageLogged { get; private set; }
		public override void Debug(string message)
		{
			if (ShouldLog(LogChatterLevels.Debug))
				DebugMessageLogged = true;
		}

		public override void Debug(string message, bool writeNewLine)
		{
			throw new NotImplementedException();
		}
	}

	public class with_a_logger : FixtureBase
	{
		protected TestLogger logger;

		protected LogChatterLevels LogChatterLevel;

		protected override void Before_all_tests()
		{
			base.Before_all_tests();

			logger = new TestLogger();
			logger.LogChatterLevel = LogChatterLevel;

			LogAll(logger);
		}

		private void LogAll(ILogger logger)
		{
			logger.Error("Error Logged");
			logger.Warning("Warning Logged");
			logger.Information("Information Logged");
			logger.Debug("Debug Logged");
		}
	}

	[TestFixture]
	public class when_verifying_an_Error_only_LogChatterLevel : with_a_logger
	{
		protected override void Before_all_tests()
		{
			LogChatterLevel = LogChatterLevels.Error;

			base.Before_all_tests();
		}

		[Test]
		public void should_log_an_error_message()
		{
			logger.ErrorMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_not_log_a_warning_message()
		{
			logger.WarningMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_an_information_message()
		{
			logger.InformationMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_a_debug_message()
		{
			logger.DebugMessageLogged.ShouldBeFalse();
		}
	}
	[TestFixture]
	public class when_verifying_a_warning_only_LogChatterLevel : with_a_logger
	{
		protected override void Before_all_tests()
		{
			LogChatterLevel = LogChatterLevels.Warning;

			base.Before_all_tests();
		}

		[Test]
		public void should_not_log_an_error_message()
		{
			logger.ErrorMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_log_a_warning_message()
		{
			logger.WarningMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_not_log_an_information_message()
		{
			logger.InformationMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_a_debug_message()
		{
			logger.DebugMessageLogged.ShouldBeFalse();
		}

	}

	public class when_verifying_a_information_only_LogChatterLevel : with_a_logger
	{
		protected override void Before_all_tests()
		{
			LogChatterLevel = LogChatterLevels.Information;

			base.Before_all_tests();
		}

		[Test]
		public void should_not_log_an_error_message()
		{
			logger.ErrorMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_a_warning_message()
		{
			logger.WarningMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_log_an_information_message()
		{
			logger.InformationMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_not_log_a_debug_message()
		{
			logger.DebugMessageLogged.ShouldBeFalse();
		}
	}

	public class when_verifying_a_debug_only_LogChatterLevel : with_a_logger
	{
		protected override void Before_all_tests()
		{
			LogChatterLevel = LogChatterLevels.Debug;

			base.Before_all_tests();
		}

		[Test]
		public void should_not_log_an_error_message()
		{
			logger.ErrorMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_a_warning_message()
		{
			logger.WarningMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_an_information_message()
		{
			logger.InformationMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_log_a_debug_message()
		{
			logger.DebugMessageLogged.ShouldBeTrue();
		}
	}

	public class when_verifying_a_Full_LogChatterLevel : with_a_logger
	{
		protected override void Before_all_tests()
		{
			LogChatterLevel = LogChatterLevels.Full;

			base.Before_all_tests();
		}

		[Test]
		public void should_log_an_error_message()
		{
			logger.ErrorMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_log_a_warning_message()
		{
			logger.WarningMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_log_an_information_message()
		{
			logger.InformationMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_log_a_debug_message()
		{
			logger.DebugMessageLogged.ShouldBeTrue();
		}
	}

	public class when_verifying_a_None_LogChatterLevel : with_a_logger
	{
		protected override void Before_all_tests()
		{
			LogChatterLevel = LogChatterLevels.None;

			base.Before_all_tests();
		}

		[Test]
		public void should_not_log_an_error_message()
		{
			logger.ErrorMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_a_warning_message()
		{
			logger.WarningMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_an_information_message()
		{
			logger.InformationMessageLogged.ShouldBeFalse();
		}

		[Test]
		public void should_not_log_a_debug_message()
		{
			logger.DebugMessageLogged.ShouldBeFalse();
		}
	}

	public class when_verifying_a_Information_or_Warning_or_Error_LogChatterLevel : with_a_logger
	{
		protected override void Before_all_tests()
		{
			LogChatterLevel = LogChatterLevels.Information | LogChatterLevels.Warning | LogChatterLevels.Error;
			base.Before_all_tests();
		}

		[Test]
		public void should_log_an_error_message()
		{
			logger.ErrorMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_log_a_warning_message()
		{
			logger.WarningMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_log_an_information_message()
		{
			logger.InformationMessageLogged.ShouldBeTrue();
		}

		[Test]
		public void should_not_log_a_debug_message()
		{
			logger.DebugMessageLogged.ShouldBeFalse();
		}
	}
}
