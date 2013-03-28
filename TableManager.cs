using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace DbTableBuilder
{
    class TableManager
    {
        private Assembly innerAssembly;

        public void LoadAssembly(string fileName)
        {
            innerAssembly = Assembly.LoadFile(fileName);
        }

        public IList<string> GetNamespaceNames()
        {
            var ns = from obj in innerAssembly.GetTypes()
                     where obj.Namespace.Contains("Model")
                     select obj.Namespace;
            return ns.Distinct().ToList();
        }

        public IList<string> GetTypeNames(string nsName)
        {
            var types = from obj in innerAssembly.GetTypes()
                        where obj.Namespace == nsName
                        select obj.FullName;
            return types.ToList();
        }

        public string CreateSqlScript(string nsName, string[] typeNames)
        {
            var types = from obj in innerAssembly.GetTypes()
                        where obj.Namespace == nsName && typeNames.Contains(obj.FullName) && !obj.IsEnum
                        select obj;
            MysqlBuilder builder = new MysqlBuilder();
            StringBuilder sb = new StringBuilder();
            foreach (var type in types.ToList())
            {
                sb.Append(builder.GenerateSql(type));
            }
            return sb.ToString();
        }
    }
}
