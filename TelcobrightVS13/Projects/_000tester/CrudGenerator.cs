using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibraryExtensions;
using System.IO;
using MySql.Data.MySqlClient;
using System.Configuration;
using LibraryExtensions.EntityHelper;

namespace TelcobrightTester
{
    public class CrudGenerator
    {
        private string NameSpaceName { get; }
        private List<ClassDeclaration> Classes { get; }
        private List<string> NamespacesToInclude { get; set; }
        private bool partial { get; set; }
        private List<string> ClassesToOverrideProperties { get; set; }
        private List<string> ClassesToOverrideMethods { get; set; }
        List<string> ClassesToExcludeICacheble { get; set; }
        private List<string> ClassesToGenerateFluentValidatioinRules { get; set; }
        private Dictionary<string, List<ischema_columns>> TableWiseAttributes { get; set; }
        public CrudGenerator(string nameSpacename, bool partial, List<string> tables, List<string> namespacesToInclude,
            List<string> classesToOverrideProperties, List<string> classesToOverrideMethods,
            List<string> classesToExcludeICacheble,
            List<string> classesToGenerateFluentValidatioinRules)
        {
            this.ClassesToGenerateFluentValidatioinRules = classesToGenerateFluentValidatioinRules;
            this.ClassesToOverrideProperties = classesToOverrideProperties;
            this.ClassesToOverrideMethods = classesToOverrideMethods;
            this.ClassesToExcludeICacheble = classesToExcludeICacheble;
            this.NameSpaceName = nameSpacename;
            this.NamespacesToInclude = namespacesToInclude;
            this.partial = partial;
            Classes = new List<ClassDeclaration>();
            using (MySqlConnection con=new MySqlConnection(ConfigurationManager.ConnectionStrings["Partner"].ConnectionString))
            {
                con.Open();
                InformationSchemaRetriever ischemaRetriever = 
                    new InformationSchemaRetriever(tableNames:tables,con:con,databaseName:"platinum");
                TableWiseAttributes = ischemaRetriever.GetSchemaInformation();
            }
            
            foreach (KeyValuePair<string, List<ischema_columns>> kv in TableWiseAttributes)
            {
                bool generatePropertiesAsOverride = classesToOverrideProperties.Contains(kv.Key);
                bool generateMethodsAsOverride = classesToOverrideMethods.Contains(kv.Key);
                Classes.Add(new ClassDeclaration(kv.Key, kv.Value.Select(c => c.ColumnName).ToList(), this.partial,generatePropertiesAsOverride,generateMethodsAsOverride));
            }
        }
        public void GenerateCrudForAllClasses(string targetDir)
        {
            AdjustClassNamesToMatchCodeFirstSingularization(targetDir,this.Classes);
            GenerateAllExceptExtInsertEnums(targetDir);
            GenerateExtInsertColEnums(targetDir);
            ReWritePropertiesAsOverrideInMediationModel(targetDir);
        }

        void AdjustClassNamesToMatchCodeFirstSingularization(string crudDir, List<ClassDeclaration> classes)
        {
            string entityDir = (new DirectoryInfo(crudDir).Parent.Parent).FullName;
            List<string> codeFirstClassFileNames = Directory.GetFiles(entityDir, "*.cs", SearchOption.TopDirectoryOnly)
                .Select(c=> Path.GetFileName(c)).ToList();
            foreach (var classDeclaration in classes)
            {
                string dbTableName = classDeclaration.tableNameInDb;
                if (codeFirstClassFileNames.Contains(dbTableName+".cs")
                ) //db table name matches, no singularization
                {
                    classDeclaration.tableNameInCode = dbTableName;
                }
                else//s has been removed by code first generator
                {
                    string tableNameWithoutS = dbTableName.Left(dbTableName.Length-1);
                    if (codeFirstClassFileNames.Contains(tableNameWithoutS+".cs")//without last s
                    ) //db table name has been changed by codefirst by removing s
                    {
                        classDeclaration.tableNameInCode = tableNameWithoutS;
                    }
                }
            }
        }
        private void GenerateAllExceptExtInsertEnums(string targetDir)
        {
            foreach (ClassDeclaration c in this.Classes)
            {
                if (ClassesToExcludeICacheble.Contains(c.tableNameInDb)) continue;
                StringBuilder sbThisClass = c.GetClassGenerationString();
                using (StreamWriter sw = new StreamWriter(targetDir + Path.DirectorySeparatorChar + c.tableNameInCode + ".cs"))
                {
                    sw.WriteLine(string.Join(Environment.NewLine, NamespacesToInclude.Select(n => "using " + n + ";")));
                    sw.Write("namespace ");
                    sw.WriteLine(this.NameSpaceName);
                    sw.WriteLine("{");
                    sw.Write(sbThisClass.ToString());
                    sw.WriteLine("}");
                }
            }
        }
        private void GenerateExtInsertColEnums(string targetDir)
        {
            using (StreamWriter sw = new StreamWriter(targetDir + Path.DirectorySeparatorChar + "_StaticExtInsertColumnHeaders.cs"))
            {
                sw.WriteLine(string.Join(Environment.NewLine, NamespacesToInclude.Select(n => "using " + n + ";")));
                sw.Write("namespace ");
                sw.WriteLine(this.NameSpaceName);
                sw.WriteLine("{");
                sw.WriteLine("\tpublic static class StaticExtInsertColumnHeaders");
                sw.WriteLine("\t{");
                List<string> enumLines = Classes.Select(c => c.GetExtInsertColumns()).ToList();
                string sep = Environment.NewLine;
                sw.WriteLine(string.Join(sep, enumLines));
                sw.WriteLine("\t}");
                sw.WriteLine("}");
            }
        }
        public void ReWritePropertiesAsOverrideInMediationModel(string targetDir)
        {
            DirectoryInfo di = (new DirectoryInfo(targetDir)).Parent.Parent;
            targetDir = di.FullName;
            List<string> newLines = null;
            foreach (string className in ClassesToOverrideProperties)
            {
                newLines = new List<string>();
                string fileName = targetDir + Path.DirectorySeparatorChar + className + ".cs";
                List<string> lines = File.ReadAllLines(fileName).ToList();
                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    if ((line.Contains("public") && line.Contains("override")==false)//if override exists, skip
                        && line.Contains("get;") && line.Contains("set;"))
                    {
                        line = line.Replace("public", "public override");
                    }
                    newLines.Add(line);
                }
                //overwrite
                File.WriteAllLines(fileName, newLines);
            }
        }

        public void GenerateFluentValidationRulesets(List<string> classesForFluent)
        {
            string currentPath = System.IO.Path
                .GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                .Substring(6);
            DirectoryInfo di = new DirectoryInfo(currentPath);
            string targetDir = ((di.Parent).Parent).Parent.FullName + Path.DirectorySeparatorChar
                               + "Models_Mediation" + Path.DirectorySeparatorChar + "EntityExtensions" +
                               Path.DirectorySeparatorChar +
                               "Validators";
            List<ischema_columns> fieldsForThisClass;
            foreach (var c in classesForFluent)
            {
                fieldsForThisClass = this.TableWiseAttributes[c.ToLower()];
                StringBuilder sb=new StringBuilder();
                int startIndent = 1;
                string validatorClassName= c + "Validator";
                sb.AppendLineWithIndent("public class " +validatorClassName+ ": AbstractValidator<" + c + ">", startIndent);
                sb.AppendLineWithIndent("{", 2);
                sb.AppendLineWithIndent("public "+validatorClassName+"()", 3);
                sb.AppendLineWithIndent("{",4);
                //foreach (var iSchemaCols in fieldsForThisClass)
                //{
                //    string ruleSetNameForField="rs"
                //    string rulesetStr = $@"RuleSet(""rsFieldName", () =>
                //                            {{
                //                                RuleFor(c => c.FieldName).NotEmpty();
                //                            }});";
                //}
            }
        }

    }
}

