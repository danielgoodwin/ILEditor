﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ILEditor.Classes
{
    class Config
    {
        private string ConfigLocation;
        private Dictionary<string, string> Data;

        public Config(string Location)
        {
            ConfigLocation = Location;
            Data = new Dictionary<string, string>();
            LoadConfig();
        }

        public void LoadConfig()
        {
            string[] data;
            if (File.Exists(ConfigLocation))
            {
                foreach (string Line in File.ReadAllLines(ConfigLocation))
                {
                    data = Line.Split(new[] { '=' }, 2);
                    for (int i = 0; i < data.Length; i++) data[i] = data[i].Trim();

                    if (Data.ContainsKey(data[0]))
                    {
                        Data[data[0]] = data[1];
                    }
                    else
                    {
                        Data.Add(data[0], data[1]);
                    }
                }
            }

            SaveConfig();
        }

        public void CheckExist(string key, string value)
        {
            if (!Data.ContainsKey(key))
                SetValue(key, value);
        }

        public void DoEditorDefaults()
        {
            CheckExist("acspath", "false");
            CheckExist("darkmode", "false");
        }

        public void DoSystemDefaults()
        {
            CheckExist("system", "system");
            CheckExist("username", "myuser");
            CheckExist("password", "mypass");
            CheckExist("alias", Data["system"]);
            CheckExist("useFTPES", "false");
            CheckExist("transferMode", "AutoPassive");

            CheckExist("datalibl", "SYSTOOLS");
            CheckExist("curlib", "SYSTOOLS");

            CheckExist("homeDir", "/home/" + Data["username"] + "/");
            CheckExist("tempSpf", "QSOURCE");

            CheckExist("printerLib", "*LIBL");
            CheckExist("printerObj", "QPRINT");
            CheckExist("fetchJobLog", "false");

            CheckExist("TREE_LIST", "");
            CheckExist("FONT", "Consolas");
            CheckExist("ZOOM", 12.75f.ToString());
            CheckExist("INDENT_SIZE", "4");
            CheckExist("SHOW_SPACES", "false");
            CheckExist("HIGHLIGHT_CURRENT_LINE", "true");
            CheckExist("CONV_TABS", "true");
            CheckExist("CL_FORMAT_ON_SAVE", "false");
            CheckExist("CHARACTER_ASSIST", "false");

            CheckExist("CMPTYPES", "RPGLE|SQLRPGLE|CLLE|C|CMD|PF|LF|CBL|SQLCBL|RPG|SQLRPG|DSPF|PRTF|CLP");
            CheckExist("DFT_RPGLE", "CRTBNDRPG");
            CheckExist("DFT_SQLRPGLE", "CRTSQLRPGI");
            CheckExist("DFT_CLLE", "CRTBNDCL");
            CheckExist("DFT_C", "CRTBNDC");
            CheckExist("DFT_CMD", "CRTCMD");
            CheckExist("DFT_PF", "CRTPF");
            CheckExist("DFT_LF", "CRTLF");
            CheckExist("DFT_CBL", "CRTCBLPGM");
            CheckExist("DFT_SQLCBL", "CRTSQLCBL");
            CheckExist("DFT_RPG", "CRTRPGPGM");
            CheckExist("DFT_SQLRPG", "CRTSQLRPG");
            CheckExist("DFT_DSPF", "CRTDSPF");
            CheckExist("DFT_PRTF", "CRTPRTF");
            CheckExist("DFT_CLP", "CRTCLPGM");

            CheckExist("TYPE_RPGLE", "CRTBNDRPG|CRTRPGMOD");
            CheckExist("CRTBNDRPG", "CRTBNDRPG PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) OPTION(*EVENTF) DBGVIEW(*SOURCE) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");
            CheckExist("CRTBNDRPG_IFS", "CRTBNDRPG PGM(&BUILDLIB/&FILENAME) SRCSTMF('&FILEPATH') OPTION(*EVENTF) DBGVIEW(*SOURCE)");

            CheckExist("CRTRPGMOD", "CRTRPGMOD MODULE(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) OPTION(*EVENTF) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_SQLRPGLE", "CRTSQLRPGI|CRTSQLRPGI_MOD");
            CheckExist("CRTSQLRPGI", "CRTSQLRPGI OBJ(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) COMMIT(*NONE) OPTION(*EVENTF *XREF)");
            CheckExist("CRTSQLRPGI_IFS", "CRTSQLRPGI OBJ(&BUILDLIB/&FILENAME) SRCSTMF('&FILEPATH') COMMIT(*NONE) OPTION(*EVENTF *XREF)");

            CheckExist("CRTSQLRPGI_MOD", "CRTSQLRPGI OBJ(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) COMMIT(*NONE) OBJTYPE(*MODULE) OPTION(*EVENTF *XREF)");

            CheckExist("TYPE_CLLE", "CRTBNDCL");
            CheckExist("CRTBNDCL", "CRTBNDCL PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) OPTION(*EVENTF)");

            CheckExist("TYPE_C", "CRTBNDC|CRTCMOD");
            CheckExist("CRTBNDC", "CRTBNDC PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) DBGVIEW(*SOURCE) OPTION(*EVENTF)");
            CheckExist("CRTCMOD", "CRTCMOD MODULE(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) DBGVIEW(*SOURCE) OPTION(*EVENTF)");

            CheckExist("TYPE_CMD", "CRTCMD");
            CheckExist("CRTCMD", "CRTCMD CMD(&OPENLIB/&OPENMBR) PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF)");

            CheckExist("TYPE_PF", "CRTPF");
            CheckExist("CRTPF", "CRTPF FILE(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_LF", "CRTLF");
            CheckExist("CRTLF", "CRTLF FILE(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_RPG", "CRTRPGPGM");
            CheckExist("CRTRPGPGM", "CRTRPGPGM PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_CBL", "CRTCBLPGM");
            CheckExist("CRTCBLPGM", "CRTCBLPGM PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_SQLRPG", "CRTSQLRPG");
            CheckExist("CRTSQLRPG", "CRTSQLRPG PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_SQLCBL", "CRTSQLCBL");
            CheckExist("CRTSQLCBL", "CRTSQLCBL PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_DSPF", "CRTDSPF");
            CheckExist("CRTDSPF", "CRTDSPF FILE(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_PRTF", "CRTPRTF");
            CheckExist("CRTPRTF", "CRTPRTF FILE(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");

            CheckExist("TYPE_CLP", "CRTCLPGM");
            CheckExist("CRTCLPGM", "CRTCLPGM PGM(&OPENLIB/&OPENMBR) SRCFILE(&OPENLIB/&OPENSPF) SRCMBR(&OPENMBR) ERRFILE(&OPENLIB/QBIFERR) ERRMBR(&OPENMBR)");


            CheckExist("IFS_LINKS", "");
        }

        public void SaveConfig()
        {
            List<string> fileout = new List<string>();
            foreach (var key in Data.Keys)
            {
                fileout.Add(key + '=' + Data[key]);
            }
            File.WriteAllLines(ConfigLocation, fileout.ToArray());
        }

        public string GetValue(string Key)
        {
            if (Data.ContainsKey(Key))
                return Data[Key];
            else
                return "";
        }

        public void SetValue(string Key, string Value)
        {
            if (Data.ContainsKey(Key))
                Data[Key] = Value;
            else
                Data.Add(Key, Value);

            SaveConfig();
        }
    }

    class Password
    {
        public static string Encode(string ValuePlain)
        {
            RegistryKey SoftwareKey = Registry.CurrentUser.OpenSubKey("ILEditor", true);

            if (SoftwareKey == null)
                SoftwareKey = Registry.CurrentUser.CreateSubKey("ILEditor");

            byte[] valBytes = Encoding.ASCII.GetBytes(ValuePlain);

            // Generate additional entropy (will be used as the Initialization vector)
            byte[] entropy;

            entropy = SoftwareKey.GetValue("passkey") as byte[];
            if (entropy == null)
            {
                entropy = new byte[20];
                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                    rng.GetBytes(entropy);
                SoftwareKey.SetValue("passkey", entropy);
            }
            
            byte[] ciphertext;
            ciphertext = ProtectedData.Protect(valBytes, entropy, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(ciphertext);
        }

        public static string Decode(string ValueBase64)
        {
            RegistryKey SoftwareKey = Registry.CurrentUser.OpenSubKey("ILEditor", true);

            if (SoftwareKey == null)
                SoftwareKey = Registry.CurrentUser.CreateSubKey("ILEditor");

            byte[] entropy = SoftwareKey.GetValue("passkey") as byte[];

            if (entropy != null)
            {
                try
                {
                    //Usually crashed due to invalid base64 (the old password (-:)
                    byte[] ciphertext = Convert.FromBase64String(ValueBase64);
                    byte[] plaintext = ProtectedData.Unprotect(ciphertext, entropy, DataProtectionScope.CurrentUser);
                    return Encoding.ASCII.GetString(plaintext);
                }
                catch
                {
                    return "";
                }
            }
            else
            {
                return "";
            }
        }
    }
}
