using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting.IonicZip;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityGoogleDrive;
using File = UnityGoogleDrive.Data.File;

public class AutoBuilderUploader
{
    public AutoBuilderUploader() => PromptUser();

    public class GameVersion
    {
        public enum VersionType
        {
            Major,
            Minor,
            SubMinor
        }
        public VersionType versionType;
        public string versionNumber;
        public int major, minor, subMinor;
    }
    static GameVersion gameVersion = new GameVersion();

    #region Configuration

    //////////////////////////////////////
    const string GameName = "";
    const string GameFileID = "";
    const string VersionFileID = "";
    //////////////////////////////////////
    
    const string BuildFolder = "./Build/";
    const string GameExeFilePath = BuildFolder + GameName + ".exe";
    const string GameZipFilePath = BuildFolder + GameName + ".zip";
    const string VersionFilePath = BuildFolder + "Version.txt";
    #endregion

    public static void StartBuild() => PromptUser();

    static void PromptUser()
    {
        int dialogResult = EditorUtility.DisplayDialogComplex("Select Update Type", "Is this A Major Update?", "Yes", "Cancel", "No");
        if (dialogResult == 1) return;

        if (dialogResult == 2)
        {
            dialogResult = EditorUtility.DisplayDialogComplex("Select Update Type", "Is this A Minor Or SubMinor Update?", "Minor", "Cancel", "SubMinor");
            if (dialogResult == 1) return;
            if (dialogResult == 0) dialogResult++;
        }
        gameVersion.versionType = (GameVersion.VersionType)dialogResult;
        BuildGame();
    }

    static void BuildGame()
    {
        if (!UpdateGameVersion()) return;

        ClearBuildDirectory();
        System.IO.File.WriteAllText(VersionFilePath, gameVersion.versionNumber);

        var report = BuildPipeline.BuildPlayer(GetCurrentlyActiveScenes(), GameExeFilePath, BuildTarget.StandaloneWindows64, BuildOptions.None);
        CompressBuildDirectory();

        UploadGame(report);
    }
    static void UploadGame(BuildReport report)
    {
        if (report.summary.result != BuildResult.Succeeded) return;

        UploadFileToGoogle($"{GameName}.zip", GameZipFilePath, GameFileID);
        UploadFileToGoogle("Version.txt", VersionFilePath, VersionFileID);

        EditorUtility.DisplayDialog("Upload Complete", "Game Successfully Built And Uploaded", "Close");
    }

    static void UploadFileToGoogle(string fileName, string filePath, string fileID)
    {
        byte[] rawFile = System.IO.File.ReadAllBytes(filePath);
        File file = new File { Name = fileName, Content = rawFile };

        var update = GoogleDriveFiles.Update(fileID, file).Send();
        update.OnDone += OnFileUpdated;

        while (update.Progress < 0.99f)
            EditorUtility.DisplayProgressBar("Uploading Game Zip Please Wait", filePath, update.Progress);
    }
    static void OnFileUpdated(File updateFile)
    {
        EditorUtility.ClearProgressBar();
    }

    static void CompressBuildDirectory()
    {
        using (ZipFile zip = new ZipFile())
        {
            zip.AddDirectory(BuildFolder);
            zip.Save(GameZipFilePath);
        }
    }
    static void ClearBuildDirectory()
    {
        if (Directory.Exists(BuildFolder)) Directory.Delete(BuildFolder, true);
        Directory.CreateDirectory(BuildFolder);
    }
    static bool UpdateGameVersion()
    {
        if (System.IO.File.Exists(VersionFilePath))
        {
            gameVersion.versionNumber = System.IO.File.ReadAllText(VersionFilePath);

            string[] versionStrings = gameVersion.versionNumber.Split('.');
            gameVersion.major = int.Parse(versionStrings[0]);
            gameVersion.minor = int.Parse(versionStrings[1]);
            gameVersion.subMinor = int.Parse(versionStrings[2]);

            if (gameVersion.versionType == GameVersion.VersionType.Major)
                gameVersion.major++;
            else if (gameVersion.versionType == GameVersion.VersionType.Minor)
                gameVersion.minor++;
            else if (gameVersion.versionType == GameVersion.VersionType.SubMinor)
                gameVersion.subMinor++;

            gameVersion.versionNumber = $"{gameVersion.major}.{gameVersion.minor}.{gameVersion.subMinor}";
            return true;
        }
        else
        {
            System.IO.File.WriteAllText(VersionFilePath, "0.0.0");
            Debug.LogError($"Version File Does Not Exist! A New Version File Has Been Created Inside {BuildFolder}. " +
                $"Version Number Is Currently 0.0.0, Consider Changing If This Is Not The Correct Version. " +
                $"Re-Run The Builder To Continue");
            return false;
        }
    }

    static EditorBuildSettingsScene[] GetCurrentlyActiveScenes() => EditorBuildSettings.scenes.Where(scene => scene.enabled).ToArray();
}
