include build-tasks.ps1

properties {
	$build_configuration = 'Debug'
}

Task default -depends build-Debug-Fast
