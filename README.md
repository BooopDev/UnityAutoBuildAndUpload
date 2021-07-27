# UnityAutoBuildAndUpload
Automatically build and upload your Unity game to google drive.

This is a really simple script but does require some knowledge of the GoogleDrive API and depends on [UnityGoogleDrive](https://github.com/Elringus/UnityGoogleDrive) By Elringus. This tool makes building and hosting your game files much easier and was made in conjunction with my Game Launcher to automatically distribute my game files. For my project, I used the [UnityToolbarExtender](https://github.com/marijnz/unity-toolbar-extender)(Optional) By Marijnz to create a an easy access button on my toolbar to trigger the build process. This is optional and can be easily started through other means such as custom editor windows or inspectors.

__(Note) This is not the perfect solution for hosting games, but a way for smaller indie devs like myself to help distribute files amongst friends, family and testers. I recommend people who plan on using this to host their games on a large scale to be weary that google drive does have a download limit to prevent abuse and your files may be locked for short periods of time if you exceed that download quota.__

## Dependencies
- [UnityGoogleDrive](https://github.com/Elringus/UnityGoogleDrive)
- [UnityToolbarExtender](https://github.com/marijnz/unity-toolbar-extender)(Optional)

## Installation
- Download [UnityGoogleDrive](https://github.com/Elringus/UnityGoogleDrive) and follow their installation instructions
- Download the [UnityPackage](this) and drag it into your project.

## Setup
- Firstly follow the instructions for the UnityGoogleDrive Package to setup your Google Drive API settings
- Upload an initial {GameName}.zip and Version.txt file to your google drive to create the File ID's necessary for the next step. Ensure that the name of your zip file is the same as your game name. You must also share your files and make them public for anyone to download.
- Once you've setup your drive, navigate to the AutoBuilder script and find the configuration settings at the top of the file. Paste in your game name, your {GameName}.zip File ID and your Version.txt File ID. To get the File ID's you'll find them in the share link of the file. 
```
If this was the link.
"https://drive.google.com/file/d/1AprEimMMmunJoxnL5kp1vU1XsBF3SuX6/view?usp=sharing"

This is the File ID
"1AprEimMMmunJoxnL5kp1vU1XsBF3SuX6"
```
- And you're done!

## Usage
For this Example I used the [UnityToolbarExtender](https://github.com/marijnz/unity-toolbar-extender) but you can run this on any editor window or custom inspector. Run the script and a prompt will appear asking for the version type you want to upload and once selected, will automatically build and upload your files for you.

```C#
using UnityEditor;
using UnityEngine;
using UnityToolbarExtender;

[InitializeOnLoad]
public static class ToolBarButtons 
{
    static ToolBarButtons()
    {
        ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
    }

    static void OnToolbarGUI()
    {
        if (GUILayout.Button(new GUIContent("Build And Upload To Google Drive"), EditorStyles.toolbarButton))
            AutoBuilderUploader.StartBuild();
    }
}
```
