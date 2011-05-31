#& '.\tools\NUnit\nunit-console-x86.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.MSTest.when_testing_the_runner_with_MSTest_tests.Should_have_pulled_the_DescriptionAttribute_information_out_of_a_test

#.\src\build\bin\Debug\StatLight.exe "-x=http://localhost:31821/StatLight.RemoteIntegrationTestPage.aspx" --UseRemoteTestPage -o=MSTest -v=May2010 -b


#.\src\build\bin\Debug\StatLight.exe "-x=.\src\StatLight.IntegrationTests.Silverlight.LotsOfTests\Bin\Debug\StatLight.IntegrationTests.Silverlight.LotsOfTests.xap"

$build_configuration = 'Debug'

#& ".\src\build\bin\$build_configuration\StatLight.exe" "-d=.\src\StatLight.IntegrationTests.Silverlight.OtherTestAssembly\Bin\$build_configuration\StatLight.IntegrationTests.Silverlight.OtherTestAssembly.dll" --webserveronly

#& ".\src\build\bin\$build_configuration\StatLight.exe" "-x=.\src\StatLight.IntegrationTests.Silverlight.MSTest\Bin\Debug\StatLight.IntegrationTests.Silverlight.MSTest.xap"


#& '.\tools\NUnit\nunit-console-x86.exe' /noshadow .\src\build\bin\debug\StatLight.IntegrationTests.dll /run StatLight.IntegrationTests.ProviderTests.MSTest.when_testing_the_runner_with_MSTest_tests.Should_have_pulled_the_DescriptionAttribute_information_out_of_a_failing_test
