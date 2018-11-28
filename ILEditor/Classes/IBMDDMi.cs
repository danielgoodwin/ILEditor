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
using com.ibm.jtopenlite;
using com.ibm.jtopenlite.ddm;

namespace ILEditor.Classes
{
    class IBMDDMi : IBMiTransport
    {
        private static DDMConnection ClientDDM;

        public static bool IsConnected()
        {
            if (ClientDDM != null)
                return !ClientDDM.isClosed();
            else
                return false;
        }

        public static void Disconnect()
        {
            if (!ClientDDM.isClosed())
            {
                ClientDDM.close();
                ClientDDM = null;
            }
        }

        public static bool Connect(bool OfflineMode = false, string promptedPassword = "")
        {
            string[] remoteSystem;
            bool result = false;

            try
            {
                string password = "";

                remoteSystem = IBMi.CurrentSystem.GetValue("system").Split(':');

                if (promptedPassword == "")
                    password = Password.Decode(IBMi.CurrentSystem.GetValue("password"));
                else
                    password = promptedPassword;

                ClientDDM = DDMConnection.getConnection(remoteSystem[0], IBMi.CurrentSystem.GetValue("username"), password);

                result = true;

                if (OfflineMode == false)
                {
                    // Ignore result as a new system may not have library list set correctly
                    // We can store status if required.
                    RemoteCommand($"CHGLIBL LIBL({ IBMi.CurrentSystem.GetValue("datalibl").Replace(',', ' ')}) CURLIB({ IBMi.CurrentSystem.GetValue("curlib") })", false);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("Unable to connect to " + IBMi.CurrentSystem.GetValue("system") + " - " + e.Message, "Cannot Connect", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
            finally
            {

            }

            return result;
        }


        public static string GetSystem()
        {
            if (ClientDDM != null)
                if (!ClientDDM.isClosed())
                    return "IBMDDMi";
                else
                    return "";
            else
                return "";
        }

        public static bool DownloadFile(string Local, string Remote)
        {
            return DownloadFile(Local, Remote, new SrcDDMCallbackAdapter());
        }

        public static bool DownloadFile(string Local, string Remote, DDMStreamCallbackAdaptor callback)
        {
            bool Result = false;
            try
            {
                if (ClientDDM != null && !ClientDDM.isClosed())
                {
                    string[] parts = Remote.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                    string remotelib = parts[1].Replace(".lib", "");
                    string remotefile = parts[2].Replace(".file", "");
                    string remotembr = parts[3].Replace(".mbr", "");

                    DDMRecordFormat remotefmt = ClientDDM.getRecordFormat(remotelib, remotefile);
                    if (remotefmt != null)
                    {
                        DDMFile remote = ClientDDM.open(remotelib, remotefile, remotembr, remotefmt.getName());
                        if (remote != null)
                        {
                            // Open local file and pass to Reader
                            StreamWriter sw = new StreamWriter(Local, false, Encoding.ASCII);
                            callback.Format = remotefmt;
                            callback.Writer = sw;
                            while (!callback.isDone())
                            {
                                ClientDDM.readNext(remote, callback);
                            }
                            ClientDDM.close(remote);
                            sw.Close();
                        }
                    }

                }
                else
                    return true; //error
            }
            catch (Exception e)
            {

                Result = true;
            }

            return Result;
        }

        public class SrcDDMCallbackAdapter : DDMStreamCallbackAdaptor
        {
            public SrcDDMCallbackAdapter()
            {
                _writer = null;
                _format = null;
                Fields = new string[] { "SRCDTA" };
            }

        }

        public class AllFieldsDDMCallbackAdapter : DDMStreamCallbackAdaptor
        {
            public AllFieldsDDMCallbackAdapter()
            {
                _writer = null;
                _format = null;
                Fields = null;
            }

        }

        //Returns true if successful
        public static bool UploadFile(string Local, string Remote)
        {
            if (ClientDDM != null && !ClientDDM.isClosed())
            {
                string[] parts = Remote.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                string remotelib = parts[1].Replace(".lib", "");
                string remotefile = parts[2].Replace(".file", "");
                string remotembr = parts[3].Replace(".mbr", "");

                try
                {
                    DDMRecordFormat remotefmt = ClientDDM.getRecordFormat(remotelib, remotefile);
                    if (remotefmt != null)
                    {
                        // At this stage - file does exist, we have to clear member first
                        // Cross fingers we finish this out!
                        if (!RemoteCommand("CLRPFM FILE(" + remotelib + "/" + remotefile + ") MBR(" + remotembr + ")"))
                        {
                            return false;
                        }

                        DDMFile remote = ClientDDM.open(remotelib, remotefile, remotembr, remotefmt.getName(),
                                    DDMFile.WRITE_ONLY, false, 1, 1);
                        if (remote != null)
                        {
                            // Open local file and pass to Writer
                            StreamReader sr = new StreamReader(Local, Encoding.ASCII);
                            int sequence = 1;
                            //MyDDMWriteCallback writer = new MyDDMWriteCallback(sr, remotefmt);
                            String data = sr.ReadLine();
                            while (data != null)
                            {

                                //ClientDDM.write(remote, writer);

                                byte[] returnData = remote.getRecordDataBuffer(); // new byte[_format.getLength() + _format.getFieldCount()];

                                for (int idx = 0; idx < remotefmt.getFieldCount(); idx++)
                                {
                                    DDMField fld = remotefmt.getField(idx);

                                    // SRCSEQ/SRCDAT have to be handled differently (as not stored locally yet)
                                    if ("SRCSEQ".Equals(fld.getName(), StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        fld.setString(sequence.ToString("000000"), returnData);
                                        sequence = sequence + 1;
                                    }
                                    else if ("SRCDAT".Equals(fld.getName(), StringComparison.InvariantCultureIgnoreCase))
                                    {
                                        int dummyDate = 0;
                                        fld.setString(dummyDate.ToString("000000"), returnData);
                                    }
                                    //if ("SRCDTA".Equals(fld.getName(), StringComparison.InvariantCultureIgnoreCase))
                                    else
                                    {
                                        fld.setString(data, returnData);
                                    }
                                }

                                bool[] nulls = new bool[remotefmt.getFieldCount()];
                                for (int idx = 0; idx < remotefmt.getFieldCount(); idx++)
                                {
                                    nulls[idx] = false;
                                }


                                ClientDDM.write(remote, returnData, 0, nulls, null);

                                data = sr.ReadLine();

                            }
                            ClientDDM.close(remote);
                            sr.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Exception in writing
                    return false;
                }

                return true;

            }

            else
                return false;
        }

        //
        // Writer callback does not seem to work!
        private class MyDDMWriteCallback : DDMWriteCallback
        {
            StreamReader _reader;
            DDMRecordFormat _format;
            int _numLines;
            int _sequence = 1;

            public MyDDMWriteCallback(StreamReader reader, DDMRecordFormat remotefmt, int numLines)
            {
                _reader = reader;
                _format = remotefmt;
                _numLines = numLines;
            }

            public MyDDMWriteCallback(StreamReader reader, DDMRecordFormat remotefmt)
            {
                _reader = reader;
                _format = remotefmt;
                _numLines = 1;
            }

            int DDMWriteCallback.getNumberOfRecords(DDMCallbackEvent ddmce)
            {
                return _numLines;
            }

            byte[] DDMWriteCallback.getRecordData(DDMCallbackEvent ddmce, int i)
            {
                String data = _reader.ReadLine();

                byte[] returnData = ddmce.getFile().getRecordDataBuffer();

                for (int idx = 0; idx < _format.getFieldCount(); idx++)
                {
                    DDMField fld = _format.getField(idx);

                    // SRCSEQ/SRCDAT have to be handled differently
                    if ("SRCSEQ".Equals(fld.getName(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        fld.setString(String.Format("{0}", _sequence), returnData);
                        _sequence += 1;

                    }
                    else if ("SRCDAT".Equals(fld.getName(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        fld.setString(String.Format("{0}", 0), returnData);
                    }
                    //if ("SRCDTA".Equals(fld.getName(), StringComparison.InvariantCultureIgnoreCase))
                    else
                    {
                        fld.setString(data, returnData);
                    }
                }
                return returnData;
            }

            int DDMWriteCallback.getRecordDataOffset(DDMCallbackEvent ddmce, int i)
            {
                return _format.getLength();
            }

            bool[] DDMWriteCallback.getNullFieldValues(DDMCallbackEvent ddmce, int i)
            {
                bool[] nulls = new bool[_format.getFieldCount()];
                for (int idx = 0; idx < _format.getFieldCount(); idx++)
                {
                    nulls[idx] = false;
                }
                return nulls;

            }
        }
        //Returns true if successful
        public static bool RemoteCommand(string Command, bool ShowError = true)
        {
            if (ClientDDM != null && !ClientDDM.isClosed())
            {
                try
                {
                    java.util.List lstMessages = ClientDDM.executeReturnMessageList(Command);
                    if (lstMessages == null || lstMessages.size() == 0)
                    {
                        return true;
                    }
                    else
                    {
                        for (int msgidx = 0; msgidx < lstMessages.size(); msgidx++)
                        {
                            com.ibm.jtopenlite.Message msg = (com.ibm.jtopenlite.Message)lstMessages.get(msgidx);
                            if (msg.getSeverity() > 1) // DDM returns sev 0-8 range (AS400 severity / 10 if you will )
                            {
                                if (ShowError)
                                {
                                    MessageBox.Show(msg.getText(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                                return false;
                            }
                        }
                    }
                    // Get here means no messages with Sev > 10, so ok
                    return true;
                } catch (Exception ex)
                {
                    // command failed.
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string RemoteCommandResponse(string Command)
        {
            if (ClientDDM != null && !ClientDDM.isClosed())
            {
                java.util.List lstMessages = ClientDDM.executeReturnMessageList(Command);
                if (lstMessages == null || lstMessages.size() == 0)
                {
                    return "";
                }
                else
                {
                    for (int msgidx = 0; msgidx < lstMessages.size(); msgidx++)
                    {
                        com.ibm.jtopenlite.Message msg = (com.ibm.jtopenlite.Message)lstMessages.get(0);
                        if (msg.getSeverity() > 10)
                        {

                            return msg.getText();
                        }
                    }
                }
                // Get here means no messages with Sev > 10, so ok
                return "";
            }
            else
            {
                return "Not connected.";
            }
        }

        //Returns true if successful
        public static bool RunCommands(string[] Commands)
        {
            bool result = true;
            if (!ClientDDM.isClosed())
            {
                foreach (string Command in Commands)
                {
                    if (RemoteCommand(Command) == false)
                        result = false;
                }
            }
            else
            {
                result = false;
            }

            return result;
        }

        public static bool FileExists(string remoteFile)
        {
            // Used for STMF files
            return false;
        }
        public static bool DirExists(string remoteDir)
        {
            try
            {
                // Used for STMF files
                return false;
            }
            catch (Exception ex)
            {
                Editor.TheEditor.SetStatus(ex.Message + " - please try again.");
                return false;
            }
        }
        public static FtpListItem[] GetListing(string remoteDir)
        {
            //Used in IFS File browser
            return null;
        }

        public static string RenameDir(string remoteDir, string newName)
        {
            string[] pieces = remoteDir.Split('/');
            pieces[pieces.Length - 1] = newName;
            newName = String.Join("/", pieces);

            // Used in IFS Browser
            return null;
        }
        public static string RenameFile(string remoteFile, string newName)
        {
            string[] pieces = remoteFile.Split('/');
            pieces[pieces.Length - 1] = newName;
            newName = String.Join("/", pieces);

            // Used in IFS Browser
            return null;
        }

        public static void DeleteDir(string remoteDir)
        {
            // Used in IFS Browser
            return;
        }

        public static void DeleteFile(string remoteFile)
        {
            // Used in IFS Browser
            return;
        }

        public static void SetWorkingDir(string RemoteDir)
        {
            // Used in IFS Browser
            return;
        }
        public static void CreateDirecory(string RemoteDir)
        {
            // Used in IFS Browser
            return;
        }
        public static void UploadFiles(string RemoteDir, string[] Files)
        {
            // Not used

        }
    }
}
