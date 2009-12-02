include tools\PowerShell\zip.ps1

properties {
	$not_build_configuration = Get-Not-Build-Configuration
	$build_dir = ".\src\build\bin\$build_configuration"
	$program_files_dir = Get-x86-ProgramFiles-Location
	$silverlight_core_assemblies_location = "$program_files_dir\Microsoft Silverlight\3.0.40818.0"
	$silverlight_libraries_client_assemblies = "$program_files_dir\Microsoft SDKs\Silverlight\v3.0\Libraries\Client"
	$statlight_xap_for_prefix = "StatLight.Client.For" 

	$release_dir = 'Release'
	$release_zip_path = "$release_dir\StatLight.zip"
	
	$nunit_exe = 'Tools\NUnit\nunit-console-x86.exe'

	$test_assembly_path = "src\StatLight.Core.Tests\bin\x86\$build_configuration\StatLight.Core.Tests.dll"
	$integration_test_assembly_path = "src\StatLight.IntegrationTests\bin\x86\$build_configuration\StatLight.IntegrationTests.dll"
	
	$microsoft_silverlight_testing_versions = @('December2008', 'March2009', 'July2009', 'October2009')	
	#$microsoft_silverlight_testing_versions = @('July2009')	
}

Task build-start -depends clean, writeProperties, nantBuildProject, buildStatLightSolution, buildStatLight, buildStatLightIntegrationTests
{
}

Task build-full-Release -depends build-start, run-tests, run-integrationTests, createReleasePackage
{
}

Task build-Debug -depends build-start, run-tests, run-integrationTests
{
}

Task build-Debug-Fast -depends build-start, run-tests
{
}



# Is this a Win64 machine regardless of whether or not we are currently 
# running in a 64 bit mode 
function global:Test-Win64Machine() {
    return test-path (join-path $env:WinDir "SysWow64")
}

function global:Rename-Extensions {
	param([string]$itemsPath, [string]$fromExtension, [string]$toExtension)
	Get-Item "$itemsPath" | foreach{ Move-Item $_.FullName $_.FullName.Replace($fromExtension, $toExtension) }
}

function get-assembly-version()
{
	param([string] $file)
	
	$assembly = [System.Reflection.Assembly]::LoadFrom($file)
	$version = $assembly.GetName().Version;
	"v$($version.Major).$($version.Minor).$($version.Build).$($version.Revision)"
}

function global:Build-Csc-Command {
	param([array]$options, [array]$sourceFiles, [array]$references, [array]$resources)
	
	$csc = 'C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe'

	# can't say I'm doing delimeters correctly, but seems to work ???
	$delim = [string]""""
	
	$opts = $options

	if($references.Count -gt 0)
	{
		$opts += '/reference:' + $delim + [string]::Join($delim + ' /reference:' + $delim, $references) + $delim
	}

	if($resources.Count -gt 0)
	{
		$opts += '/resource:' + $delim + [string]::Join($delim + ' /resource:' + $delim, $resources) + $delim
	}
	
	if($sourceFiles.Count -gt 0)
	{
		$opts += [string]::Join(' ', $sourceFiles)
	}
	
	$cmd = [string]::Join(" ", $options)
	$cmd = $csc + " " + $opts
	$cmd;
}

function global:Execute-Command-String {
	param([string]$cmd)
	
	# this drove me crazy... all I wanted to do was execute
	# something like this (excluding the [])
	#
	# [& $csc $opts] OR [& $cmd]
	#
	# however couldn't figure out the correct powershell syntax...
	# But I was able to execute it if I wrote the string out to a 
	# file and executed it from there... would be nice to not 
	# have to do that.

	$tempFileGuid = ([System.Guid]::NewGuid())
	$scriptFile = ".\temp_build_csc_command-$tempFileGuid.ps1"
	Remove-If-Exist $scriptFile

	Write-Host ''
	Write-Host '*********** Executing Command ***********'
	Write-Host $cmd
	Write-Host '*****************************************'
	Write-Host ''
	Write-Host ''

	$cmd >> $scriptFile
	& $scriptFile
	Remove-If-Exist $scriptFile
}

function global:StatLightReferences {
	param([string]$microsoft_silverlight_testing_version_name)
	
	$references = @(
		"$silverlight_core_assemblies_location\mscorlib.dll",
		"$silverlight_core_assemblies_location\System.dll",
		"$silverlight_core_assemblies_location\System.Core.dll",
		"$silverlight_core_assemblies_location\System.Net.dll",
		"$silverlight_core_assemblies_location\System.Runtime.Serialization.dll",
		"$silverlight_core_assemblies_location\System.ServiceModel.dll",
		"$silverlight_core_assemblies_location\System.Windows.dll",
		"$silverlight_core_assemblies_location\System.Windows.Browser.dll",
		"$silverlight_core_assemblies_location\System.Xml.dll",
		"$silverlight_libraries_client_assemblies\System.Windows.Controls.dll",
		"$silverlight_libraries_client_assemblies\System.Xml.Linq.dll",
		"$silverlight_libraries_client_assemblies\System.Xml.Serialization.dll",
		".\lib\Silverlight\Microsoft\$microsoft_silverlight_testing_version_name\Microsoft.Silverlight.Testing.dll"
		".\lib\Silverlight\Microsoft\$microsoft_silverlight_testing_version_name\Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"
	)
	$references;
}

function global:StatLightIntegrationTestsReferences {
	param([string]$microsoft_silverlight_testing_version_name)
	
	$references = @(
		"$silverlight_core_assemblies_location\mscorlib.dll",
		"$silverlight_core_assemblies_location\System.dll",
		"$silverlight_core_assemblies_location\System.Core.dll",
		"$silverlight_core_assemblies_location\System.Net.dll",
		"$silverlight_core_assemblies_location\System.Runtime.Serialization.dll",
		"$silverlight_core_assemblies_location\System.ServiceModel.dll",
		"$silverlight_core_assemblies_location\System.Windows.dll",
		"$silverlight_core_assemblies_location\System.Windows.Browser.dll",
		"$silverlight_core_assemblies_location\System.Xml.dll",
		"$silverlight_libraries_client_assemblies\System.Windows.Controls.dll",
		"$silverlight_libraries_client_assemblies\System.Xml.Linq.dll",
		"$silverlight_libraries_client_assemblies\System.Xml.Serialization.dll",
		".\lib\Silverlight\Microsoft\$microsoft_silverlight_testing_version_name\Microsoft.Silverlight.Testing.dll"
		".\lib\Silverlight\Microsoft\$microsoft_silverlight_testing_version_name\Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"
	)

	$references;
}

function global:CompileStatLight {
	param([string]$microsoft_Silverlight_Testing_Version_Name, [string]$microsoft_silverlight_testing_version_path, [string]$outAssemblyName)

	$resources = @(
		"src\StatLight.Client.Silverlight\obj\$build_configuration\StatLight.Client.Silverlight.g.resources"
	)

	$references = StatLightReferences $microsoft_Silverlight_Testing_Version_Name

	$sourceFiles = @(
		"src\AssemblyInfo.cs",
		"src\StatLight.Core\CommonExtensions.cs",
		"src\StatLight.Core\Reporting\Messages\LogMessageType.cs",
		"src\StatLight.Core\Reporting\Messages\MobilOtherMessageType.cs",
		"src\StatLight.Core\Reporting\Messages\MobilScenarioResult.cs",
		"src\StatLight.Core\Reporting\Messages\TestOutcome.cs",
		"src\StatLight.core\UnitTestProviders\UnitTestProviderTypes.cs",
		"src\StatLight.Core\WebServer\StatLightServiceRestApi.cs",
		"src\StatLight.Core\WebServer\TestRunConfiguration.cs"
	)

	$sourceFiles += Get-ChildItem 'src\StatLight.Client.Silverlight\' -recurse `
		| where{$_.Extension -like "*.cs"} `
		| foreach {$_.FullName} `
		| where{!$_.Contains($not_build_configuration)}

	$options = @(
		'/noconfig',
		'/nowarn:1701`,1702',
		'/nostdlib+',
		'/errorreport:prompt',
		'/platform:anycpu',
		'/warn:4'
		"/define:$build_configuration``;TRACE``;SILVERLIGHT``;$microsoft_Silverlight_Testing_Version_Name",
		'/debug-',
		'/optimize+',
		'/keyfile:src\StatLight.snk',
		"/out:$outAssemblyName",
		'/target:library'
	)

	$cmd = Build-Csc-Command -options $options -sourceFiles $sourceFiles -references $references -resources $resources
	
	Execute-Command-String $cmd
}

function global:CompileStatLightIntegrationTests {
	param([string]$microsoft_Silverlight_Testing_Version_Name, [string]$microsoft_silverlight_testing_version_path, [string]$outAssemblyName)

	$resources = @(
		"src\StatLight.IntegrationTests.Silverlight.MSTest\obj\$build_configuration\StatLight.IntegrationTests.Silverlight.MSTest.g.resources"
	)

	$references = StatLightIntegrationTestsReferences $microsoft_Silverlight_Testing_Version_Name

	$sourceFiles = @( #no manual files...
	)

	$sourceFiles += Get-ChildItem 'src\StatLight.IntegrationTests.Silverlight.MSTest\' -recurse `
		| where{$_.Extension -like "*.cs"} `
		| foreach {$_.FullName} `
		| where{!$_.Contains($not_build_configuration)}

	$options = @(
		'/noconfig',
		'/nowarn:1701`,1702',
		'/nostdlib+',
		'/errorreport:prompt',
		'/platform:anycpu',
		'/warn:4'
		"/define:$build_configuration``;TRACE``;SILVERLIGHT``;$microsoft_Silverlight_Testing_Version_Name",
		'/debug-',
		'/optimize+',
		'/keyfile:src\StatLight.snk',
		"/out:$outAssemblyName",
		'/target:library'
	)

	$cmd = Build-Csc-Command -options $options -sourceFiles $sourceFiles -references $references -resources $resources
	
	Execute-Command-String $cmd
}

function global:Remove-If-Exist {
	param($file)
	if(Test-Path $file)
	{
		Remove-Item $file -Force -ErrorAction SilentlyContinue -Recurse
	}
}


function global:Build-And-Package-StatLight {
	param([string]$microsoft_Silverlight_Testing_Version_Name)
	
	$statlightBuildFilePath = "$build_dir\StatLight.Client.Silverlight.dll"
	
	CompileStatLight $microsoft_Silverlight_Testing_Version_Name ".\lib\Silverlight\Microsoft\$microsoft_Silverlight_Testing_Version_Name" $statlightBuildFilePath
	
	$zippedName = "$build_dir\$statlight_xap_for_prefix.$microsoft_Silverlight_Testing_Version_Name.zip"

	$zipFiles = StatLightReferences $microsoft_Silverlight_Testing_Version_Name `
				| Where-Object { -not $_.Contains($silverlight_core_assemblies_location) } `
				| foreach{ Get-Item $_}
	$zipFiles += @(
					Get-Item ".\src\StatLight.Client.Silverlight\Bin\$build_configuration\AppManifest.xaml"
					Get-Item $statlightBuildFilePath
				)
	Create-Xap $zippedName $zipFiles
}

function global:Build-And-Package-StatLight-IntegrationTests {
	param([string]$microsoft_Silverlight_Testing_Version_Name)

	$dllName = 'StatLight.IntegrationTests.Silverlight.MSTest.dll'
	$dllPath = "$build_dir\$dllName"

	Remove-If-Exist $dllPath

	CompileStatLightIntegrationTests $microsoft_Silverlight_Testing_Version_Name .\lib\Silverlight\Microsoft\$microsoft_Silverlight_Testing_Version_Name $dllPath
	
	$zippedName = "$build_dir\StatLight.Client.For.$microsoft_Silverlight_Testing_Version_Name.Integration.zip"
	$zipFiles = StatLightIntegrationTestsReferences $microsoft_Silverlight_Testing_Version_Name `
				| Where-Object { -not $_.Contains($silverlight_core_assemblies_location) } `
				| foreach{ Get-Item $_}
				$zipFiles += @(
					Get-Item "src\StatLight.IntegrationTests.Silverlight.MSTest\Bin\$build_configuration\AppManifest.xaml"
					Get-Item $dllPath
				)

	Create-Xap $zippedName $zipFiles
}

function global:Create-Xap {
	param($newZipFileName, $filesToInclude)
	Remove-If-Exist $newZipFileName
	$filesToInclude | Add-Zip $newZipFileName
	Move-Item $newZipFileName $newZipFileName.Replace(".zip", ".xap")
}

function global:Get-x86-ProgramFiles-Location {
	$program_files_dir = 'C:\Program Files'	
	if(Test-Win64Machine)
	{
		$program_files_dir = 'C:\Program Files (x86)'
	}
	$program_files_dir;
}

function global:Is-Release-Build {
	if($build_configuration.Equals('Release'))
	{
		$return = $true
	}
	else
	{
		$return = $false
	}
	$return;
}

function global:Get-Not-Build-Configuration {
	if(Is-Release-Build)
	{
		$not_build_configuration = 'Debug'
	}
	else
	{
		$not_build_configuration = 'Release'
	}
	$not_build_configuration;
}


Task writeProperties { 

	#
	#Value	Meaning
	#0	Turn script tracing off.
	#1	Trace script lines as they are executed.
	#2	Trace script lines, variable assignments, function calls, and scripts.
	Set-PSDebug -Trace 0
	
	$ErrorActionPreference = "Stop"
	
	Dump-Properties
}

Task help {
	Dump-Tasks
}

Task clean {
	#-ErrorAction SilentlyContinue --- because of the *.vshost which is locked and we can't delete.
	Remove-Item $build_dir\* -Force -ErrorAction SilentlyContinue
	Remove-Item $release_dir -Force -ErrorAction SilentlyContinue -Recurse
	
	mkdir $build_dir -Force
}

Task nantBuildProject {

#	if(Is-Release-Build)
#	{
#		& .\tools\NAnt\nant.exe build-release
#	}
#	else
#	{
#		& .\tools\NAnt\nant.exe build
#	}
}

Task init-release {

}

Task buildStatLight {

	Rename-Extensions -itemsPath "$build_dir\*.xap" -fromExtension ".xap" -toExtension ".zip"

	$microsoft_silverlight_testing_versions | foreach { Build-And-Package-StatLight $_ }

	Rename-Extensions -itemsPath "$build_dir\*.zip" -fromExtension ".zip" -toExtension ".xap"
}

Task buildStatLightIntegrationTests {
	
	Rename-Extensions -itemsPath "$build_dir\*.xap" -fromExtension ".xap" -toExtension ".zip"

	$microsoft_silverlight_testing_versions | foreach { Build-And-Package-StatLight-IntegrationTests $_ }

	Rename-Extensions -itemsPath "$build_dir\*.zip" -fromExtension ".zip" -toExtension ".xap"
}

Task createReleasePackage {
	if(-not (Test-Path $release_dir))
	{
		New-Item $release_dir -type directory -force
	}
	Remove-If-Exist "$release_dir\*"


	$filesToInclude = @(
		Get-ChildItem "$build_dir\*.xap" | where{ -not $_.FullName.Contains("Integration") }
		Get-ChildItem $build_dir\* -Include *.dll, *.txt, StatLight.exe
		Get-ChildItem ".\StatLight.EULA.txt"
	)
	
	$dllFile = Get-Item '.\src\build\bin\Debug\StatLight.Core.dll'
	$versionBuildPath = "$release_dir\$(get-assembly-version $dllFile.FullName)"
	New-Item -Path $versionBuildPath -ItemType directory
	$filesToInclude | foreach{ Copy-Item $_ "$versionBuildPath\$($_.Name)"  }
	Get-Item $versionBuildPath | Add-Zip $release_zip_path
}

Task buildStatLightSolution {
	$msbuild = 'C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe'
	& $msbuild .\src\StatLight.sln /t:Rebuild /p:Configuration=$build_configuration /p:Platform=x86
	if($LastExitCode)
	{
		throw 'msbuild failed on StatLight.sln'
	}
}

Task run-tests {
	& $nunit_exe $test_assembly_path

	if($LastExitCode)
	{
		throw 'Unit Tests Failed'
	}
}

Task run-integrationTests {
	& $nunit_exe $integration_test_assembly_path /noshadow

	if($LastExitCode)
	{
		throw 'Integration Tests Failed'
	}
}
