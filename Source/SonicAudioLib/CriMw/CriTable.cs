using System;
using System.IO;
using System.Linq;

using SonicAudioLib.IO;
using SonicAudioLib.Module;

namespace SonicAudioLib.CriMw
{
    public class CriTable : ModuleBase
    {
        private CriFieldCollection fields;
        private CriRowCollection rows;
        private string tableName = "(no name)";
        private CriTableWriterSettings writerSettings;

        public CriFieldCollection Fields
        {
            get
            {
                return fields;
            }
        } 

        public CriRowCollection Rows
        {
            get
            {
                return rows;
            }
        }

        public string TableName
        {
            get
            {
                return tableName;
            }

            set
            {
                tableName = value;
            }
        }

        public CriTableWriterSettings WriterSettings
        {
            get
            {
                return writerSettings;
            }

            set
            {
                writerSettings = value;
            }
        }

        public void Clear()
        {
            rows.Clear();
            fields.Clear();
        }

        public CriRow NewRow()
        {
            CriRow criRow = new CriRow(this);

            foreach (CriField criField in fields)
            {
                criRow.Records.Add(criField, criField.DefaultValue);
            }

            return criRow;
        }

        public override void Read(Stream source)
        {
            using (CriTableReader reader = CriTableReader.Create(source))
            {
                tableName = reader.TableName;

                for (int i = 0; i < reader.NumberOfFields; i++)
                {
                    fields.Add(reader.GetFieldName(i), reader.GetFieldType(i), reader.GetFieldValue(i));
                }

                while (reader.Read())
                {
                    rows.Add(reader.GetValueArray());
                }
            }
        }

        public override void Write(Stream destination)
        {
            using (CriTableWriter writer = CriTableWriter.Create(destination, writerSettings))
            {
                writer.WriteStartTable(tableName);

                writer.WriteStartFieldCollection();
                foreach (CriField criField in fields)
                {
                    bool useDefaultValue = false;
                    object defaultValue = null;

                    if (rows.Count > 1)
                    {
                        useDefaultValue = true;
                        defaultValue = rows[0][criField];

                        if (rows.Any(row => !row[criField].Equals(defaultValue)))
                        {
                            useDefaultValue = false;
                        }
                    }

                    if (useDefaultValue)
                    {
                        writer.WriteField(criField.FieldName, criField.FieldType, defaultValue);
                    }

                    else
                    {
                        writer.WriteField(criField.FieldName, criField.FieldType);
                    }
                }
                writer.WriteEndFieldCollection();

                foreach (CriRow criRow in rows)
                {
                    writer.WriteRow(true, criRow.GetValueArray());
                }

                writer.WriteEndTable();
            }
        }

        public override long CalculateLength()
        {
            // TODO
            return base.CalculateLength();
        }

        public CriTable()
        {
            fields = new CriFieldCollection(this);
            rows = new CriRowCollection(this);
            writerSettings = new CriTableWriterSettings();
        }

        public CriTable(string tableName) : this()
        {
            this.tableName = tableName;
        }
    }
}
