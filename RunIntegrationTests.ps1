#
# Update-TypeData -prependPath NUnitSpecificationExtensions.ps1xml
#
################## Build Configuration
$build_configuration = 'Release'

$buildPath = ".\src\build\bin\$build_configuration"
$ErrorActionPreference = "Stop"
################## 

[System.Reflection.Assembly]::LoadFrom((Get-Item .\tools\NUnit\nunit.framework.dll).FullName) | Out-Null

function Remove-If-Exist {
	param($file)
	if(Test-Path $file)
	{
		Remove-Item $file -Force -ErrorAction SilentlyContinue -Recurse
	}
}

function RunStatLightForVersion {
	param([string]$microsoft_Silverlight_Testing_Version_Name)
	
	$tempFileGuid = ([System.Guid]::NewGuid())
	$scriptFile = ".\temp_statlight-integration-output-$tempFileGuid.ps1"
	Remove-If-Exist $scriptFile
	
	& $buildPath\StatLight.exe "-x=$buildPath\StatLight.Client.For.$microsoft_Silverlight_Testing_Version_Name.Integration.xap" "-v=$microsoft_Silverlight_Testing_Version_Name" "-r=$scriptFile"
	
	[Reflection.Assembly]::LoadWithPartialName("System.Xml.Linq") | Out-Null
	$file = get-item $scriptFile
	$doc = [System.Xml.Linq.XDocument]::Load($file)
	
	$passingTests = $doc.Descendants('test') | where{ $_.Attribute('passed').Value -eq 'true' }
	$passingTests.Count.ShouldEqual(3);

	$ignoredTests = @($doc.Descendants('test') | where{ $_.Attribute('passed').Value -eq 'false' } | where { $_.Value.Contains('Ignoring') })
	$ignoredTests.Count.ShouldEqual(1);

	$failedTests = @($doc.Descendants('test') | where{ $_.Attribute('passed').Value -eq 'false' } | where { ! $_.Value.Contains('Ignoring') })
	$failedTests.Count.ShouldEqual(1);

	Remove-If-Exist $scriptFile
	
	[System.IO.File]::Exists($scriptFile).ShouldBeFalse()
}

RunStatLightForVersion "December2008"
RunStatLightForVersion "March2009"
RunStatLightForVersion "July2009"
RunStatLightForVersion "October2009"
