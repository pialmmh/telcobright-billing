using System;
using System.Linq;
using System.Web;
using System.Web.UI;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using WebApplication1.Models;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;
using System.Configuration;
using MediationModel;
namespace WebApplication1.Account
{
    public partial class Register : Page
    {

        List<user> _users;//don't declare new
        List<UserVsRole> _userVsRoles;
        List<userrole> _lstUserRoles;
        Dictionary<string, role> _roles;
        [Serializable]
        public class UserVsRole
        {
            public string UserId { get; set; }
            public string UserName { get; set; }
            public string RoleId { get; set; }
            public string RoleName { get; set; } // so far handle one role only
            public string email { get; set; }
        }

        override protected void OnInit(EventArgs e)
        {
            Load += new EventHandler(Page_Load);
            base.OnInit(e);
        }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!this.IsPostBack)
            {

                BindGrid();
            }
        }

        private void BindGrid()
        {
            using (PartnerEntities context = new PartnerEntities())
            {
                this._users = context.users.OrderBy(c => c.Email).ToList();
                this._userVsRoles = new List<UserVsRole>();
                this._lstUserRoles = context.userroles.ToList();
                this._roles = context.roles.ToDictionary(c => c.Id);

                this.ViewState["users"] = this._users;
                this.ViewState["userVsRoles"] = this._userVsRoles;
                this.ViewState["roles"] = this._roles;

                this.DropDownListRole.Items.Clear();
                this.DropDownListRole.Items.Add(new ListItem("[Select]", "-1"));
                foreach (role rl in this._roles.Values)
                {
                    this.DropDownListRole.Items.Add(new ListItem(rl.Name, rl.Id));
                }
                foreach (user thisUser in this._users)
                {
                    userrole firstRole = this._lstUserRoles.Where(c => c.UserId == thisUser.Id).FirstOrDefault();
                    string firstRoleName = "";
                    string firstRoleId = "";
                    if (firstRole != null)
                    {
                        firstRoleName = this._roles[firstRole.RoleId.ToString()].Name;
                        firstRoleId = this._roles[firstRole.RoleId.ToString()].Id;
                    }
                    this._userVsRoles.Add(new UserVsRole()
                    {
                        UserId = thisUser.Id,
                        UserName = thisUser.UserName,
                        RoleName = firstRoleName,
                        RoleId = firstRoleId,
                        email = thisUser.Email,
                    }
                    );
                }
                this.GridViewUsers.DataSource = this._userVsRoles;
                this.GridViewUsers.DataBind();
                ShowEditUser(null, false);

            }
            this.LinkButtonNewUser.Visible = true;
        }


        protected void GridViewUsers_RowEditing(object sender, GridViewEditEventArgs e)
        {
            this.DivManageUser.Visible = true;
            this._userVsRoles = (List<UserVsRole>) this.ViewState["userVsRoles"];
            this.GridViewUsers.DataSource = this._userVsRoles;
            this.GridViewUsers.DataBind();
            this.GridViewUsers.EditIndex = e.NewEditIndex;
            string userId = this._userVsRoles[e.NewEditIndex].UserId;
            UserVsRole selectedUser = this._userVsRoles.Where(c => c.UserId == userId).FirstOrDefault();
            this.ViewState["selectedUser"] = selectedUser;
            if (selectedUser != null)
            {
                ShowEditUser(selectedUser, true);
            }
            this.GridViewUsers.EditIndex = -1;
            e.Cancel = true;
        }

        private void ShowEditUser(UserVsRole userVsRole, bool show)//call with null when hiding the edit part
        {

            if (show == true)//show edit part
            {
                this.DivManageUser.Visible = true;
            }
            else//hide edit part
            {
                this.DivManageUser.Visible = false;
            }

            if (userVsRole == null) return;
            this.Email.Text = userVsRole.email;
            if (userVsRole.email == "admin@telcobright.com")
            {
                this.DropDownListRole.Enabled = false;
            }
            else
            {
                this.DropDownListRole.Enabled = true;
            }

            if (userVsRole.email == "")//new user********
            {
                this.divPassword.Visible = true;
                this.Email.Enabled = true;
                this.divPasswordCaption.Visible = false;
                this.divPassword.Visible = true;
                this.button_save.CommandArgument = "new";
                this.DropDownListRole.SelectedIndex = 0;
            }
            else//edit existing user ********************
            {
                this.LinkButtonNewUser.Visible = false;
                this.divPasswordCaption.Visible = true;
                this.divPassword.Visible = false;
                this.Email.Enabled = false;
                this._roles = (Dictionary<string, role>) this.ViewState["roles"];
                role selectedRole = null;
                this._roles.TryGetValue(userVsRole.RoleId, out selectedRole);
                if (selectedRole != null)
                {
                    this.DropDownListRole.SelectedValue = selectedRole.Id;
                }
                else
                {
                    this.DropDownListRole.SelectedIndex = 0;
                }
                this.button_save.CommandArgument = "edit";
            }



        }

        protected class NewUser
        {
            public NewUser(List<string> LstRoleIds)
            {
                this.LstRoleIds = LstRoleIds;
            }
            public string GetHashedPassword(string clearPassword)
            {
                PasswordHasher pHasher = new PasswordHasher();
                return pHasher.HashPassword(clearPassword);
            }
            public List<string> LstRoleIds { get; set; }
            public string EditMode { get; set; }
            public string email { get; set; }
            public string UserId { get; set; }
            public string RoleId { get; set; }
            public string password { get; set; }
            public string confirmPassword { get; set; }

            private string EmptyValidation(string val)
            {
                if (string.IsNullOrEmpty(val) || string.IsNullOrWhiteSpace(val))
                {
                    return " cannot be empty!";
                }
                return "";
            }
            public string EmailValidate()
            {
                return EmptyValidation(this.email);
            }
            public string RoleValidate()
            {
                if (this.LstRoleIds.Contains(this.RoleId) == false)
                {
                    return "Role cannot be Empty!";
                }
                return "";
            }
            public string PasswordEmptyValidate()
            {
                string passValidate = EmptyValidation(this.password);
                string passConfirmValidate = EmptyValidation(this.confirmPassword);
                if (passValidate != "")
                {
                    return "Password" + passValidate;
                }
                else if (passConfirmValidate != "")
                {
                    return "Confirm password" + passConfirmValidate;
                }
                else
                {
                    return "";
                }
            }
            public string PasswordMatchValidation()
            {
                if (this.password != this.confirmPassword)
                {
                    return "Password and confirm password do not match!";
                }
                return "";
            }
            public void InsertUser(HttpContext context)
            {
                //unfortunately couldn't implement transaction as create user part uses a securitystamp value...
                var manager = context.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var signInManager = context.GetOwinContext().Get<ApplicationSignInManager>();
                var user = new ApplicationUser() { UserName = this.email, Email = this.email };
                IdentityResult result = manager.Create(user, this.password);
                //fetch the newly created userid, if not when re-creating user for password change, the old id gets used.
                MySqlConnection con = new MySqlConnection(
                ConfigurationManager.ConnectionStrings["Partner"].ConnectionString);
                if (con.State != System.Data.ConnectionState.Open) con.Open();
                using (MySqlCommand cmd = new MySqlCommand(
                    "select id from users where email='" + this.email + "'", con))
                {
                    MySqlDataReader reader = cmd.ExecuteReader();
                    while(reader.Read())
                    {
                        this.UserId = reader[0].ToString();
                        break;
                    }
                    reader.Close();
                }
                
                if (result.Succeeded)
                {
                    UpdateRoles(con);
                }
                else
                {
                    if (result.Errors != null)
                    {
                        throw new Exception(result.Errors.FirstOrDefault());
                    }
                    throw new Exception("Unspecified error occured while creating user!");
                }

            }
            public void UpdateUser()
            {
                MySqlConnection con = new MySqlConnection(
                ConfigurationManager.ConnectionStrings["Partner"].ConnectionString);
                if (con.State != System.Data.ConnectionState.Open) con.Open();
                using (MySqlCommand cmd = new MySqlCommand("", con))//transaction
                {
                    try
                    {
                        cmd.CommandText = "set autocommit=0;";
                        cmd.ExecuteNonQuery();
                        //role part
                        UpdateRoles(con);
                        cmd.CommandText = "commit;";
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        cmd.CommandText = "rollback;";
                        cmd.ExecuteNonQuery();
                        throw new Exception(e.Message);
                    }
                }
            }
            public void UpdateRoles(MySqlConnection con)
            {
                using (MySqlCommand cmd = new MySqlCommand("", con))//transaction
                {
                    if (con.State != System.Data.ConnectionState.Open) con.Open();
                    if (this.EditMode == "new")
                    {
                        cmd.CommandText = "insert into userroles (userid,roleid) values (" +
                            "'" + this.UserId + "'," + this.RoleId + ")";
                        cmd.ExecuteNonQuery();
                    }
                    else//update
                    {
                        cmd.CommandText = "delete from userroles where userid='" + this.UserId + "';";
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "insert into userroles (userid,roleid) values (" +
                            "'" + this.UserId + "'," + this.RoleId + ")";
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private bool ValidateNewAndEditUser(NewUser newObj)
        {
            string validationText = newObj.EmailValidate();
            if (validationText != "")
            {
                this.lblStatus.ForeColor = System.Drawing.Color.Red;
                this.lblStatus.Text = validationText;
                return false;
            }
            validationText = newObj.RoleValidate();
            if (validationText != "")
            {
                this.lblStatus.ForeColor = System.Drawing.Color.Red;
                this.lblStatus.Text = validationText;
                return false;
            }
            return true;
        }
        private bool ValidateNewUser(NewUser newObj)
        {

            string validationText = newObj.PasswordEmptyValidate();
            if (validationText != "")
            {
                this.lblStatus.ForeColor = System.Drawing.Color.Red;
                this.lblStatus.Text = validationText;
                return false;
            }
            validationText = newObj.PasswordMatchValidation();
            if (validationText != "")
            {
                this.lblStatus.ForeColor = System.Drawing.Color.Red;
                this.lblStatus.Text = validationText;
                return false;
            }
            return true;
        }
        protected void CreateUser_Click(object sender, EventArgs e)
        {
            try
            {
                var dicRoles = (Dictionary<string, role>) this.ViewState["roles"];
                var users = (List<user>) this.ViewState["users"];
                string editMode = ((Button)sender).CommandArgument;//new or edit
                NewUser newObj = new NewUser(dicRoles.Values.Select(c => c.Id).ToList())
                {
                    EditMode = editMode,
                    email = this.Email.Text,
                    RoleId = this.DropDownListRole.SelectedValue,
                    password = this.Password.Text,
                    confirmPassword = this.ConfirmPassword.Text,
                };
                if (ValidateNewAndEditUser(newObj) == false) return;
                if (editMode == "new")
                {
                    if (ValidateNewUser(newObj) == false) return;
                    newObj.InsertUser(this.Context);
                    this.lblStatus.ForeColor = System.Drawing.Color.Green;
                    this.lblStatus.Text = "User created successfully!";
                    BindGrid();
                }
                else//edit
                {
                    //new will will have userid in edit mode
                    newObj.UserId = users.Where(c => c.Email == this.Email.Text).Select(c => c.Id).First();
                    if (this.Password.Text.Trim() == "" && this.ConfirmPassword.Text.Trim() == "")//use didn't touch password parts, just update
                    {
                        newObj.UpdateUser();
                    }
                    else//user meant to change password, delete and re-create newUser again
                    {
                        if (ValidateNewUser(newObj) == false) return;
                        DeleteUser(newObj.UserId);//this needs to be changed to delete roles as well, clean up roles table for orfan record
                        //reset id, which will be autogenerated
                        newObj.UserId = "";
                        newObj.InsertUser(this.Context);
                    }
                    BindGrid();
                    this.lblStatus.ForeColor = System.Drawing.Color.Green;
                    this.lblStatus.Text = "User updated successfully!";

                }
            }
            catch (Exception e1)
            {
                this.lblStatus.ForeColor = System.Drawing.Color.Red;
                this.lblStatus.Text = e1.Message;
            }

        }



        protected void Cancel_Click(object sender, EventArgs e)
        {
            UserVsRole selectedUser = (UserVsRole) this.ViewState["selectedUser"];
            ShowEditUser(selectedUser, false);
            this.LinkButtonNewUser.Visible = true;
            this.lblStatus.Text = "";
        }



        protected void LinkButton1_Click(object sender, EventArgs e)
        {
            this.divPassword.Visible = true;
        }

        protected void LinkButtonNewUser_Click(object sender, EventArgs e)
        {
            ShowEditUser(new UserVsRole() { email = "" }, true);//differentiate between null & new record
            this.LinkButtonNewUser.Visible = false;
        }

        protected void GridViewUsers_RowDataBound(object sender, GridViewRowEventArgs e)
        {

            //if (e.Row.RowType == DataControlRowType.DataRow)
            //{

            //    if ((e.Row.RowState & DataControlRowState.Edit) > 0) return;

            //    if (e.Row.RowType == DataControlRowType.DataRow)
            //    {
            //        var LnkBtn = (LinkButton)e.Row.FindControl("LinkButtonDelete");
            //        LnkBtn.CommandArgument = DataBinder.Eval(e.Row.DataItem, "userId").ToString();
            //    }
            //}
        }

        protected void GridViewUsers_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            string userid = ((Label) this.GridViewUsers.Rows[e.RowIndex].FindControl("lblUserId")).Text;
            DeleteUser(userid);
            BindGrid();
        }
        private void DeleteUser(string userid)
        {
            MySqlConnection con = new MySqlConnection(
                ConfigurationManager.ConnectionStrings["Partner"].ConnectionString);
            if (con.State != System.Data.ConnectionState.Open) con.Open();
            using (MySqlCommand cmd = new MySqlCommand("", con))//transaction
            {
                try
                {
                    cmd.CommandText = "set autocommit=0;";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "delete from userroles where userid='" + userid + "'";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText="delete from users where id='" + userid + "'";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "commit;";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    cmd.CommandText = "rollback;";
                    cmd.ExecuteNonQuery();
                    throw new Exception(e.Message);
                }
            }
        }

        protected void LinkButtonLoginHistory_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/AccountAdmin/LoginHistory.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }

}