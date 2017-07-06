# Sonic Audio Tools
Tools for editing CriWare file formats (CSB, CPK, ACB and AWB)
## Releases
[On the AppVeyor page.](https://ci.appveyor.com/project/blueskythlikesclouds/sonicaudiotools/build/artifacts)

## Building
1. Clone from [GitHub](https://github.com/blueskythlikesclouds/SonicAudioTools.git) `git clone https://github.com/blueskythlikesclouds/SonicAudioTools.git`
2. Open the solution in Visual Studio. (Visual Studio 2017 or later is required.)
3. Install the missing NuGet packages.
4. Build the solution.

## Projects
For more information about the projects, visit the [wiki](https://github.com/blueskythlikesclouds/SonicAudioTools/wiki) page.
### [SonicAudioLib](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/SonicAudioLib)
Core class library of the solution. Handles the IO work of the other projects.
### [AcbEditor](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/AcbEditor)
A tool to replace audio data in ACB files. Handles both ACBs and AWBs. Old ACB versions which had CPKs for storage are also supported.
### [CsbBuilder](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/CsbBuilder)
A fully functional CSB creator. It's currently able to load any CSB file as long as they contain ADX files. It also always builds in the latest CSB version, which appeared probably around 2010.
### [CsbEditor](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/CsbEditor)
A tool to replace audio data in CSB files. Handles both CSBs and CPKs.

## License
See [LICENSE](https://github.com/blueskythlikesclouds/SonicAudioTools/blob/master/LICENSE)
