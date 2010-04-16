What Is StatLight?
=========================
StatLight is a tool developed for automating the setup, execution, and gathering results of 
Silverlight unit tests. StatLight helps to speed up the feedback cycles while practicing 
TDD/BDD/(insert your test style here) during Silverlight development.


Project Resource Links and Information
=========================
Source code location: http://github.com/staxmanade/StatLight
Documentation, Issue tracking, Discussion, etc located: http://statlight.codeplex.com


How to build?
=========================
StatLight build infrastructure is put together with a number of psake tasks [http://github.com/JamesKovacs/psake].

1. Make sure the location you extract the project is not too far from your drive's 
   root - (looks like the project is hitting some msbuild max file path issues)
2. Execute the build of choice.
    Release build ----- execute the build-Full-Release.bat and you should find all 
	                    the build artifacts in the .\Release folder.
    Debug build   ----- execute the build-Debug.bat file and you can check out the 
	                    artifacts placed in .\src\build...

Couple things to note during a build.
	1. The unit/integration tests throw up quite a few assertion dialogs, and message boxes (Don't 
	   close them yourself, unless it hangs for more than 15 seconds). (They _should_ close automatically during the build)
