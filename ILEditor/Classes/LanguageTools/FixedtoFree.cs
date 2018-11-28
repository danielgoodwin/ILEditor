using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILEditor.Classes.LanguageTools
{
    class FixedtoFree
    {
        public static IList<string> Convert(string[] Lines)
        {
            List<string> inputSource = new List<string>(Lines);

            // Strip trailing empty lines
            for (int idx = inputSource.Count() - 1; idx >= 0;  idx--)
            {
                if (inputSource[idx].Trim().Length == 0)
                {
                    inputSource.RemoveAt(idx);
                }
                else
                {
                    break;
                }
            }
            List<string> outputFile = new List<string>();
            

            try
            {
                string inFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".tmp";
                string outFileName = System.IO.Path.GetTempPath() + Guid.NewGuid().ToString() + ".tmp";

                System.IO.File.WriteAllLines(inFileName, inputSource.ToArray());

                fixed2free.Fixed2FreeTranslator translator = new fixed2free.Fixed2FreeTranslator();

                translator.translateSingleILEMember(inFileName, outFileName);
                if (translator.getErrorArraySize() > 0)
                {
                    string[] errors = translator.getErrorArray();
                    Editor.TheEditor.SetStatus("Free conversion failed");
                    return Lines;
                }

                string[] outputLines = System.IO.File.ReadAllLines(outFileName);

                outputFile = new List<string>(outputLines);
            }
            catch (Exception ex)
            {
                // In case of error, return unchanged
                Editor.TheEditor.SetStatus("Free conversion failed");
                return Lines;
            }

            return outputFile.ToArray();
        }

    }
}
