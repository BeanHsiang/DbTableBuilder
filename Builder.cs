using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using BLToolkit.DataAccess;
using BLToolkit.Mapping;
using BLToolkit.Validation;
using System.Collections;

namespace DbTableBuilder
{
    internal class MysqlBuilder
    {
        private T GetAttribute<T>(MemberInfo obj) where T : Attribute
        {
            object[] attrs = obj.GetCustomAttributes(typeof(T), false);
            if (attrs.Length == 0)
            {
                return null;
            }
            else
            {
                return attrs[0] as T;
            }
        }

        private string ConverToDbType(Type type, int length)
        {
            string dbType;
            switch (type.Name)
            {
                case "Int32":
                    dbType = "int";
                    break;
                case "Int64":
                    dbType = "int";
                    break;
                case "String":
                    dbType = length == 0 ? "longtext" : string.Format("varchar({0})", length);
                    break;
                case "Boolean":
                    dbType = "bool";
                    break;
                case "DateTime":
                    dbType = "datetime";
                    break;
                default:
                    dbType = "longtext";
                    break;
            }
            return dbType;
        }

        private StringBuilder CreateColumnName(PropertyInfo property)
        {
            var sb = new StringBuilder();
            var mapFieldAttr = GetAttribute<MapFieldAttribute>(property);
            var columnName = property.Name;
            if (mapFieldAttr != null)
            {
                columnName = mapFieldAttr.MapName;
            }
            sb.AppendFormat("{0} ", columnName);
            return sb;
        }

        private string GenerateColumn(PropertyInfo property)
        {
            var sb = CreateColumnName(property);
            var length = 0;
            var maxLengthAttr = GetAttribute<MaxLengthAttribute>(property);
            if (maxLengthAttr != null)
            {
                length = maxLengthAttr.Value;
            }
            sb.AppendFormat("{0} ", ConverToDbType(property.PropertyType, length));

            var notNullAttr = GetAttribute<NotNullAttribute>(property);
            if (notNullAttr != null)
            {
                sb.Append("not null ");
            }

            var auatoIncrementAttr = GetAttribute<IdentityAttribute>(property);
            if (auatoIncrementAttr != null)
            {
                sb.Append("AUTO_INCREMENT ");
            }

            var defaultlAttr = GetAttribute<DefaultValueAttribute>(property);
            if (defaultlAttr != null)
            {
                sb.AppendFormat("DEFAULT '{0}'", defaultlAttr.Value);
            }
            //return string.Format("{0},{1}", sb.ToString().TrimEnd(), Environment.NewLine);
            return sb.ToString().TrimEnd();
        }

        private string GenerateEnum(PropertyInfo property)
        {
            var sb = CreateColumnName(property);
            var mapValueAttr = GetAttribute<MapValueAttribute>(property.PropertyType);
            if (mapValueAttr != null)
            {
                sb.AppendFormat("{0} ", ConverToDbType(mapValueAttr.Values.First().GetType(), 50));
            }
            else
            {
                var fields = property.PropertyType.GetFields(BindingFlags.Public | BindingFlags.Static);
                if (fields.Length == 0)
                {
                    sb.Append("int ");
                }
                else
                {
                    mapValueAttr = GetAttribute<MapValueAttribute>(fields.First());
                    sb.AppendFormat("{0} ", mapValueAttr != null ? ConverToDbType(mapValueAttr.Values.First().GetType(), 50) : "int");
                }
            }
            return sb.ToString().TrimEnd();
        }

        private string GemerateKey(string tableName, PropertyInfo property, AssociationAttribute association)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("KEY `{0}` (`{0}`),", association.ThisKey);
            sb.Append(Environment.NewLine);
            sb.AppendFormat("CONSTRAINT `{0}_{1}` FOREIGN KEY (`{2}`) REFERENCES `{3}` (`{4}`)", tableName, property.Name, association.ThisKey, property.PropertyType.Name, association.OtherKey);
            return sb.ToString();
        }

        public string GenerateSql(Type type)
        {
            var sb = new StringBuilder();
            //object[] attrs = type.GetCustomAttributes(typeof(TableNameAttribute), false);
            string tableName = type.Name;
            var attr = GetAttribute<TableNameAttribute>(type);
            if (attr != null)
            {
                tableName = attr.Name;
            }
            sb.AppendFormat("-- Table structure for table {0}", tableName);
            sb.Append(Environment.NewLine);
            sb.AppendFormat("DROP TABLE IF EXISTS `{0}`;", tableName);
            sb.Append(Environment.NewLine);

            sb.AppendFormat("CREATE TABLE `{0}` (", tableName);
            sb.Append(Environment.NewLine);

            var columns = new ArrayList();
            foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var noMapAttr = GetAttribute<MapIgnoreAttribute>(property);
                if (noMapAttr != null)
                {
                    continue;
                }
                var association = GetAttribute<AssociationAttribute>(property);
                if (property.PropertyType.IsEnum)
                {
                    columns.Add(GenerateEnum(property));
                }
                else if (association != null)
                {
                    //columns.Add(GemerateKey(tableName, property, association));
                }
                else
                {
                    var columnSql = GenerateColumn(property);
                    if (columnSql.Length > 0)
                    {
                        columns.Add(columnSql);
                    }
                }
            }
            var primaryProperties = (from property in type.GetProperties()
                                     where GetAttribute<PrimaryKeyAttribute>(property) != null
                                     select string.Format("PRIMARY KEY (`{0}`)", property.Name)).ToArray();

            if (primaryProperties.Length > 0)
            {
                columns.Add(primaryProperties[0]);
            }
            var columnStr = String.Join("," + Environment.NewLine, columns.ToArray());
            sb.Append(columnStr);
            sb.AppendFormat("{0}) ENGINE=InnoDB DEFAULT CHARSET=utf8;", Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
    }
}

