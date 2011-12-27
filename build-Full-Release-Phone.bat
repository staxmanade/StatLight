powershell -NoProfile -Command "& { Import-Module .\psake.psm1; Invoke-psake .\default.ps1 build-full-Release-Phone -parameters @{"build_configuration"='Release';} }"

Pause
