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

                // Also ignore the properties that are not supportable (except FileInfo, Stream and ICollection<>)
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

                // Checks if all the rows have the same value for this field
                bool useDefaultValue = true;

                if (arrayList != null && arrayList.Count > 0)
                {
                    useDefaultValue = true;

                    foreach (object obj in arrayList)
                    {
                        if (propertyInfo.GetValue(obj) != propertyInfo.GetValue(arrayList[0]))
                        {
                            useDefaultValue = false;
                            break;
                        }
                    }

                    if (useDefaultValue)
                    {
                        defaultValue = propertyInfo.GetValue(arrayList[0]);
                    }
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
