#### $\color{Red}\Huge{\textsf{THIS REPOSITORY DOES NOT INCLUDE COPYRIGHTED GAME ASSETS!}}$

## TLoZ: LADXHD Updated - Windows / Android / Linux

This fork requires the user to provide the assets from the original v1.0.0 release.<br>
I have created tooling to make migrating everything to the latest version much easier.<br>
The game can be patched to port to Windows (DX11), Windows (OpenGL), Android, and Linux.

- This is a continuation of my [previous fork](https://github.com/BigheadSMZ/Links-Awakening-DX-HD) and here's a link to the [commits](https://github.com/BigheadSMZ/Links-Awakening-DX-HD/commits/master/).
- See the [manual](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated/blob/main/MANUAL.md) to learn more about the game (WIP).
- See the [changelog](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated/blob/main/CHANGELOG.md) for a list of changes from v1.0.0.
- As of v1.1.0, the game is in a really good state and the "feel" is really close to the original game.  
- As of v1.2.0, all obvious bugs have been fixed and features from the [Redux romhack](https://github.com/ShadowOne333/Links-Awakening-Redux) were implemented.
- As of v1.3.0, I consider the work that I've done to be "feature complete" and everything from this point is gravy.
- As of v1.4.0, the gravy train never stopped and much work has been done to make this port more accurate.
- As of v1.5.0, it has evolved into something I never dreamed of. Hundreds of issues fixed with tons of features.
- As of v1.6.0, just about every small detail from the original game has been restored and/or replicated.
- As of v1.7.0, it's been ported to multiple platforms and every single (known) bug since v1.0.0 has been fixed.

## Building / Contributing

I have compiled all this information into a wiki page here: [Building / Contributing](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated/wiki/Building-&-Contributing).

## Patching v1.0.0 (or v1.1.4+) to v1.7.4.

To download the latest update, there is a patcher on the [Releases](https://github.com/BigheadSMZ/Zelda-LA-DX-HD-Updated/releases) page.<br>
If you wish to build the game yourself, see **Personal Build / Publishing**.

- Find the v1.0.0 release originally from itch.io.
- It's a good idea to keep a <ins>backup</ins> of v1.0.0.
- Download the patcher from the releases page.
- Drop it into the base folder of v1.0.0 (or v1.1.4+).
- Open the patcher. Select the desired **Platform** and **Target**.
- Press the "Patch" button. It will take a bit to finish.
- When it is done, the patcher can be deleted.

The patcher can run on Linux and macOS via Wine, and will generate ready to use binaries / apps for these platforms when running on the target operating system. Please check [LINUX.md](LINUX.md) and [MACOS.md](MACOS.md) for detailed instructions.

### Silent Mode: Command Line Patching

The patcher supports silent mode for automated installations and scripts:

```
LADXHD.Patcher.exe --silent
```

| Option | Description |
|--------|-------------|
| `--silent`, `-s` | Run without GUI prompts |
| `--platform <value>` | Target platform (default: windows)<br>Values: windows, android, linux-x86, linux-arm64, macos-x86, macos-arm64 |
| `--graphics <value>` | Target graphics API<br>Default: directx (windows), opengl (all others)<br>Values: directx, opengl |
| `--help`, `-h` | Show help message |

| Exit Code | Meaning |
|-----------|---------|
| 0 | Success |
| 1 | Game executable not found |
| 2 | Patching failed |
| 3 | Invalid arguments |

## About This Repository

A few years back, an anonymous user posted a PC Port of Link's Awakening on itch.io built with MonoGame. It wasn't long before the game was taken down, fortunately the release contained the source code. This is a continuation of that PC Port but with the assets stripped away to avoid copyright issues. 

This section explains the files and folders found in the base of this respository.<br>
All software is Windows only aside from the game which has been ported to Android and Linux.

- **assets_original**: This is where the **"Content"** and **"Data"** folders from v1.0.0 should go.
- **assets_patches**: Contains xdelta3 patches that are the difference of assets from v1.0.0 to the latest updates.
- **ladxhd_game_source_code**: Source code for The Legend of Zelda: Link's Awakening DX HD.
- **ladxhd_migrate_source_code**: Source code for the migration tool which can apply/create assets patches.
- **ladxhd_modmaker_source_code**: Source code for the modmaker which can create mod installers.
- **ladxhd_patcher_source_code**: Source code for the patcher to update the game to v1.7.4.
- **LADXHD_Migrater.exe**: This is the migration tool used to apply or create patches to the assets.
- **Unblock-All-Files.ps1**: This script can be used to unblock all files automatically for Visual Studio.

The game is built with the latest version of [MonoGame](https://monogame.net/).

## About This Fork

I am a terrible programmer, but I have a love for this game. A ton of forks popped up, some with fixes, but nowhere were they all centralized. This fork attempted to find and implement all the various fixes and improvements spread across the other various forks. Once that was done, I started tackling the issues from the repository this was cloned from. And after that was done, I worked on anything else I could find that would make the game feel more like the original game.

Feel free to commit any potential fixes as a PR. There are no coding guidelines and any style is welcome as long as the code either fixes something broken or makes the game behave closer to the original. But do try to at least keep it neat.
