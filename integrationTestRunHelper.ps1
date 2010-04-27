#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.when_something_executing_in_silverlight_throws_up_a_modal_MessageBox
#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.when_something_executing_in_silverlight_throws_up_a_debug_assertion_dialog
#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.MSTest.when_testing_the_runner_with_MSTest_tests
#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.MSTest.when_testing_the_runner_with_MSTest_tests_filtered_by_certain_methods
#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.NUnit.when_testing_the_runner_with_NUnit_tests
#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.NUnit.when_testing_the_runner_with_NUnit_tests_filtered_by_certain_methods

#.\src\build\bin\Debug\StatLight.exe "-x=.\src\StatLight.IntegrationTests.Silverlight.UnitDriven\Bin\Debug\StatLight.IntegrationTests.Silverlight.UnitDriven.xap" -b
#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.UnitDriven.when_testing_the_runner_with_UnitDriven_tests_filtered_by_certain_methods
#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.UnitDriven.when_testing_the_runner_with_UnitDriven_tests

#& "src\build\bin\Debug\StatLight.exe" "-x=.\src\StatLight.Client.Tests\Bin\Debug\StatLight.Client.Tests.xap" "-o=MSTest" "-b" 

#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ServiceReferenceClientConfigTests.Should_be_able_to_detect_and_use_the_ServiceReferenceClientConfig_file

#& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.XUnit.when_testing_the_runner_with_Xunit_tests_filtered_by_certain_methods
& 'C:\Program Files\NUnit 2.5.3\bin\net-2.0\nunit-console.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.XUnit.when_testing_the_runner_with_xunit_tests
