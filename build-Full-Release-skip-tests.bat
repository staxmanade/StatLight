powershell -NoProfile -Command "& { Import-Module .\psake.psm1; Invoke-psake .\default.ps1 "build-all, package-release" -parameters @{"build_configuration"='Release';} }"

Pause
