
How to build?
=========================

StatLight build infrastructure is put together with a number of psake tasks [http://code.google.com/p/psake/].


1. start a powershell command window to this directory
2. run psake - 
    Debug build   ----- run ".\psake.ps1"
    Release build ----- run ".\psake.ps1 build-full-release.ps1"
3. all build output is placed in the .\src\build\* directory



TODO:
	- Look at updating to Silverlight 4 Beta
        - In the xap reader - if we determine it's an MSTest xap - figure out what version.
	- Investigate new S.L. toolkit integration build support


DONE:
	- replace powershell zipping util script... (now using DotNetZip - faster than older method)
	- add to build script zip of src on release build
	- Added support UnitDriven testing.
