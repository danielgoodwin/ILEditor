using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using FluentFTP;
using System.Net.Sockets;
using System.Timers;

namespace ILEditor.Classes
{
    class IBMi
    {
        public static Config CurrentSystem;
        private static int transportIdx = 1;
        public static IBMFTPi ibmftp;
        public static IBMDDMi ibmddm;

        private static IBMiTransport[] transports = { ibmftp, ibmddm };
        private static bool useDDM = true;

        public static bool IsConnected()
        {
            if (useDDM)
            {
                return IBMDDMi.IsConnected();
            }
            else
            {
                return IBMFTPi.IsConnected();
            }

        }
        public static string FTPFile = "";
        public static bool Connect(bool OfflineMode = false, string promptedPassword = "")
        {
            if (useDDM)
            {
                return IBMDDMi.Connect(OfflineMode, promptedPassword);
            }
            else
            {
                return IBMFTPi.Connect(OfflineMode, promptedPassword);
            }
        }

        public static void Disconnect()
        {

            if (useDDM)
            {
                IBMDDMi.Disconnect();
            }
            else
            {
                IBMFTPi.Disconnect();
            }

        }


        public static string GetSystem()
        {
            if (useDDM)
            {
                return IBMDDMi.GetSystem();
            }
            else
            {
                return IBMFTPi.GetSystem();
            }
        }

        //Returns false if successful
        public static bool DownloadFile(string Local, string Remote, DDMStreamCallbackAdaptor adaptor = null)
        {
            if (useDDM)
            {
                return IBMDDMi.DownloadFile(Local, Remote, adaptor);
            }
            else
            {
                return IBMFTPi.DownloadFile(Local, Remote);
            }
        }

        
        //Returns true if successful
        public static bool UploadFile(string Local, string Remote)
        {
            if (useDDM)
            {
                return IBMDDMi.UploadFile(Local, Remote);
            }
            else
            {
                return IBMFTPi.UploadFile(Local, Remote);
            }
        }

        //Returns true if successful
        public static bool RemoteCommand(string Command, bool ShowError = true)
        {
            if (useDDM)
            {
                return IBMDDMi.RemoteCommand(Command, ShowError);
            }
            else
            {
                return IBMFTPi.RemoteCommand(Command, ShowError);
            }
        }

       
        public static string RemoteCommandResponse(string Command)
        {
            if (useDDM)
            {
                return IBMDDMi.RemoteCommandResponse(Command);
            }
            else
            {
                return IBMFTPi.RemoteCommandResponse(Command);
            }
        }

        //Returns true if successful
        public static bool RunCommands(string[] Commands)
        {
            if(useDDM)
            {
                return IBMDDMi.RunCommands(Commands);
            }
            else
            {
                return IBMFTPi.RunCommands(Commands);
            }
        }

        public static bool FileExists(string remoteFile)
        {
            if (useDDM)
            {
                return IBMDDMi.FileExists(remoteFile);
            }
            else
            {
                return IBMFTPi.FileExists(remoteFile);
            }
        }
        public static bool DirExists(string remoteDir)
        {
            if (useDDM)
            {
                return IBMDDMi.DirExists(remoteDir);
            }
            else
            {
                return IBMFTPi.DirExists(remoteDir);
            }
        }
        public static FtpListItem[] GetListing(string remoteDir)
        {
            if (useDDM)
            {
                return IBMDDMi.GetListing(remoteDir);
            }
            else
            {
                return IBMFTPi.GetListing(remoteDir);
            }
        }

        public static string RenameDir(string remoteDir, string newName)
        {
            if (useDDM)
            {
                return IBMDDMi.RenameDir(remoteDir, newName);
            }
            else
            {
                return IBMFTPi.RenameDir(remoteDir, newName);
            }
        }
        public static string RenameFile(string remoteFile, string newName)
        {
            if (useDDM)
            {
                return IBMDDMi.RenameFile(remoteFile, newName);
            }
            else
            {
                return IBMFTPi.RenameFile(remoteFile, newName);
            }
        }

        public static void DeleteDir(string remoteDir)
        {
            if (useDDM)
            {
                IBMDDMi.DeleteDir(remoteDir);
            }
            else
            {
                IBMFTPi.DeleteDir(remoteDir);
            }
        }

        public static void DeleteFile(string remoteFile)
        {
            if (useDDM)
            {
                IBMDDMi.DeleteFile(remoteFile);
            }
            else
            {
                IBMFTPi.DeleteFile(remoteFile);
            }
        }

        public static void SetWorkingDir(string RemoteDir)
        {
            if (useDDM)
            {
                IBMDDMi.SetWorkingDir(RemoteDir);
            }
            else
            {
                IBMFTPi.SetWorkingDir(RemoteDir);
            }
        }
        public static void CreateDirecory(string RemoteDir)
        {
            if (useDDM)
            {
                IBMDDMi.CreateDirecory(RemoteDir);
            }
            else
            {
                IBMFTPi.CreateDirecory(RemoteDir);
            }
        }
        public static void UploadFiles(string RemoteDir, string[] Files)
        {
            if (useDDM)
            {
                IBMDDMi.UploadFiles(RemoteDir, Files);
            }
            else
            {
                IBMFTPi.UploadFiles(RemoteDir, Files);
            }
        }
    }
}
