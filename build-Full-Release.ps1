include build-tasks.ps1

properties {
	$build_configuration = 'Release'
}

Task default -depends build-Full-Release
