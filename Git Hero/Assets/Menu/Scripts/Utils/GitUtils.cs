﻿using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Githero.Menu.Ultils
{
    public class GitUtils
    {
        private const string RepoFolderName = "Repo";

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private const string FileName = "powerShell.exe";
        private const string DeleteFileCommand = "Remove-Item -Recurse -Force";
        private const string CreateFolderCommand = "New-Item -itemType Directory -Name";
        private const string ConcateneCommandsChar = ";";
        private const string CreateGitGraphCommand =
            "git --no-pager log --graph --full-history --all --pretty=format:\"\" > ..\\..\\";
#else
        private const string FileName = "/bin/bash";
        private const string DeleteFileCommand = "rm -rf";
        private const string CreateFolderCommand = "mkdir";
        private const string ConcateneCommandsChar = "&&";
        private const string CreateGitGraphCommand =
            "git --no-pager log --graph --full-history --all --pretty=format:\"\" > ../../";
#endif      

        private const string GotToFolderCommand = "cd";
        private const string GotToFirstAvaliableFolder = "cd *";
        private const string CloneBareCommand = "git clone --bare";

        public void GetRepoGraph(string gitCloneLink, Action getGraphPathAction)
        {
            var backgroundWorker = new BackgroundWorker();

            backgroundWorker.DoWork += (sender, args) =>
            {
                DeleteGraphFile();
                DeleteRepoFolder();
                CreateFolder();
                CloneBareIntoRepoFolder(gitCloneLink);
                CreateGraphFile();
                DeleteRepoFolder();
            };

            backgroundWorker.RunWorkerCompleted += (sender, args) =>
            {
                if (args.Error == null) { getGraphPathAction.Invoke(); }
                else { throw new Exception(args.Error.Message); }
            };

            backgroundWorker.RunWorkerAsync();
        }

        private void DeleteGraphFile() =>
            StartCommand($"{DeleteFileCommand} {Constants.Strings.GraphFilePath}", false);

        private void DeleteRepoFolder() =>
            StartCommand($"{DeleteFileCommand} {RepoFolderName}", false);

        private void CreateFolder() =>
            StartCommand($"{CreateFolderCommand} {RepoFolderName}");

        private void CloneBareIntoRepoFolder(string gitCloneLink) =>
            StartCommand(
                $"{GotToFolderCommand} {RepoFolderName}" +
                $" {ConcateneCommandsChar}" +
                $" {CloneBareCommand} {gitCloneLink}");

        private void CreateGraphFile()
        {
            StartCommand(
            $"{GotToFolderCommand} {RepoFolderName}" +
            $" {ConcateneCommandsChar}" +
            $" {GotToFirstAvaliableFolder}" +
            $" {ConcateneCommandsChar}" +
            $" {CreateGitGraphCommand}{Constants.Strings.GraphFilePath}");
        }

        private void StartCommand(string argument, bool allowException = true)
        {
            try
            {
#if !UNITY_EDITOR_WIN || !UNITY_STANDALONE_WIN
                argument = " -c \"" + argument + " \"";                
#endif
                var startInfo = new ProcessStartInfo()
                {
                    FileName = FileName,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                    Arguments = argument
                };

                var process = Process.Start(startInfo);
                process.WaitForExit();
                process.Close();
            }
            catch (Exception exception)
            {
                if (allowException) { throw new Exception(exception.Message); }
            }

        }

    }

}