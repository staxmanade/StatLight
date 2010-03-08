
How to build?
=========================

StatLight build infrastructure is put together with a number of psake tasks [http://code.google.com/p/psake/].


1. start a powershell (or dos cmd) window in this directory
2. run psake - 
    Debug build   ----- run ".\psake.ps1"
    Release build ----- run ".\psake.ps1 build-full-release.ps1"
3. all build output is placed in the .\src\build\* directory

