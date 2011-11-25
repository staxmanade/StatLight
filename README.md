What Is StatLight?
=========================
StatLight is a tool developed for automating the setup, execution, and gathering results of 
Silverlight unit tests. StatLight helps to speed up the feedback cycles while practicing 
TDD/BDD/(insert your test style here) during Silverlight development.


Project Resource Links and Information
=========================
- Source code location: http://github.com/staxmanade/StatLight
- Documentation, Issue tracking, Discussion, etc located: http://statlight.codeplex.com

Where do I get the most current release?
=========================
http://statlight.codeplex.com

How to build?
=========================
StatLight build infrastructure is put together with a number of psake tasks [http://github.com/JamesKovacs/psake].

1. Make sure the location you extract the project is not too far from your drives 
   root - (looks like the project is hitting some msbuild max file path issues)
2. Execute the build of choice.
   Release build ----- You can either execute 
                  - build-Full-Release.bat (compile the solution, runs test suite, creates package release)
                  or
                  - build-Full-Release-skip-tests.bat (compile the solution, creates package release)
                  then
                  - you should find the build artifacts in the .\Release folder.
   Debug build   ----- execute the build-Debug.bat file and you can check out the 
                       artifacts placed in .\src\build...

Build NOTE:
The unit/integration tests throw up quite a few assertion dialogs, and message boxes (Don't close them yourself, unless it hangs for more than 15 seconds). (They _should_ close automatically during the build, however sometimes the integration tests leave one behind. If it does you can close it and everything should be fine.)

Contributors
--
Thanks for contributions from:

[Christopher Bennage](https://github.com/bennage)
[Johannes Rudolph](https://github.com/JohannesRudolph)
[Mike Benza](https://github.com/MikeBenza)
[Remo Gloor](https://github.com/remogloor)
[Steven De Kock](https://github.com/sdekock)