properties {
	# NOTE: the $build_configuration property must be set prior to calling anything in here... default.ps1 for example
	$build_configuration =  ?? $build_configuration 'Debug'

	$not_build_configuration = Get-Not-Build-Configuration
	$build_dir = ".\src\build\bin\$build_configuration"
	$program_files_dir = Get-x86-ProgramFiles-Location
#	$silverlight_core_assemblies_location = "$program_files_dir\Microsoft Silverlight\3.0.40818.0"
	$silverlight_core_assemblies_location = "$program_files_dir\Reference Assemblies\Microsoft\Framework\Silverlight\v3.0"
	$silverlight_libraries_client_assemblies = "$program_files_dir\Microsoft SDKs\Silverlight\v3.0\Libraries\Client"
#C:\Program Files\Reference Assemblies\Microsoft\Framework\Silverlight\v3.0	
	$statlight_xap_for_prefix = "StatLight.Client.For" 

	$release_dir = 'Release'
	
	$nunit_console_path = 'Tools\NUnit\nunit-console-x86.exe'

	$statLightSourcesFilePrefix = 'StatLight.Sources.'
	$core_assembly_path = "$build_dir\StatLight.Core.dll"
	$test_assembly_path = "src\StatLight.Core.Tests\bin\x86\$build_configuration\StatLight.Core.Tests.dll"
	$integration_test_assembly_path = "$build_dir\StatLight.IntegrationTests.dll"
	
	# All of the versions that this script will create compatible 
	# builds of the statlight silverlight client for...
	#
	# How to add a new version
	#  - 1. Create the new version in the libs path .\libs\Silverlight\Microsoft\<version>\*.dll
	#  - 2. Add the version below 
	#  - 3. Add the version to the MicrosoftTestingFrameworkVersion enum in the project
	$microsoft_silverlight_testing_versions = @(
			'March2010'

#			'December2008'
#			'March2009'
			'July2009'
			'October2009'
			'November2009'
		)
}

function ?? {
    $result = $null
	$i = 0;
	while($args[$i] -eq $null)
	{
		$i = $i + 1;
	}
	
	return $args[$i]
}

Task default -depends build-debug

Task build-Debug -depends build-all, test-all {
}

Task build-full-Release -depends build-all, test-all, package-release {
}

if(!(Test-Path ('variable:hasLoadedNUnitSpecificationExtensions')))
{
	echo 'loading NUnitSpecificationExtensions...'
	Update-TypeData -prependPath .\tools\PowerShell\NUnitSpecificationExtensions.ps1xml
	[System.Reflection.Assembly]::LoadFrom((Get-Item .\tools\NUnit\nunit.framework.dll).FullName) | Out-Null
	$global:hasLoadedNUnitSpecificationExtensions = $true;
}


# Is this a Win64 machine regardless of whether or not we are currently 
# running in a 64 bit mode 
function global:Test-Win64Machine() {
    return test-path (join-path $env:WinDir "SysWow64")
}

function global:rename-file-extensions {
	param([string]$itemsPath, [string]$fromExtension, [string]$toExtension)
	Get-Item "$itemsPath" | foreach{ Move-Item $_.FullName $_.FullName.Replace($fromExtension, $toExtension) }
}

function get-assembly-version() {
	param([string] $file)
	
	$fileStream = ([System.IO.FileInfo] (Get-Item $file)).OpenRead()
	$assemblyBytes = new-object byte[] $fileStream.Length
	$fileStream.Read($assemblyBytes, 0, $fileStream.Length) | Out-Null #out null this because this function should only return the version & this call was outputting some garbage number
	$fileStream.Close()
	$version = [System.Reflection.Assembly]::Load($assemblyBytes).GetName().Version;
	
	#format the version and output it...
	$version
}

function get-formatted-assembly-version() {
	param([string] $file)
	
	$version = get-assembly-version $file
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
	Remove-If-Exists $scriptFile

	Write-Host ''
	Write-Host '*********** Executing Command ***********'
	Write-Host $cmd
	Write-Host '*****************************************'
	Write-Host ''
	Write-Host ''

	$cmd >> $scriptFile
	exec { . $scriptFile } 'Doh - Compile failed...'
	Remove-If-Exists $scriptFile
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
		".\lib\Silverlight\MEF\System.ComponentModel.Composition.dll"
		".\lib\Silverlight\MEF\System.ComponentModel.Composition.Initialization.dll"
		".\src\build\bin\$build_configuration\StatLight.Client.Harness.dll"
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
		".\lib\Silverlight\MEF\System.ComponentModel.Composition.dll"
		".\lib\Silverlight\MEF\System.ComponentModel.Composition.Initialization.dll"
		".\src\build\bin\$build_configuration\StatLight.Client.Harness.dll"
	)

	$references;
}

function global:compile-StatLight-MSTestHost {
	param([string]$microsoft_Silverlight_Testing_Version_Name, [string]$microsoft_silverlight_testing_version_path, [string]$outAssemblyName)

#	$resources = @(
#		"src\StatLight.Client.Harness\obj\$build_configuration\StatLight.Client.Harness.g.resources"
#	)

	$references = StatLightReferences $microsoft_Silverlight_Testing_Version_Name

	$sourceFiles = @(
		"src\AssemblyInfo.cs"
	)

	$sourceFiles += Get-ChildItem 'src\StatLight.Client.Harness.MSTest\' -recurse `
		| where{$_.Extension -like "*.cs"} `
		| foreach {$_.FullName} `
		| where{!$_.Contains($not_build_configuration)}

echo $sourceFiles

	$extraCompilerFlags = [string]''
	if("$microsoft_Silverlight_Testing_Version_Name" -eq 'March2010')
	{
		$extraCompilerFlags = 'MSTestMarch2010'
	}
	$buildConfigToUpper = $build_configuration.ToUpper();
	echo $buildConfigToUpper
	$options = @(
		'/noconfig',
		'/nowarn:1701`,1702',
		'/nostdlib+',
		'/errorreport:prompt',
		'/platform:anycpu',
		'/warn:4'
		"/define:$buildConfigToUpper``;TRACE``;SILVERLIGHT``;$extraCompilerFlags``;$microsoft_Silverlight_Testing_Version_Name",
		'/debug-',
		'/optimize+',
		'/keyfile:src\StatLight.snk',
		"/out:$outAssemblyName",
		'/target:library'
	)

	$cmd = Build-Csc-Command -options $options -sourceFiles $sourceFiles -references $references -resources $resources
	
	Execute-Command-String $cmd
}

function global:compile-StatLight-MSTestHostIntegrationTests {
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

	$buildConfigToUpper = $build_configuration.ToUpper();
	echo $buildConfigToUpper
	$options = @(
		'/noconfig',
		'/nowarn:1701`,1702',
		'/nostdlib+',
		'/errorreport:prompt',
		'/platform:anycpu',
		'/warn:4'
		"/define:$buildConfigToUpper``;TRACE``;SILVERLIGHT``;$microsoft_Silverlight_Testing_Version_Name",
		'/debug-',
		'/optimize+',
		'/keyfile:src\StatLight.snk',
		"/out:$outAssemblyName",
		'/target:library'
	)

	$cmd = Build-Csc-Command -options $options -sourceFiles $sourceFiles -references $references -resources $resources
	
	Execute-Command-String $cmd
}

function global:Remove-If-Exists {
	param($file)
	if(Test-Path $file)
	{
		echo "Deleting file $file"
		Remove-Item $file -Force -Recurse
	}
}

function global:Build-And-Package-StatLight-MSTest {
	param([string]$microsoft_Silverlight_Testing_Version_Name)
	
	$statlightBuildFilePath = "$build_dir\StatLight.Client.Harness.MSTest.dll"
	
	compile-StatLight-MSTestHost $microsoft_Silverlight_Testing_Version_Name ".\lib\Silverlight\Microsoft\$microsoft_Silverlight_Testing_Version_Name" $statlightBuildFilePath
	
	Assert ( test-path $statlightBuildFilePath) "File should exist $statlightBuildFilePath"
	
	$zippedName = "$build_dir\$statlight_xap_for_prefix.$microsoft_Silverlight_Testing_Version_Name.zip"

	$newAppManifestFile = "$(($pwd).Path)\src\build\AppManifest.xaml"

	# the below chunk will add the StatLight.Client.Harness.MSTest to the AppManifest
	$appManifestContent = [xml](get-content ".\src\StatLight.Client.Harness\Bin\$build_configuration\AppManifest.xaml")
#	$assemblyPart = $appManifestContent.CreateElement("AssemblyPart")
	#$assemblyPart.SetAttribute("Name", "http://schemas.microsoft.com/client/2007/deployment", "StatLight.Client.Harness.MSTest")
#	$assemblyPart.SetAttribute("Name", "StatLight.Client.Harness.MSTest")
#	$assemblyPart.SetAttribute("Source", "StatLight.Client.Harness.MSTest.dll")
#	$appManifestContent.Deployment."Deployment.Parts".AppendChild($assemblyPart)
$extraStuff = '
    <AssemblyPart x:Name="StatLight.Client.Harness.MSTest" Source="StatLight.Client.Harness.MSTest.dll" />
    <AssemblyPart x:Name="Microsoft.Silverlight.Testing" Source="Microsoft.Silverlight.Testing.dll" />
    <AssemblyPart x:Name="Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight" Source="Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll" />
'
	$appManifestContent.Deployment."Deployment.Parts".InnerXml = $appManifestContent.Deployment."Deployment.Parts".InnerXml + $extraStuff
	$appManifestContent.Save($newAppManifestFile);


#cat $newAppManifestFile
#throw 'test'
	$zipFiles = StatLightReferences $microsoft_Silverlight_Testing_Version_Name `
				| Where-Object { -not $_.Contains($silverlight_core_assemblies_location) } `
				| foreach{ Get-Item $_}
	$zipFiles += @(
					Get-Item $newAppManifestFile
					Get-Item $statlightBuildFilePath
				)
	Create-Xap $zippedName $zipFiles
}

function global:Build-And-Package-StatLight-MSTest-IntegrationTests {
	param([string]$microsoft_Silverlight_Testing_Version_Name)

	$dllName = 'StatLight.IntegrationTests.Silverlight.MSTest.dll'
	$dllPath = "$build_dir\$dllName"

	Remove-If-Exists $dllPath

	compile-StatLight-MSTestHostIntegrationTests $microsoft_Silverlight_Testing_Version_Name .\lib\Silverlight\Microsoft\$microsoft_Silverlight_Testing_Version_Name $dllPath
	
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
	Remove-If-Exists $newZipFileName
	$filesToInclude | Zip-Files-From-Pipeline $newZipFileName
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

function GetTemporaryXmlFile()
{
	$tempFileGuid = ([System.Guid]::NewGuid())
	$scriptFile = ("$($pwd.Path)\temp_statlight-integration-output-$tempFileGuid.xml")
	Remove-If-Exists $scriptFile
	$scriptFile
}

function Execute-MSTest-Version-Acceptance-Tests {
	param([string]$microsoft_Silverlight_Testing_Version_Name)
	
	$scriptFile = GetTemporaryXmlFile;
	
	& "$build_dir\StatLight.exe" "-x=$build_dir\StatLight.Client.For.$microsoft_Silverlight_Testing_Version_Name.Integration.xap" "-v=$microsoft_Silverlight_Testing_Version_Name" "-r=$scriptFile" "-b"
	
	[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null
	$file = get-item $scriptFile
	$doc = [System.Xml.Linq.XDocument]::Load($file)

	$passedCount = 0;
	$ignoredCount = 0;
	$failedCount = 0;
	$systemGeneratedfailedCount = 0;

	foreach($test in $doc.Descendants('test'))
	{
		$resultTypeValue = $test.Attribute('resulttype').Value
		if($resultTypeValue -eq 'Passed')
		{
			$passedCount = $passedCount + 1;
		}
		elseif($resultTypeValue -eq 'Ignored')
		{
			$ignoredCount = $ignoredCount + 1;
		}
		elseif($resultTypeValue -eq 'Failed')
		{
			$failedCount = $failedCount + 1;
		}
		elseif($resultTypeValue -eq 'SystemGeneratedFailure')
		{
			$systemGeneratedfailedCount = $systemGeneratedfailedCount + 1;
		}
		else
		{
			throw "Unknown ResultType [$resultTypeValue]"
		}
	
	}
	
	$systemGeneratedfailedCount.ShouldEqual(1);
	if($build_configuration -eq 'Debug')
	{
		$failedCount.ShouldEqual(3);
	}
	else
	{
		$failedCount.ShouldEqual(2);
	}
	$passedCount.ShouldEqual(5);
	$ignoredCount.ShouldEqual(1);
	
	AssertXmlReportIsValid $scriptFile
	
	#added sleep to wait for file system to loose the lock on the file so we can delete it
	[System.Threading.Thread]::Sleep(500);
	Remove-If-Exists $scriptFile
	#rm "$($pwd.Path)\temp_statlight-integration-output-*"
}

function AssertXmlReportIsValid([string]$scriptFile)
{
		$statLightCoreFilePath = (Get-Item "$build_dir\StatLight.Core.dll").FullName
	$fileStream = ([System.IO.FileInfo] (Get-Item $statLightCoreFilePath)).OpenRead()
	$assemblyBytes = new-object byte[] $fileStream.Length
	$fileStream.Read($assemblyBytes, 0, $fileStream.Length) | Out-Null #out null this because this function should only return the version & this call was outputting some garbage number
	$fileStream.Close()
	[System.Reflection.Assembly]::Load($assemblyBytes) | Out-Null;
	
	$errs = $null
	$passed = [StatLight.Core.Reporting.Providers.Xml.XmlReport]::ValidateSchema($scriptFile, [ref] $errs)
	if($passed -eq $false)
	{
		foreach($msg in $errs)
		{
			Write-Host $msg -ForegroundColor Red
		}
		throw "Failed the xmlreport schema validation."
	}
}

function LoadZipAssembly {
	if(!(Test-Path ('variable:hasLoadedIonicZipDll')))
	{
		$scriptDir = ".\lib\Desktop\DotNetZip"
		$zippingAssembly = (Get-Item "$scriptDir\Ionic.Zip.Reduced.dll").FullName

		echo "loading Zipping assembly [$zippingAssembly]..."
		[System.Reflection.Assembly]::LoadFrom($zippingAssembly) | Out-Null
		$global:hasLoadedIonicZipDll = $true;
	}
}

function Zip-Files-From-Pipeline
{
    param([string]$zipfilename, [bool] $addAllZipsToRoot = $true)
	
	BEGIN {
		
		LoadZipAssembly;
	
		$pwdPath = $PWD.Path;
	
		$zipfile =  new-object Ionic.Zip.ZipFile
	}
	PROCESS {
		[string] $fileFullName;
		$isDirectory = $false
		$file = $_

		if($file.GetType() -eq [string])
		{
			$fileFullName = $file
		}
		elseif($file.GetType() -eq [System.IO.FileInfo])
		{
			$fileInfo = [System.IO.FileInfo]$file
			$fileFullName = $fileInfo.FullName
		}
		elseif($file.GetType() -eq [System.IO.DirectoryInfo])
		{
			$directoryToZip = "$($file.FullName)".Replace("\", "\\")
			echo "Zipping Directory $directoryToZip"
			$zipfile.AddDirectory($directoryToZip, "")
			echo "Directory added..."
			$isDirectory = $true
		}
		else
		{
			echo "DEBUG: PSIsContainer = $($file.PSIsContainer)"
			echo "DEBUG: PSPath = $($file.PSPath)"
			echo "DEBUG: FullName = $($file.FullName)"
			
			$unknownTypeName = $file.GetType();
			throw "Zip-Files-From-Pipeline - Can only work with [string] and [System.IO.FileInfo] items. Cannot use unknown type - [$unknownTypeName]"
		}
		
		if(!$isDirectory)
		{
			if($addAllZipsToRoot -eq $true)
			{
				$zipfile.AddFile($fileFullName, "") | Out-Null
			}
			else
			{
				if($fileFullName.Substring(0, $pwdPath.Length) -eq $pwdPath)
				{
					$hackedFileName = $fileFullName.Substring($pwdPath.Length);
				}
				else
				{
					$hackedFileName = $fileFullName
				}
		
				$hackedFileName = Split-Path $hackedFileName -Parent
	
				$zipfile.AddFile($fileFullName, $hackedFileName) | Out-Null
			}
		}
	}	
	END {
		echo "Saving zip to $pwdPath\$zipfilename"
		$zipfile.Save("$pwdPath\$zipfilename")
		$zipfile.Dispose()
	}
}

#########################################
#
# build script misc pre-build tasks
#
#########################################

Task initialize { 

	#
	#Value	Meaning
	#0	Turn script tracing off.
	#1	Trace script lines as they are executed.
	#2	Trace script lines, variable assignments, function calls, and scripts.
	#Set-PSDebug -Trace 0
	
	$ErrorActionPreference = "Stop"
	
	echo "running build with configuration of $build_configuration"
	echo "running build with configuration of $build_dir"
	
	
	  
#	foreach($propertyBlock in $script:context.Peek().properties) 
#	{
#		. $propertyBlock
#	}

#	foreach($key in $script:context.Peek().properties.keys)
#	{
#		echo "***"
#		echo $key
#		#set-item -path "variable:\$key" -value $properties.$key | out-null
#	}

#	$props = $script:context.Peek().properties
#	& $props
 
}

Task clean-build {
	#-ErrorAction SilentlyContinue --- because of the *.vshost which is locked and we can't delete.
	Remove-If-Exists $build_dir\*
	Remove-If-Exists $release_dir
	
	mkdir $build_dir -Force
}

#########################################
#
# Building/Compilation tasks
#
#########################################

Task compile-StatLight-MSTestHostVersions {

	rename-file-extensions -itemsPath "$build_dir\*.xap" -fromExtension ".xap" -toExtension ".zip"

	$microsoft_silverlight_testing_versions | foreach { Build-And-Package-StatLight-MSTest $_ }

	rename-file-extensions -itemsPath "$build_dir\*.zip" -fromExtension ".zip" -toExtension ".xap"
}

Task compile-StatLight-MSTestHostVersionIntegrationTests {
	
	rename-file-extensions -itemsPath "$build_dir\*.xap" -fromExtension ".xap" -toExtension ".zip"

	$microsoft_silverlight_testing_versions | foreach { Build-And-Package-StatLight-MSTest-IntegrationTests $_ }

	rename-file-extensions -itemsPath "$build_dir\*.zip" -fromExtension ".zip" -toExtension ".xap"
}

Task compile-Solution {
	$msbuild = 'C:\Windows\Microsoft.NET\Framework\v3.5\MSBuild.exe'
	exec { . $msbuild .\src\StatLight.sln /t:Rebuild /p:Configuration=$build_configuration /p:Platform=x86 } 'msbuild failed on StatLight.sln'
}


#########################################
#
# Unit/Integration tests
#
#########################################


Task test-core {
	exec { & $nunit_console_path $test_assembly_path } 'Unit Tests Failed'
}

Task test-integrationTests {
	exec { & $nunit_console_path $integration_test_assembly_path /noshadow } 'test-integrationTests Failed'
}


Task test-tests-in-other-assembly {
	
	$scriptFile = GetTemporaryXmlFile;
	
	& "$build_dir\StatLight.exe" "-x=.\src\StatLight.IntegrationTests.Silverlight\Bin\$build_configuration\StatLight.IntegrationTests.Silverlight.xap" "-t=OtherAssemblyTests" "-r=$scriptFile"	"-b"
	
	[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null
	$file = get-item $scriptFile
	$doc = [System.Xml.Linq.XDocument]::Load($file)

	$passedCount = 0;
	$ignoredCount = 0;
	$failedCount = 0;
	$systemGeneratedfailedCount = 0;
	
	foreach($test in $doc.Descendants('test'))
	{
		$resultTypeValue = $test.Attribute('resulttype').Value

		if($resultTypeValue -eq 'Passed')
		{
			$passedCount = $passedCount + 1;
		}
		elseif($resultTypeValue -eq 'Ignored')
		{
			$ignoredCount = $ignoredCount + 1;
		}
		elseif($resultTypeValue -eq 'Failed')
		{
			$failedCount = $failedCount + 1;
		}
		elseif($resultTypeValue -eq 'SystemGeneratedFailure')
		{
			$systemGeneratedfailedCount = $systemGeneratedfailedCount + 1;
		}
		else
		{
			throw "Unknown ResultType [$resultTypeValue]"
		}
		
	}
	
	$passedCount.ShouldEqual(2);
	$ignoredCount.ShouldEqual(1);
	$failedCount.ShouldEqual(1);
	$systemGeneratedfailedCount.ShouldEqual(0);

	AssertXmlReportIsValid $scriptFile
}

Task test-client-harness-tests {
	exec { & "$build_dir\StatLight.exe" "-x=.\src\StatLight.Client.Tests\Bin\$build_configuration\StatLight.Client.Tests.xap" "-b" } 'test-client-harness-tests Failed'
}

Task test-all-mstest-version-acceptance-tests {
	$microsoft_silverlight_testing_versions | foreach { Execute-MSTest-Version-Acceptance-Tests $_ }
}

#########################################
#
# Release packaging
#
#########################################

Task clean-release {
	if(-not (Test-Path $release_dir))
	{
		New-Item $release_dir -type directory -force | Out-Null
	}
	Remove-If-Exists "$release_dir\*" | Out-Null
}

Task package-zip-project-sources-snapshot {
	# files to zip ... (Get all files & run all the files thought this exclusion logic)...
	$files = (Get-ChildItem -Path .\ -Recurse | `
		where { !($_.PSIsContainer) } | `
		where { !($_.GetType() -eq [System.IO.DirectoryInfo])} | `
		where { ($_.FullName -notlike "*\obj\*") } | `
		where { ($_.FullName -notlike "*\bin\*") } | `
		where { ($_.FullName -notlike "*\src\build\*") } | `
		where { ($_.FullName -notlike "*\_Resharper*") } | `
		where { ($_.FullName -notlike "*\.git*") } | `
		where { ($_.FullName -notlike "*TestResult.xml") } | `
		where { ($_.FullName -notlike "*.sln.cache") } | `
		where { ($_.FullName -notlike "*.user") } | `
		where { ($_.FullName -notlike "*\Release\*") })

	$versionString = get-formatted-assembly-version $core_assembly_path
	$sourceZipFile = "$statLightSourcesFilePrefix$versionString.zip"
	
	echo "Remove-If-Exists $sourceZipFile"
	Remove-If-Exists $sourceZipFile | Out-Null

	#DEBUG: $files | foreach{ echo "$($_.GetType()) - $_" }

	echo "Zipping up the source files"
	$files | Zip-Files-From-Pipeline $sourceZipFile $false | Out-Null
	
	echo "Moving the zipped source into the $release_dir folder."
	mv $sourceZipFile $release_dir\$sourceZipFile -Force | Out-Null
}

Task package-release -depends clean-release, package-zip-project-sources-snapshot {
	$versionBuildPath = "$release_dir\$(get-formatted-assembly-version $core_assembly_path)"

	$expectedFilesToInclude = @(
			'Ionic.Zip.Reduced.dll'
			'Microsoft.Silverlight.Testing.License.txt'
			'StatLight.Client.For.July2009.xap'
			'StatLight.Client.For.March2010.xap'
			'StatLight.Client.For.November2009.xap'
			'StatLight.Client.For.October2009.xap'
			'StatLight.Core.dll'
			'StatLight.EULA.txt'
			'StatLight.exe'
			'StatLight.Sources.v*'
		)

	$knownFilesToExclude = @(
		'Microsoft.Silverlight.Testing.dll'
		'Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll'
		'nunit.framework.dll'
		'StatLight.Client.Harness.dll'
		'StatLight.IntegrationTests.dll'
		'StatLight.IntegrationTests.Silverlight.MSTest.dll'
	)

	$filesToCopyFromBuild = @(
		Get-ChildItem "$build_dir\*.xap" | where{ -not $_.FullName.Contains("Integration") }
		Get-ChildItem $build_dir\* -Include *.dll, *.txt, StatLight.exe #-exclude nunit.framework.dll, StatLight.Client.Harness.dll, Microsoft.Silverlight.Testing.dll, Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll, StatLight.IntegrationTests.dll, StatLight.IntegrationTests.Silverlight.MSTest.dll
		Get-ChildItem ".\StatLight.EULA.txt"
	)

	New-Item -Path $versionBuildPath -ItemType directory | Out-Null

	Move-Item (Get-ChildItem $release_dir\$statLightSourcesFilePrefix*) "$versionBuildPath\$($_.Name)"
	$filesToCopyFromBuild | foreach{ Copy-Item $_ "$versionBuildPath\$($_.Name)"  }

	$knownFilesToExclude | foreach{Remove-Item $versionBuildPath\$_ }
	
	$unexpectedFilesInReleaseDir = (Get-ChildItem $versionBuildPath -Exclude $expectedFilesToInclude)
	if($unexpectedFilesInReleaseDir.Count)
	{
		$unexpectedFilesInReleaseDir
		throw "Unexpected files in release directory"
	}
	
	foreach($expectedFile in $expectedFilesToInclude)
	{
		Assert (Test-Path "$versionBuildPath\$expectedFile") "Could not find expected file $expectedFile in release directory $versionBuildPath"
	}
	
	Assert ($assertAllFilesWereFound.Count -eq $assertAllFilesWereFound.Count) "Not all the necessary files were found in the release directory - $expectedFilesToInclude"
	
	$version = get-assembly-version $core_assembly_path

	$release_zip_path = "$release_dir\StatLight.v$($version.Major).$($version.Minor).zip"
	
	Get-Item $versionBuildPath | Zip-Files-From-Pipeline $release_zip_path
}


Task help -Description "Prints out the different tasks within the StatLIght build engine." {
	Write-Documentation
}

Task ? -Description "Prints out the different tasks within the StatLIght build engine." {
	Write-Documentation
}

Task test-all -depends test-core, test-client-harness-tests, test-integrationTests, test-all-mstest-version-acceptance-tests, test-tests-in-other-assembly {
}

Task build-all -depends clean-build, initialize, compile-Solution, compile-StatLight-MSTestHostVersions, compile-StatLight-MSTestHostVersionIntegrationTests {
}
