[![Mod Assistant](https://cdn.assistant.moe/images/ModAssistant/Icons/Banner.svg?v=5)](https://github.com/Assistant/ModAssistant/releases/latest)
[![Download here!](https://cdn.assistant.moe/images/ModAssistant/Icons/Download.svg)](https://github.com/Assistant/ModAssistant/releases/latest)

Mod Assistant is a PC mod installer for Beat Saber. It uses mods from [BeatMods](https://beatmods.com/).

* [Features](#Features)
* [Usage](#Usage)
* [Themes](#Themes)
  * [Custom Themes](#Custom-Themes)
  * [Built In](#Built-In)
  * [Packaged `.mat` Files](#Packaged-mat-Files)
  * [Loose Folder Themes](#Loose-Folder-Themes)
  * [Overriding Themes](#Overriding-Themes)
* [Common Issues](#Common-Issues)


## Features

Mod Assistant boasts a rich feature set, some of which include:
* Dependency resolution
* Installed mod detection
* Mod uninstallation
* OneClick&trade; Install support
* Complex theming engine
* Localization support
* Headpats and Hugs

## Usage
Download the newest installer from the release section and run it. This application auto-updates when launched, there is no need to download a new release each time.

1. **Run the game at least once before trying to mod the game!** This applies to reinstalling your game too. All mods are moved into an `Old X.X.X Plugins` folder on first launch to avoid version mismatches, so make sure to do this before installing mods on a fresh version.
2. Once that's done, simply check off the mods that you wish to install and click the <kbd>Install or Update</kbd> button. Likewise, click the <kbd>Uninstall</kbd> button to remove any mods.
3. Mods are installed to `IPA/Pending` until the game is run. Boot the game to complete mod installation.


## Themes
<details>
    <summary><b>Light</b></summary>
    <div>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light/Intro.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light/Mods.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light/About.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light/Options.png" /></p>
    </div>
</details>

<details>
    <summary><b>Dark</b></summary>
    <div>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Dark/Intro.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Dark/Mods.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Dark/About.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Dark/Options.png" /></p>
    </div>
</details>

<details>
    <summary><b>BSMG</b></summary>
    <div>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/BSMG/Intro.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/BSMG/Mods.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/BSMG/About.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/BSMG/Options.png" /></p>
    </div>
</details>

<details>
    <summary><b>Light Pink</b></summary>
    <div>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light Pink/Intro.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light Pink/Mods.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light Pink/About.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light Pink/Options.png" /></p>
    </div>
</details>

<details>
    <summary><b>Your own!</b></summary>
    <div>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Custom/Intro.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Custom/Mods.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Custom/About.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Custom/Options.png" /></p>
    </div>
</details>

### Custom Themes
Custom themes are located in a folder called `Themes` in the same folder as `ModAssistant.exe`. Mod Assistant can load themes from one of three sources.

### Built In
These come with the program and you can't change them, however you can overwrite them by creating one of the other two theme types with the same name.

If you have a particularly popular theme, you can submit a [Pull Request](https://github.com/Assistant/ModAssistant/pulls) to add your theme as a built-in theme.

### Packaged `.mat` Files
These are pre-packaged theme files. Under the hood they are renamed`.zip` files, and the file structure is the same as that of `Folders` themes. These will be overwritten by `Folders` themes with the same name. 
To create one follow the instructions on `Folders` themes, and zip the files up into a zip archive, and rename it to `<themeName>.mat`.

### Loose Folder Themes
These will overwrite all other themes, and are loaded from a folder named after the theme. There are 4 types of files you can include:

* `Theme.xaml` 
  * This file determines the colors and styling of the theme.
  * The filename isn't important, but the `.xaml` file extension is.
  * To see an example of this file press the <kbd>Export Template</kbd> button in the `Options` page. It will create a folder in `Themes` called `Ugly Kulu-Ya-Ku`. You can open that file to use as a template for your own themes, or just use it. 
  
* `Waifu.png` 
  * This will be loaded as the background image.
  * It will be centered, and you can pick how to stretch it in the associated `.xaml` file.
  * The filename isn't important, but the `.png` file extension is.
* `Waifu.side.png`
  * This will be loaded as the left side image.<br />It will be left aligned, and you can pick its vertical alignment in the associated `.xaml` file.
  * The filename isn't important, but the `.side.png` file extension is.
* `Video.{mp4, webm, mkv, avi, m2ts}`
  * This will be loaded as a background video, with sound.
  * The filename isn't important, but the file extension must be supported (`.mp4`, `.webm`, `.mkv`, `.avi`, `.m2ts`)
  * Whether the file works or not will depend on what codecs the file has, and whether those are available on your machine.

### Overriding Themes
You can mix and match parts from different themes by giving them the same name.
The priority in which they will be used is `Loose Folder Themes` > `Packaged .mat files` > `Built in`. Overriding themes will only change the files that are included.

Examples:
* Adding `/Themes/Dark.mat` which includes `.png` and `.xaml` files will override both those aspects of the `Dark` theme.
* Adding `/Themes/Dark/image.png` will use that image as the background for the `Dark` theme, overriding both the built in theme and `Dark.mat` if it exists.


## Common Issues
**I hit install but I don't see anything in game!**
  Double check that you followed the [Usage](#usage) instructions correctly.
  Make sure you're looking in the right place. Sometimes mod menus move as  modding libraries/practices change.
  
**I don't see a certain mod in the mods list!**
  Mod Assistant uses mods from [BeatMods](https://beatmods.com/) and shows whatever is available for download. If you need to install a mod manually, please refer to the [Beat Saber Modding Group Wiki](https://bsmg.wiki/pc-modding.html#manual-installation).
  
**I hit install but now my game won't launch, I can't click any buttons, I only see a black screen, etc**
  Please visit the [Beat Saber Modding Group](https://discord.gg/beatsabermods) `#pc-help` channels. Check the pinned messages or ask for help and see if you can work things out.
  
## Credits
semver by Max Hauser
https://github.com/maxhauser/semver
