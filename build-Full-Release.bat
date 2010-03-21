powershell -Command "& { Import-Module .\psake.psm1; Invoke-psake .\build-tasks.ps1 build-Full-Release -parameters @{"build_configuration"='Release';} }"

Pause