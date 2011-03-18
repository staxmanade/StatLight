#& '.\tools\NUnit\nunit-console-x86.exe' /noshadow .\src\StatLight.IntegrationTests\StatLight.IntegrationTests.nunit /run StatLight.IntegrationTests.ProviderTests.MSTest.when_testing_the_runner_with_MSTest_tests.Should_have_pulled_the_DescriptionAttribute_information_out_of_a_test

#.\src\build\bin\Debug\StatLight.exe "-x=http://localhost:31821/StatLight.RemoteIntegrationTestPage.aspx" --UseRemoteTestPage -o=MSTest -v=May2010 -b


#.\src\build\bin\Debug\StatLight.exe "-x=.\src\StatLight.IntegrationTests.Silverlight.LotsOfTests\Bin\Debug\StatLight.IntegrationTests.Silverlight.LotsOfTests.xap"

$build_configuration = 'Debug'
mkdir -Force ".\src\build\bin\$build_configuration\Extensions\" | Out-Null
cp ".\src\Samples\SampleExtension\bin\$build_configuration\SampleExtension.dll" ".\src\build\bin\$build_configuration\Extensions\" -Force

& ".\src\build\bin\$build_configuration\StatLight.exe" "-d=.\src\StatLight.IntegrationTests.Silverlight.OtherTestAssembly\Bin\$build_configuration\StatLight.IntegrationTests.Silverlight.OtherTestAssembly.dll" | Tee-Object -variable output 

if(($output | select-string "Hello From Class1" | Measure).Count -eq 0){
	$output
	throw "Extension did not print expected output"
}