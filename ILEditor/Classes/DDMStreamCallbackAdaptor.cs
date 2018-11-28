using com.ibm.jtopenlite.ddm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ILEditor.Classes
{
    public class DDMStreamCallbackAdaptor : DDMReadCallbackAdapter
    {
        protected StreamWriter _writer;
        protected DDMRecordFormat _format;
        protected string[] _fields;
        protected Dictionary<string, string> _fieldsLookup = new Dictionary<string, string>();

        public StreamWriter Writer
        {
            set
            {
                _writer = value;
            }
        }

        public DDMRecordFormat Format
        {
            set
            {
                _format = value;
            }
        }

        public string[] Fields
        {
            set
            {
                _fields = value;
                // Create direct lookup for fields
                _fieldsLookup.Clear();
                if (_fields != null)
                {
                    for (int i = 0; i < _fields.Length; i++)
                    {
                        _fieldsLookup.Add(_fields[i], _fields[i]);
                    }
                }
            }
        }

        public DDMStreamCallbackAdaptor()
        {
            _writer = null;
            _format = null;
            _fields = null;

        }

        public override void newRecord(int recordNumber, byte[] recordData, bool[] nullFieldMap)
        {
            StringBuilder sb = new StringBuilder();
            if (_format == null || _writer == null)
            {
                return;
            }
            //
            // Get in field order if specified.

            if (_fieldsLookup != null && _fieldsLookup.Count() > 0)
            {
                for (int fldidx = 0; fldidx < _fields.Length; fldidx++)
                {
                    for (int idx = 0; idx < _format.getFieldCount(); idx++)
                    {
                        DDMField fld = _format.getField(idx);
                        if (fld.getName().Equals(_fields[fldidx],
                            StringComparison.InvariantCultureIgnoreCase))
                        {

                            switch (fld.getType())
                            {
                                case DDMField.TYPE_CHARACTER:
                                case DDMField.TYPE_DBCS_OPEN:
                                case DDMField.TYPE_DBCS_EITHER:
                                case DDMField.TYPE_DBCS_GRAPHIC:
                                case DDMField.TYPE_DBCS_ONLY:
                                    sb.Append(fld.getString(recordData).PadRight(fld.getLength()));
                                    break;

                                default:
                                    //
                                    // Numeric, leading zero padded.
                                    sb.Append(fld.getString(recordData).PadLeft(fld.getNumberOfDigits(), '0'));
                                    break;
                            }
                        }

                    }
                }

            }
            else
            {
                //
                // Just process fields in order in file.

                for (int idx = 0; idx < _format.getFieldCount(); idx++)
                {
                    DDMField fld = _format.getField(idx);
                    //
                    // Process fields if no fields specified or
                    // field name is in list passed.
                    //if (_fieldsLookup == null || _fieldsLookup.Count() == 0 ||
                    //       _fieldsLookup.ContainsKey(fld.getName()))
                    {
                        switch (fld.getType())
                        {
                            case DDMField.TYPE_CHARACTER:
                            case DDMField.TYPE_DBCS_OPEN:
                            case DDMField.TYPE_DBCS_EITHER:
                            case DDMField.TYPE_DBCS_GRAPHIC:
                            case DDMField.TYPE_DBCS_ONLY:
                                sb.Append(fld.getString(recordData).PadRight(fld.getLength()));
                                break;

                            default:
                                //
                                // Numeric, leading zero padded.
                                sb.Append(fld.getString(recordData).PadLeft(fld.getNumberOfDigits(), '0'));
                                break;
                        }

                    }
                }
            }
            _writer.WriteLine(sb.ToString());
        }
    }
}
