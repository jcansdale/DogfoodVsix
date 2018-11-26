### What is Dogfood.vsix?

Dogfood.vsix is what you use when your extension is ready to try in your main Visual Studio 2015 or 2017 instance.

![image](https://user-images.githubusercontent.com/11719160/41686244-441010b6-74db-11e8-809c-a8e4b1c2acdb.png)

It does a few things to make this process as painless as possible.

1. If your current solution contains an extension project, it will locate the .vsix and suggest opening it
2. You're free to choose a different one or paste in a .vsix URL to open (e.g. from a releases page)
3. If the extension is already installed, it will be uninstalled (even if it's a newer version)
4. All users extensions will be tweaked to install for the current user (so you don't need to close all `devenv.exe` processes)
5. The name of the extension will have `[Dogfood]` added so you know it's *special*
6. Your extension's install directory and assets will be displayed on the output window

![image](https://user-images.githubusercontent.com/11719160/41687045-42cafb28-74de-11e8-805c-17c528c7a4c6.png)

Restart Visual Studio and start dogfooding! 🍽 

You can find the latest `Dogfood.vsix` here:
http://vsixgallery.com/author/jamie%20cansdale
