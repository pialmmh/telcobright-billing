using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InstallConfig._CommonValidation;
using LibraryExtensions;

namespace InstallConfig
{
    public class CdrValidationRuleGenerator
    {
        private DateTime NotAllowedCallDateTimeBefore { get; }
        private DirectoryInfo projectDirectory = 
            new DirectoryInfo(FileAndPathHelper.GetBinPath()).Parent.Parent.Parent;
        public CdrValidationRuleGenerator(DateTime notAllowedCallDateTimeBefore)
        {
            this.NotAllowedCallDateTimeBefore = notAllowedCallDateTimeBefore;
        }

        public void GenerateRulesAsMefAssemblies()
        {
            
        }

        void GenerateCommonCdrValidationRules()
        {
            CommonCdrValRulesTempl templClass=new CommonCdrValRulesTempl(this.NotAllowedCallDateTimeBefore);
            Dictionary<string, string> rulesWithMsg = templClass.GetCommonValidationRules();
            string targetFileName = this.projectDirectory.FullName + Path.DirectorySeparatorChar
                                             + "CdrValidationRules" + Path.DirectorySeparatorChar +
                                             "CommonCdrValRules.cs";
            WriteRulesToClassFile(rulesWithMsg,targetFileName,
                GetRulesContainerClassHeader("TelcobrightMediation"));
        }
        void GenerateInconsistentCdrValidationRules()
        {
            CommonCdrValRulesTempl templClass = new CommonCdrValRulesTempl(this.NotAllowedCallDateTimeBefore);
            Dictionary<string, string> rulesWithMsg = templClass.GetCommonValidationRules();
            string targetFileName = this.projectDirectory.FullName + Path.DirectorySeparatorChar
                                    + "CdrValidationRules" + Path.DirectorySeparatorChar +
                                    "InconsistentCdrValRules.cs";
            WriteRulesToClassFile(rulesWithMsg: rulesWithMsg, 
                targetRulesContainerClassFile: targetFileName,
                rulesContainerClassHeader: GetRulesContainerClassHeader("TelcobrightMediation"));
        }
        void WriteRulesToClassFile(Dictionary<string, string> rulesWithMsg,
            string targetRulesContainerClassFile,
            string rulesContainerClassHeader)
        {
            string mefTemplateForRuleClass = this.projectDirectory.FullName + Path.DirectorySeparatorChar
                                             + "UtilInstallConfig" + Path.DirectorySeparatorChar +
                                             "ValidationRuleGenerator" + Path.DirectorySeparatorChar +
                                             "CdrValidationRuleTemplate.txt";
            if (File.Exists(targetRulesContainerClassFile))
            {
                File.Delete(targetRulesContainerClassFile);
            }
            string validationRuleTypeToSetInExportMetaData = targetRulesContainerClassFile.Replace(".cs", "");
            using (StreamWriter writer = new StreamWriter(targetRulesContainerClassFile))
            {
                writer.Write(rulesContainerClassHeader);
                foreach (var kv in rulesWithMsg)
                {
                    string validationExpression = kv.Key;
                    string validationMessage = kv.Value;
                    string mefRuleClassName = validationMessage.Replace(" ", "");
                    if (validationExpression.StartsWith("ClassBody:"))
                    {
                        validationExpression = validationExpression.Replace("ClassBody", "");
                    }
                    else validationExpression= new StringBuilder("return ").Append(validationExpression)
                            .Append(";").ToString();
                    string classBody = File.ReadAllText(mefTemplateForRuleClass)
                        .Replace("RuleName", mefRuleClassName)
                        .Replace("CdrValidationRuleType",validationRuleTypeToSetInExportMetaData)
                        .Replace("ValidationMessage", validationMessage)
                        .Replace("Expression", validationExpression);
                    writer.Write(Environment.NewLine + classBody + Environment.NewLine);
                }
                writer.Write("}");
            }
        }

        string GetRulesContainerClassHeader(string nameSpace)
        {
            return   $@"using System;
                        using System.Collections.Generic;
                        using TelcobrightMediation;
                        using System.ComponentModel.Composition;
                        using MediationModel;
                        using LibraryExtensions;
                        namespace {nameSpace}
                        {{";
        }
    }
}
