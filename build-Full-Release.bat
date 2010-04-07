powershell -Command "& { Import-Module .\psake.psm1; Invoke-psake .\default.ps1 build-Full-Release -properties @{"build_configuration"='Release';} }"

Pause
