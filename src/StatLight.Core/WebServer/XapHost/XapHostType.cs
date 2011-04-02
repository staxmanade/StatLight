namespace StatLight.Core.WebServer.XapHost
{
    public enum XapHostType
    {
        MSTestDecember2008,
        MSTestMarch2009,
        MSTestJuly2009,
        MSTestOctober2009,
        MSTestNovember2009,
        MSTestMarch2010,
        MSTestApril2010,
        MSTestMay2010,
        MSTestFeb2011,

        UnitDrivenDecember2009,
        XunitContribApril2011
    }

    public enum MicrosoftTestingFrameworkVersion
    {
        //Obsolete versions
        December2008 = 1,
        March2009 = 2,

        //Supported versions
        July2009 = 3,
        October2009 = 4,
        November2009 = 5,

        March2010 = 6,
        April2010 = 7,

        May2010 = 8,

        Feb2011 = 9,

        //FYI: go update StatLightClientXapNames.Default value when changing the default runner...
    }
}