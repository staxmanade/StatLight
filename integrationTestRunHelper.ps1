#& '.\tools\NUnit\nunit-console-x86.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.MSTest.when_testing_the_runner_with_MSTest_tests.Should_have_pulled_the_DescriptionAttribute_information_out_of_a_test

#.\src\build\bin\Debug\StatLight.exe "-x=http://localhost:31821/StatLight.RemoteIntegrationTestPage.aspx" --UseRemoteTestPage -o=MSTest -v=May2010 -b


#.\src\build\bin\Debug\StatLight.exe "-x=.\src\StatLight.IntegrationTests.Silverlight.LotsOfTests\Bin\Debug\StatLight.IntegrationTests.Silverlight.LotsOfTests.xap"

$build_configuration = 'Debug'

#& ".\src\build\bin\$build_configuration\StatLight.exe" "-d=.\src\StatLight.IntegrationTests.Silverlight.OtherTestAssembly\Bin\$build_configuration\StatLight.IntegrationTests.Silverlight.OtherTestAssembly.dll" --webserveronly

#& '.\tools\NUnit\nunit-console-x86.exe' /noshadow .\src\build\bin\debug\StatLight.IntegrationTests.dll /run StatLight.IntegrationTests.ProviderTests.XUnit.when_testing_the_runner_with_xunit_tests.Should_get_passing_test_result
& '.\tools\NUnit\nunit-console-x86.exe' /noshadow .\src\build\bin\debug\StatLight.IntegrationTests.dll /run StatLight.IntegrationTests.when_something_executing_in_silverlight_throws_up_a_modal_MessageBox.Should_only_have_three_results_total_EXCEPT_cant_detect_which_method_to_map_the_message_box_to_so_our_total_is_now_six
