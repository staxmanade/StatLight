
TODO:

1. update README with build information etc...
2. Hello



How to build?
=========================

StatLight build infrastructure is put together with a number of psake tasks [http://code.google.com/p/psake/].


1. start a powershell command window to this directory
2. run psake - 
    Debug build   ----- run ".\psake.ps1"
    Release build ----- run ".\psake.ps1 build-full-release.ps1"
3. all output is built to the .\src\build\* directory


for reference...
castle - build... http://svn.castleproject.org:8080/svn/castle/trunk/How%20to%20build.txt