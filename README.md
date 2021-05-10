[![Mod Assistant](https://cdn.assistant.moe/images/ModAssistant/Icons/Banner.svg?v=5)](https://github.com/Assistant/ModAssistant/releases/latest)
[![点此下载! Download here!](https://cdn.assistant.moe/images/ModAssistant/Icons/Download.svg)](https://github.com/beatmods-top/ModAssistant/releases/)

Mod Assistant 是节奏光剑(Beat Saber)PC版mod安装器。它使用来自[BeatMods](https://beatmods.com/)和[BeatMods.top](https://beatmods.top) (第三方源，镜像BeatMods.com并包含第三方上传插件和网易影核1.8.0版支持)

* [特性](#特性)
* [用法](#用法)
* [主题](#主题)
  * [自定义主题](#自定义主题)
  * [内置主题](#内置主题)
  * [打包 `.mat` 文件](#打包mat文件)
  * [主题文件夹](#主题文件夹)
  * [覆盖主题](#覆盖主题)
* [常见问题](#常见问题)

## 特性

Mod Assistant拥有丰富的功能，其中包括：
* 处理依赖项
* 检测已安装mod
* 卸载mod
* OneClick&trade; 一键安装支持
* 复杂主题引擎
* 本地化支持
* 摸摸头贴贴
* 切换下载节点
* 网易影核版插件支持(1.8.0)

## 用法
从Release中下载最新安装器并运行。程序在启动时会自动更新，所以不需要每次都下载新版本。

1. **在为游戏安装mod前请至少运行一次游戏！** 重装游戏也需要这样做。所有旧的mod会在游戏版本变更后的第一次启动时被移动到`Old X.X.X Plugins`文件夹，所以在更新游戏后别忘启动游戏一次。
2. 做完上一步后，你只需要打勾想要安装的mod然后点击<kbd>安装或更新</kbd>按钮。同样，如果你想卸载mod的话点击<kbd>卸载</kbd>按钮。
3. 安装的mod会在游戏运行前放在 `IPA/Pending`文件夹中。运行游戏来完成mod安装。

## 主题
<details>
    <summary><b>明亮(Light)</b></summary>
    <div>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light/Intro.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light/Mods.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light/About.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light/Options.png" /></p>
    </div>
</details>

<details>
    <summary><b>黑暗(Dark)</b></summary>
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
    <summary><b>浅粉(Light Pink)</b></summary>
    <div>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light Pink/Intro.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light Pink/Mods.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light Pink/About.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Light Pink/Options.png" /></p>
    </div>
</details>

<details>
    <summary><b>你自定义的!</b></summary>
    <div>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Custom/Intro.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Custom/Mods.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Custom/About.png" /></p>
        <p><img src="https://cdn.assistant.moe/images/ModAssistant/Themes/Custom/Options.png" /></p>
    </div>
</details>

### 自定义主题
自定义主题保存在`ModAssistant.exe`所在文件夹的`Themes`文件夹中。Mod Assistant可以通过以下三种方式加载主题。

### 内置主题
这些主题内置于程序并且无法更改。但是你可以通过以下另外两种方式创建同名主题来覆盖内置主题。

如果你有特别好的人气主题，你可以到这里[提交请求(Pull Request)](https://github.com/Assistant/ModAssistant/pulls)来作为内置主题。

### 打包`.mat`文件
这些是预打包好的主题文件。你可以把它们重命名为`.zip`文件，并且文件结构和下面的`文件夹`类型主题相同。但是这类主题会被下面的同名`文件夹`类型主题覆盖。
创建该类主题请按照下面的`文件夹`类型主题说明创建主题，打成zip压缩文件并重命名为`<主题名>.mat`。

### 主题文件夹
这类主题将会覆盖其他主题，并且从以主题名命名的文件夹中加载。里面可以包含以下4类文件：

* `Theme.xaml` 
  * 该文件定义主题颜色和主题样式。
  * 文件名叫什么不重要，但是一定要保证扩展名为`.xaml`。
  * 要查看示例文件，请在`选项`页点击<kbd>导出模板</kbd> 按钮。在`Themes`文件夹中会生成一个名为`Ugly Kulu-Ya-Ku`的文件夹。你可以打开其中的文件作为模板文件进行修改。
  
* `Waifu.png` 
  * 该文件作为背景图片加载。
  * 默认为居中显示，你可以在关联的`.xaml`文件中定义如何拉伸。
  * 文件名叫什么不重要，但是一定要保证扩展名为`.png`。
* `Waifu.side.png`
  * 该文件作为左侧栏图片加载。
  * 默认为左对齐，你可以在关联的`.xaml`文件中定义垂直对齐属性。
  * 文件名叫什么不重要，但是一定要保证扩展名为`.side.png`。
* `Video.{mp4, webm, mkv, avi, m2ts}`
  * 该文件作为背景视频加载并播放声音。
  * 文件名叫什么不重要，但是一定要保证扩展名为支持的文件格式 (`.mp4`, `.webm`, `.mkv`, `.avi`, `.m2ts`)
  * 文件是否支持还取决于文件编码格式是否能在你的设备上播放。

### 覆盖主题
你可以在同一主题名中使用多种匹配的组件。
使用优先级为`文件夹主题` > `打包.mat文件` > `内置主题`。覆盖主题仅在文件存在时有效。

示例:
* 添加`/Themes/Dark.mat`文件，其中包含`.png`和`.xaml`文件将会覆盖`Dark`主题中的对应文件。.
* 添加`/Themes/Dark/image.png`文件将会将该图片作为`Dark`主题的背景图并覆盖内置, `Dark`主题以及`Dark.mat`文件(如果存在的话)。


## 常见问题
**我看不懂英文**
  请点击左侧栏中的`Options`标签页，在右上角找到`A 文`标识右侧下拉框 选择`中文`即可。

**mod列表加载不出来**
  由于你的运营商或者节点原因，你可能无法正常访问服务器。可以尝试切换节点。  
  请点击左侧栏中的`Options/选项`标签页，在右上角找到`⏬`标识右侧下拉框切换节点。  
  国际源使用Cloudflare的CDN。  
  国内源使用上海阿里云。  
  增强源使用美国机房。  

**我点了安装但是游戏里什么都看不到**
  请仔细阅读[用法](#用法)里的说明。
  确保你没有看差任何地方。有些时候mod的菜单会随着列表宽度而变化。
  如果没有解决，请查看[详细教程与问题解答](https://bs-wgzeyu.gtxcn.com/pc-faq/)。

**我在mod列表中找不到一些mod!**
  Mod Assistant使用的mod来自[BeatMods](https://beatmods.com/)和[BeatMods.top](https://beatmods.top)而且只显示可供下载的mod。 如果你想手动安装mod，请阅读[新手入门指南](https://bs-wgzeyu.gtxcn.com/pc-guide/)或[详细教程与问题解答](https://bs-wgzeyu.gtxcn.com/pc-faq/)中的手动安装Mod教程。
  *增强源在国际源的基础上包含了国际源和国内源所没有的一些第三方插件。*
  
**我使用网易影核版节奏空间想装mod**
  *目前网易影核版节奏空间mod只支持游戏版本为1.8.0*
  请切换下载节点至`增强源`并将游戏目录定位到节奏空间安装文件夹。
  安装任何非官方mod都将安装特别版BSIPA，并且限制可加载mod项为列表中所列出的mod。
  你可以通过安装影核版BSIPA恢复至官方状态(仅可加载官方mod)。

**我点了安装但是游戏打不开，点不了任何按钮或者只能看到黑屏**
  请查看[详细教程与问题解答](https://bs-wgzeyu.gtxcn.com/pc-faq/)。如果解决不了，可以在右上角加群提问，也可以访问[Beat Saber Modding Group](https://discord.gg/beatsabermods) `#pc-help` 频道。

## 鸣谢
semver by Max Hauser
https://github.com/maxhauser/semver

Mod Assistant is a PC mod installer for Beat Saber. It uses mods from [BeatMods](https://beatmods.com/) and [BeatMods.top](https://beatmods.top) (A 3rd party site mirrors BeatMods.com with 3rd party plugins and Netvios Edition Support).

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
* Downloads Server switch
* Netvios Edition Plugin Support

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
  Please visit the [Beat Saber Modding Group](https://discord.gg/beatsabermods) `#pc-help` channels. Check the pinned messages or ask for help and see if you can work out things out.

## Credits
semver by Max Hauser
https://github.com/maxhauser/semver
