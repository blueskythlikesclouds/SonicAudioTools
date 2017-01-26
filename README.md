# Sonic Audio Tools
Sonic Audio Tools is a set of tools for editing the audio formats seen in Sonic the Hedgehog games. Currently, it's mostly focused on CRIWARE's audio formats, this means that these tools can be used to edit files from any game, as long as they are CSB, CPK, ACB or AWB.
## Building
1. Clone from [GitHub](https://github.com/blueskythlikesclouds/SonicAudioTools.git) `git clone https://github.com/blueskythlikesclouds/SonicAudioTools.git`
2. Open the solution in Visual Studio. (Visual Studio 2015 or later is required.)
3. Install the missing NuGet packages.
4. Build the solution.

## Projects
### [SonicAudioLib](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/SonicAudioLib)
Core class library of the solution. Handles the IO work of the other projects.
### [AcbEditor](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/AcbEditor)
A tool to replace audio data in ACB files. Handles both ACBs and AWBs. Old ACB versions which had CPKs for storage are also supported.
### [CsbBuilder](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/CsbBuilder)
A fully functional CSB creator. It's currently able to load any CSB file as long as they contain ADX files. It also always builds in the latest CSB version, which appeared probably around 2010.
### [CsbEditor](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Source/CsbEditor)
A tool to replace audio data in CSB files. Handles both CSBs and CPKs.

## Releases
The compiled executables can be found in the [Release](https://github.com/blueskythlikesclouds/SonicAudioTools/tree/master/Release) directory. This directory will be removed when the projects get released under the [Releases](https://github.com/blueskythlikesclouds/SonicAudioTools/releases) page.

## License
See [LICENSE.txt](https://github.com/blueskythlikesclouds/SonicAudioTools/blob/master/LICENSE.txt)
