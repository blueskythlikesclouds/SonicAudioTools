# Sonic Audio Tools
Tools for editing CriMw / CriWare file formats. (CSB / CPK / ACB / AWB)
## Releases
[On the AppVeyor page.](https://ci.appveyor.com/project/blueskythlikesclouds/sonicaudiotools/build/artifacts)
AppVeyor builds every commit automatically, so you can get the executables from the link above.
Stable builds are planned to be published [here.](https://github.com/blueskythlikesclouds/SonicAudioTools/releases)

## Building
If you still wish to build the solution yourself, this is what you should do:
1. Clone from [GitHub](https://github.com/blueskythlikesclouds/SonicAudioTools.git) `git clone https://github.com/blueskythlikesclouds/SonicAudioTools.git`
2. Open the solution in Visual Studio. (Visual Studio 2017 or later is required.)
3. Install the missing NuGet packages.
4. Build the solution.

## Projects
To see more information about projects, visit the [wiki](https://github.com/blueskythlikesclouds/SonicAudioTools/wiki).
### [Sonic Audio Library](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/SonicAudioLib)
Main class library of the solution. Contains classes for IO and file formats.
### [ACB Editor](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/AcbEditor)
Audio data replacer for ACB / AWB files. Supports really old ACB files as well, which stored CPK files instead of currently used AWB files. Usage is explained in the program.
NOTE: It doesn't extract cue names so every audio will have its id on extracted file name.
### [CSB Builder](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/CsbBuilder)
CSB file importer/builder. Allows you to fully edit CSB files, preview your changes, add/remove cues, etc.
### [CSB Editor](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/CsbEditor)
Audio data replacer for CSB / CPK files. It works like ACB Editor and is a lot simpler than CSB Builder.

## License
Sonic Audio Tools uses the MIT license, meaning you're able to use and modify the code freely, as long as you include the copyright notice.
For more details, see [LICENSE](https://github.com/blueskythlikesclouds/SonicAudioTools/blob/master/LICENSE)
