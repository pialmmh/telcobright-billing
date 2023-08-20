using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MediationModel;
using Spring.Core.TypeResolution;
using Spring.Expressions;
using TelcobrightMediation;
using TelcobrightMediation.Config;

/// <summary>
/// Summary description for CommonCode
/// </summary>

namespace PortalApp._portalHelper
{
    public static class PageUtil
    {
        public static string GetOperatorName() {
            string conStrPartner = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;
            string dbNameAppConf = "";
            foreach (string param in conStrPartner.Split(';'))
            {
                if (param.ToLower().Contains("database"))
                {
                    dbNameAppConf = param.Split('=')[1].Trim('"');
                    break;
                }
            }
            telcobrightpartner thisPartner = null;
            string binpath = System.Web.HttpRuntime.BinDirectory;
            TelcobrightConfig telcobrightConfig = PageUtil.GetTelcobrightConfig();
            using (PartnerEntities conTelco = new PartnerEntities())
            {
                thisPartner = conTelco.telcobrightpartners.Where(c => c.databasename == dbNameAppConf).ToList().First();
            }
            return thisPartner.CustomerName;
        }
        public static string GetPortalBinPath()
        {
            return HttpContext.Current.Server.MapPath("~/bin");
        }
        public static TelcobrightConfig GetTelcobrightConfig()
        {
            string operatorShortName = (ConfigurationManager.ConnectionStrings["Partner"].ConnectionString
                .Split(';').FirstOrDefault(c => c.StartsWith("database"))).Split('=').Last().Trim();
            string portalConfigFileName= string.Join(Path.DirectorySeparatorChar.ToString(), (Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)).Split(Path.DirectorySeparatorChar).Where(c => c.Contains("file") == false)) + Path.DirectorySeparatorChar.ToString() 
                + operatorShortName + ".conf";
            TelcobrightConfig telcobrightConfig = ConfigFactory.GetConfigFromFile(portalConfigFileName);
            return telcobrightConfig;
        }
        public static List<role> GetRoles()
        {
            var user = HttpContext.Current.User;
            string userid = "";
            List<string> roleIds = new List<string>();
            List<role> currentRoles = new List<role>();
            using (PartnerEntities context = new PartnerEntities())
            {
                user currentUser = context.users.Where(c => c.UserName == user.Identity.Name).ToList().FirstOrDefault();
                if (currentUser != null)
                {
                    userid = currentUser.Id;
                    roleIds = context.userroles.Where(r => r.UserId == userid).Select(r => r.RoleId).ToList();
                    currentRoles = context.roles.Where(c => roleIds.Contains(c.Id)).ToList();
                }
            }
            return currentRoles;
        }

        public static void ApplyPageSettings(object pageAsObjet, bool isMasterPage, TelcobrightConfig tbc)
        {
            Page aspxPage = null;
            MasterPageExtendedForSpring masterExt = null;
            if (isMasterPage == true)
            {
                masterExt = (MasterPageExtendedForSpring)(pageAsObjet);
                TypeRegistry.RegisterType("MasterPageExtendedForSpring", typeof(MasterPageExtendedForSpring));//for spring based page settings
            }
            else
            {
                aspxPage = (Page)(pageAsObjet);
            }
            List<role> currentRoles = GetRoles();
            List<string> currentRoleNames = currentRoles.Select(c => c.Name).ToList();
            string pagePath = aspxPage != null ? aspxPage.AppRelativeVirtualPath : masterExt.MasterPage.AppRelativeVirtualPath.ToLower();//use tolower for master because got different results during execution
            List<SettingByRoles> settingByRolesForOnePage = tbc.PortalSettings.PageSettings.DicPageSettingsByRole[pagePath];

            if (settingByRolesForOnePage != null)
            {
                if (settingByRolesForOnePage != null)
                {
                    List<SettingByRoles> settingsForAllRoles = settingByRolesForOnePage.Where(c => c.RoleNames.Contains("*")).ToList();//for all roles
                    List<SettingByRoles> settingsForOtherRoles = settingByRolesForOnePage.Except(settingsForAllRoles).ToList();
                    foreach (SettingByRoles settingByRoles in settingsForAllRoles)
                    {
                        if (settingByRoles.RoleNames.Contains("*"))//for all role
                        {
                            foreach (string springProperty in settingByRoles.SpringExpressionIfRole)
                            {
                                var tempArr = springProperty.Split('=');
                                ExpressionEvaluator.SetValue(pageAsObjet, tempArr[0], tempArr[1]);
                            }
                        }
                    }
                    //for other roles    
                    bool isRoleApplied = false;
                    foreach (SettingByRoles settingByRoles in settingsForOtherRoles)
                    {
                        if (settingByRoles.RoleNames.Intersect(currentRoleNames).Any())//apply settings if Role
                        {
                            foreach (string springProperty in settingByRoles.SpringExpressionIfRole)
                            {
                                var tempArr = springProperty.Split('=');
                                ExpressionEvaluator.SetValue(pageAsObjet, tempArr[0], tempArr[1]);
                            }
                            isRoleApplied = true;
                        }
                        else//apply settings if not Role
                        {
                            foreach (string springProperty in settingByRoles.SpringExpressionIfNotRole)
                            {
                                var tempArr = springProperty.Split('=');
                                ExpressionEvaluator.SetValue(pageAsObjet, tempArr[0], tempArr[1]);
                            }
                        }
                        if (isRoleApplied) break;
                    }
                }
            }
        }//apply page settings

    }



    public class CommonCode
    {





        public Dictionary<string, TreeNode> DicTreeState;

        public CommonCode()
        {
            //
            // TODO: Add constructor logic here
            //
            this.DicTreeState = new Dictionary<string, TreeNode>();
        }





        public void GetAllTreeNodes(TreeNode node, ref Dictionary<string, TreeNode> dicTreeState)
        {

            dicTreeState.Add(node.ValuePath, node);
            foreach (TreeNode tn in node.ChildNodes)
            {
                if (tn.ChildNodes.Count != 0)
                {
                    for (int i = 0; i < tn.ChildNodes.Count; i++)
                    {
                        GetAllTreeNodes(tn.ChildNodes[i], ref dicTreeState);
                    }
                }
            }
        }

        public TreeNode RetrieveNodes(TreeNode node, string urlToLookUp)
        {
            TreeNode selectedNode = null;
            if (node.NavigateUrl.ToLower() == urlToLookUp.ToLower())
            {
                selectedNode = node;
                return selectedNode;
            }
            foreach (TreeNode tn in node.ChildNodes)
            {
                if (tn.NavigateUrl.ToLower() == urlToLookUp.ToLower())
                {
                    tn.Selected = true;
                    selectedNode = tn;
                    return selectedNode;
                }
                else
                {
                    if (tn.ChildNodes.Count != 0)
                    {
                        //navurl was getting changed for unknown reason, who knows why on earth...
                        string myNavUrl = urlToLookUp;//adding this line solved that problem
                        for (int i = 0; i < tn.ChildNodes.Count; i++)
                        {
                            selectedNode = RetrieveNodes(tn.ChildNodes[i], myNavUrl);
                            if (selectedNode != null) return selectedNode;
                        }
                    }
                }
            }
            return selectedNode;
        }

        public Control FindControlRecursive(Control root, string id)
        {
            if (root.ID == id)
                return root;

            foreach (Control ctl in root.Controls)
            {
                Control foundCtl = FindControlRecursive(ctl, id);
                if (foundCtl != null)
                    return foundCtl;
            }

            return null;
        }

        public void LoadReportTemplatesTree(ref TreeView treeView1)
        {
            //load Report Templates from database
            //clear existing nodes first to avoid duplication
            TreeNode nPrev = treeView1.FindNode("Report Templates");
            nPrev.ChildNodes.Clear();
            using (PartnerEntities context = new PartnerEntities())
            {
                TreeNode n = treeView1.FindNode("Report Templates");
                foreach (reporttemplate thisTempl in context.reporttemplates.OrderBy(c => c.Templatename).ToList())
                {
                    TreeNode tempNode = new TreeNode();
                    int qMarkExists = thisTempl.PageUrl.IndexOf('?');
                    tempNode.NavigateUrl = thisTempl.PageUrl +
                        (qMarkExists < 0 ? "?templ=" : ",templ=") +
                        thisTempl.Templatename;
                    tempNode.Text = thisTempl.Templatename;
                    tempNode.Value = thisTempl.Templatename;
                    tempNode.Expanded = true;
                    n.ChildNodes.Add(tempNode);
                }
            }
        }

        public string SaveTemplateControlsByPage(Control thisPage, string templateName, string pageUrl)
        {
            //find id="MainContent"
            try
            {
                Control contentPage = FindControlRecursive(thisPage, "MainContent");
                if (contentPage == null)
                {
                    return "Could not find Content Page!";
                }

                List<Control> lstPageControls = new List<Control>();
                foreach (Control ctl in contentPage.Controls)
                {
                    lstPageControls.Add(ctl);
                }
                foreach (Control ctl in contentPage.Controls)//add updatepanel controls
                {
                    if (ctl.ID.StartsWith("UpdatePanel") == true)
                    {
                        //ControlValues += Ctl.ID + ":textbox:" + ((TextBox)Ctl).Text + ":" + ((TextBox)Ctl).Enabled + ",";
                        foreach (Control ucl in ((UpdatePanel)ctl).ContentTemplateContainer.Controls)
                        {
                            lstPageControls.Add(ucl);
                        }
                    }
                }
                string controlValues = "";
                foreach (Control ctl in lstPageControls)
                {
                    if (ctl.ID == "TextBoxDuration")
                    {
                        controlValues += ctl.ID + ":textbox:" + ((TextBox)ctl).Text + ":" + ((TextBox)ctl).Enabled + ",";
                    }
                    if (ctl.GetType() == typeof(CheckBox))
                    {
                        controlValues += ctl.ID + ":checkbox:" + ((CheckBox)ctl).Checked + ":" + ((CheckBox)ctl).Enabled + ",";
                    }
                    else if (ctl.GetType() == typeof(DropDownList))
                    {
                        controlValues += ctl.ID + ":dropdownlist:" + ((DropDownList)ctl).SelectedValue + ":" + ((DropDownList)ctl).Enabled + ",";
                    }
                    else if (ctl.GetType() == typeof(RadioButton))
                    {
                        controlValues += ctl.ID + ":radiobutton:" + ((RadioButton)ctl).Checked + ":" + ((RadioButton)ctl).Enabled + ",";
                    }
                    else if (ctl.GetType() == typeof(TextBox))
                    {
                        controlValues += ctl.ID + ":textbox:" + ((TextBox)ctl).Enabled + ":" + ((TextBox)ctl).Enabled + ",";
                    }
                }
                controlValues = controlValues.Substring(0, controlValues.Length - 1);
                //for some reason, controle values were getting :: sometime, replace them with "
                controlValues = controlValues.Replace("::", ":");
                using (PartnerEntities context = new PartnerEntities())
                {
                    reporttemplate newTemplate = new reporttemplate();
                    newTemplate.Templatename = templateName;
                    newTemplate.ControlValues = controlValues;
                    newTemplate.PageUrl = pageUrl;
                    context.reporttemplates.Add(newTemplate);
                    context.SaveChanges();
                }
                return "success";
            }
            catch (Exception e1)
            {
                return e1.InnerException.Message;
            }
        }

        public string SetTemplateControls(Control thisPage, string templateName)
        {
            //find id="MainContent"
            try
            {
                //string DropdownlistSValues = "";
                Control contentPage = FindControlRecursive(thisPage, "MainContent");
                if (contentPage == null)
                {
                    return "Could not find Content Page!";
                }

                string controlValues = "";
                using (PartnerEntities context = new PartnerEntities())
                {
                    controlValues = context.reporttemplates.Where(c => c.Templatename == templateName).First().ControlValues;
                }
                if (controlValues != "")
                {
                    string[] controlsAsStrArr = controlValues.Split(',');
                    long realTimeDuration = 30;
                    //sort descenting order, so that textboxduration comes first...
                    foreach (string strThisControl in controlsAsStrArr.OrderByDescending(c => c).ToArray())
                    {
                        string[] controlAttributes = strThisControl.Split(':');
                        if (controlAttributes[0] == "TextBoxDuration")
                        {
                            TextBox thisControl = (TextBox)contentPage.FindControl("TextBoxDuration");
                            thisControl.Text = controlAttributes[2];
                            if (!long.TryParse(controlAttributes[2], out realTimeDuration))
                            {
                                realTimeDuration = 30;
                            }
                        }
                        else if (controlAttributes[1] == "checkbox")
                        {
                            CheckBox thisControl = (CheckBox)contentPage.FindControl(controlAttributes[0]);
                            thisControl.Checked = (controlAttributes[2] == "True" ? true : false);
                            if (controlAttributes[3] != "") thisControl.Enabled = (controlAttributes[3] == "True" ? true : false);
                            if (thisControl.ID == "CheckBoxRealTimeUpdate" && thisControl.Checked == true)
                            {
                                TextBox textBoxDuration = (TextBox)contentPage.FindControl("TextBoxDuration");

                                //TextBoxDuration.Enabled = true;
                                //set current timespan in text boxes
                                DateTime endtime = DateTime.Now;
                                DateTime starttime = endtime.AddMinutes(realTimeDuration * (-1));
                                TextBox txtDate1 = (TextBox)contentPage.FindControl("txtDate1");
                                TextBox txtDate = (TextBox)contentPage.FindControl("txtDate");
                                txtDate1.Text = endtime.ToString("dd/MM/yyyy HH:mm:ss");
                                txtDate.Text = starttime.ToString("dd/MM/yyyy HH:mm:ss");

                            }
                        }
                        else if (controlAttributes[1] == "radiobutton")
                        {
                            RadioButton thisControl = (RadioButton)contentPage.FindControl(controlAttributes[0]);
                            thisControl.Checked = (controlAttributes[2] == "True" ? true : false);
                            if (controlAttributes[3] != "") thisControl.Enabled = (controlAttributes[3] == "True" ? true : false);
                        }
                        else if (controlAttributes[1] == "dropdownlist")
                        {
                            DropDownList thisControl = (DropDownList)contentPage.FindControl(controlAttributes[0]);
                            thisControl.DataBind();
                            thisControl.SelectedValue = int.Parse(controlAttributes[2].ToString()).ToString();//selected value doesn't work here due to no databinding
                                                                                                              //DropdownlistSValues+=ThisControl.ID+"=" + int.Parse(ControlAttributes[2].ToString()).ToString()+",";
                            if (controlAttributes[3] != "") thisControl.Enabled = (controlAttributes[3] == "True" ? true : false);
                        }
                        else if (controlAttributes[1] == "textbox")
                        {
                            TextBox thisControl = (TextBox)contentPage.FindControl(controlAttributes[0]);
                            if (controlAttributes[3] != "") thisControl.Enabled = (controlAttributes[3] == "True" ? true : false);
                        }
                    }

                }


                return "success";
                //return DropdownlistSValues;
            }
            catch (Exception e1)
            {
                return e1.InnerException.Message;
            }
        }

    }
}
