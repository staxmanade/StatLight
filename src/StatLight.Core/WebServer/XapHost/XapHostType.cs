namespace StatLight.Core.WebServer.XapHost
{
    public enum XapHostType
    {
        MSTest2008December,
        MSTest2009March,
        MSTest2009July,
        MSTest2009October,
        MSTest2009November,

        MSTest2010March,
        MSTest2010April,
        MSTest2010May,

        MSTest2011Feb,
        MSTest2011June,

        UnitDriven2009December,
        XunitContrib2011April
    }

    public enum MicrosoftTestingFrameworkVersion
    {
        //Obsolete versions
        //MSTest2008December = 1,
        //MSTest2009March = 2,

        //Supported versions
        MSTest2009July = 3,
        MSTest2009October = 4,
        MSTest2009November = 5,

        MSTest2010March = 6,
        MSTest2010April = 7,
        MSTest2010May = 8,

        MSTest2011Feb = 9,
        MSTest2011June = 10,

        //FYI: go update StatLightClientXapNames.Default value when changing the default runner...
    }
}