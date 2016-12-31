using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;
using System.Reflection;
using System.Diagnostics;

using SonicAudioLib.IO;

namespace SonicAudioLib.CriMw.Serialization
{
    public static class CriTableSerializer
    {
        public static byte[] Serialize<T>(List<T> objects, CriTableWriterSettings settings)
        {
            using (MemoryStream destination = new MemoryStream())
            {
                Serialize(destination, objects, settings);
                return destination.ToArray();
            }
        }

        public static void Serialize<T>(string destinationFileName, List<T> objects, CriTableWriterSettings settings)
        {
            using (Stream destination = File.Create(destinationFileName))
            {
                Serialize(destination, objects, settings);
            }
        }

        public static void Serialize<T>(Stream destination, List<T> objects, CriTableWriterSettings settings)
        {
            Serialize(destination, typeof(T), objects, settings);
        }

        public static void Serialize(Stream destination, Type type, ICollection objects, CriTableWriterSettings settings)
        {
            ArrayList arrayList = null;

            if (objects != null)
            {
                arrayList = new ArrayList(objects);
            }

            CriTableWriter tableWriter = CriTableWriter.Create(destination, settings);

            string tableName = type.Name;
            CriSerializableAttribute serAttribute = type.GetCustomAttribute<CriSerializableAttribute>();
            if (serAttribute != null && !string.IsNullOrEmpty(serAttribute.TableName))
            {
                tableName = serAttribute.TableName;
            }

            tableWriter.WriteStartTable(tableName);

            SortedList<int, PropertyInfo> sortedProperties = new SortedList<int, PropertyInfo>();

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                // Add the properties in order
                CriIgnoreAttribute ignoreAttribute = propertyInfo.GetCustomAttribute<CriIgnoreAttribute>();
                if (ignoreAttribute != null)
                {
                    continue;
                }

                // Also ignore the properties that are not supportable (except FileInfo and Stream)
                if (propertyInfo.PropertyType != typeof(FileInfo) &&
                    propertyInfo.PropertyType != typeof(Stream) &&
                    !CriField.FieldTypes.Contains(propertyInfo.PropertyType))
                {
                    continue;
                }

                CriFieldAttribute fieldAttribute = propertyInfo.GetCustomAttribute<CriFieldAttribute>();

                int order = ushort.MaxValue;
                if (fieldAttribute != null)
                {
                    order = fieldAttribute.Order;
                }

                while (sortedProperties.ContainsKey(order))
                {
                    order++;
                }

                sortedProperties.Add(order, propertyInfo);
            }

            tableWriter.WriteStartFieldCollection();
            foreach (var keyValuePair in sortedProperties)
            {
                PropertyInfo propertyInfo = keyValuePair.Value;
                CriFieldAttribute fieldAttribute = propertyInfo.GetCustomAttribute<CriFieldAttribute>();

                string fieldName = propertyInfo.Name;
                Type fieldType = propertyInfo.PropertyType;
                object defaultValue = null;

                // Since the invalid types were cleaned, we can assume that those can be FileInfo or Stream
                // so directly change the type to byte[]
                if (!CriField.FieldTypes.Contains(fieldType))
                {
                    fieldType = typeof(byte[]);
                }

                if (fieldAttribute != null)
                {
                    if (!string.IsNullOrEmpty(fieldAttribute.FieldName))
                    {
                        fieldName = fieldAttribute.FieldName;
                    }
                }

                bool useDefaultValue = false;

                if (arrayList != null && arrayList.Count > 1)
                {
                    useDefaultValue = true;

                    defaultValue = propertyInfo.GetValue(arrayList[0]);

                    for (int i = 1; i < arrayList.Count; i++)
                    {
                        object objectValue = propertyInfo.GetValue(arrayList[i]);
                        if (defaultValue != null)
                        {
                            if (!defaultValue.Equals(objectValue))
                            {
                                useDefaultValue = false;
                                defaultValue = null;
                                break;
                            }
                        }
                        else if (objectValue != null)
                        {
                            useDefaultValue = false;
                            defaultValue = null;
                            break;
                        }
                    }
                }

                else if (arrayList == null || (arrayList != null && arrayList.Count == 0))
                {
                    useDefaultValue = true;
                    defaultValue = null;
                }

                if (useDefaultValue)
                {
                    tableWriter.WriteField(fieldName, fieldType, defaultValue);
                }

                else
                {
                    tableWriter.WriteField(fieldName, fieldType);
                }
            }

            tableWriter.WriteEndFieldCollection();

            // Time for objects.
            if (arrayList != null)
            {
                foreach (object obj in arrayList)
                {
                    tableWriter.WriteStartRow();

                    int index = 0;
                    foreach (PropertyInfo propertyInfo in sortedProperties.Values)
                    {
                        object value = propertyInfo.GetValue(obj);

                        Type propertyType = propertyInfo.PropertyType;

                        tableWriter.WriteValue(index, value);
                        index++;
                    }

                    tableWriter.WriteEndRow();
                }
            }

            tableWriter.WriteEndTable();
            tableWriter.Dispose();
        }

        public static List<T> Deserialize<T>(byte[] sourceByteArray)
        {
            return Deserialize(sourceByteArray, typeof(T)).OfType<T>().ToList();
        }

        public static List<T> Deserialize<T>(string sourceFileName)
        {
            return Deserialize(sourceFileName, typeof(T)).OfType<T>().ToList();
        }

        public static List<T> Deserialize<T>(Stream source)
        {
            return Deserialize(source, typeof(T)).OfType<T>().ToList();
        }

        public static ArrayList Deserialize(byte[] sourceByteArray, Type type)
        {
            using (MemoryStream source = new MemoryStream(sourceByteArray))
            {
                return Deserialize(source, type);
            }
        }

        public static ArrayList Deserialize(string sourceFileName, Type type)
        {
            using (Stream source = File.OpenRead(sourceFileName))
            {
                return Deserialize(source, type);
            }
        }

        public static ArrayList Deserialize(Stream source, Type type)
        {
            ArrayList arrayList = new ArrayList();

            using (CriTableReader tableReader = CriTableReader.Create(source, true))
            {
                PropertyInfo[] propertyInfos = type.GetProperties();

                while (tableReader.Read())
                {
                    object obj = Activator.CreateInstance(type);

                    for (int i = 0; i < tableReader.NumberOfFields; i++)
                    {
                        string fieldName = tableReader.GetFieldName(i);

                        foreach (PropertyInfo propertyInfo in propertyInfos)
                        {
                            string fieldNameMatch = propertyInfo.Name;

                            CriFieldAttribute fieldAttribute = propertyInfo.GetCustomAttribute<CriFieldAttribute>();
                            
                            if (fieldAttribute != null && !string.IsNullOrEmpty(fieldAttribute.FieldName))
                            {
                                fieldNameMatch = fieldAttribute.FieldName;
                            }

                            if (fieldName == fieldNameMatch)
                            {
                                object value = tableReader.GetValue(i);
                                if (propertyInfo.PropertyType == typeof(byte[]) && value is Substream)
                                {
                                    value = ((Substream)value).ToArray();
                                }

                                propertyInfo.SetValue(obj, value);
                                break;
                            }
                        }
                    }

                    arrayList.Add(obj);
                }
            }

            return arrayList;
        }
    }
}
