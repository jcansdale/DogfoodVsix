# Dogfood.vsix

`Dogfood.vsix` installs Visual Studio extensions without the need to restart all running instances of Visual Studio. **It speeds up dogfooding your own or trying out any Visual Studio extensions.**

If you are an extension developer, `Dogfood.vsix` will conveniently locate .vsix in your extension project and open it in your main Visual Studio instance.

If you just want to quickly try out some Visual Studio extension, `Dogfood.vsix` removes the hurdle of restarting all running Visual Studio instances. It installs .vsix by restarting only the current Visual Studio instance, thus speeding up the tryout process.

# Installing and Using Dogfood.vsix

The easiest way to get `Dogfood.vsix` is to install it directly from Visual Studio by using "Tools -> Extensions and Updates". You can also [download it](https://marketplace.visualstudio.com/items?itemName=JamieCansdale.DogfoodVsix) from the Visual Studio Marketplace.

The latest version can be found at the [Open VSIX Gallery](http://vsixgallery.com/author/jamie%20cansdale).

Once installed, it adds "Dogfood" menu item to the Visual Studio "Tools" menu.

![image](https://user-images.githubusercontent.com/11719160/41686244-441010b6-74db-11e8-809c-a8e4b1c2acdb.png)
To start dogfooding just run the "Dogfood" command. If your current solution contains an extension project, `Dogfood.vsix` will locate the .vsix and suggest opening it. Alternatively, you can select any local .vsix file or paste in a .vsix URL to open (e.g. from a GitHub Releases pages or Open VSIX Gallery).

`Dogfood.vsix` will install the selected .vsix. To use the newly installed extension **you will only need to restart the Visual Studio instance you are currently in**.

# How it Works?

To make the process of dogfooding Visual Studio extensions as painless as possible *Dogfood.vsix* does the following:

1. If your current solution contains an extension project, it will locate the .vsix and suggest opening it.
2. Alternatively, you can select any local .vsix file or paste in a .vsix URL to open (e.g. from a GitHub Releases page or Open VSIX Gallery).
3. If the selected extension is already installed, it will be uninstalled (even if a newer version is currently installed).
4. All users extensions will be tweaked to install for the current user (so that you don't need to close all Visual Studio processes).
5. The name of the extension will have `[Dogfood]` added so you know it's *special*.
6. Your extension's install directory and assets will be displayed on the output window.

![image](https://user-images.githubusercontent.com/11719160/41687045-42cafb28-74de-11e8-805c-17c528c7a4c6.png)
Install `Dogfood.vsix` and start dogfooding! 🍽