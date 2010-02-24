namespace StatLight.Core.Reporting.Providers.Xml
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Xml.Linq;
	using StatLight.Core.Reporting.Messages;

	public class XmlReport
	{
		private const string TrueString = "True";
		private const string FalseString = "False";

		private readonly TestReport _report;
		private readonly string _testXapFileName;

		public XmlReport(TestReport report, string testXapFileName)
		{
			this._report = report;
			this._testXapFileName = testXapFileName;
		}

		public void WriteXmlReport(string outputFilePath)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.Write(GetXmlReport());
				writer.Close();
			}
		}

		public string GetXmlReport()
		{
            throw new NotImplementedException();
            var root =
					new XElement("StatLightTestResults",
						new XAttribute("xapFileName", _testXapFileName),
						new XAttribute("total", this._report.TotalResults),
						new XAttribute("ignored", this._report.TotalIgnored),
						new XAttribute("failed", this._report.TotalFailed),
						new XAttribute("dateRun", this._report.DateTimeRunCompleted.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture)),

                        new XElement("tests",
								(from x in _report.Results
								 select GetResult(x))
                                //(from x in _report.OtherMessages
                                // select GetOtherMessage(x))
						)
					);
			return root.ToString();
		}

        //private static object GetOtherMessage(MobilOtherMessageType result)
        //{
        //    if (result.IsIgnoreMessage())
        //    {
        //        return GetTestCaseElement(
        //            new XAttribute("passed", "False"),
        //            new XElement("failureMessage", new XCData(result.Message))
        //            );
        //    }

        //    return null;
        //}

		private static XElement GetResult(MobilScenarioResult result)
		{
			Func<MobilScenarioResult, string> formatName = resultX => "{0}.{1}".FormatWith(resultX.TestClassName, resultX.TestName);

			Func<string, MobilScenarioResult, IEnumerable<XAttribute>> getCommonAttributes = (passFail, resultX) => new[]
				{
					new XAttribute("name", formatName(resultX)),
					new XAttribute("passed", passFail),
					new XAttribute("timeToComplete", resultX.TimeToComplete.ToString()),
				};

			switch (result.Result)
			{
				case TestOutcome.Passed:
					return GetTestCaseElement(
								getCommonAttributes(TrueString, result)
							);
				case TestOutcome.Failed:
					return GetTestCaseElement(
								getCommonAttributes(FalseString, result),
								new XElement("failureMessage", new XCData(result.ExceptionMessage))
								);
			}

			return null;
		}

		private static XElement GetTestCaseElement(params object[] attributes)
		{
			return new XElement("test", attributes);
		}
	}
}
