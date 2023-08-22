using System;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebApplication1.Models;

namespace WebApplication1.Account
{
    public partial class RegisterExternalLogin : System.Web.UI.Page
    {
        protected string ProviderName
        {
            get { return (string) this.ViewState["ProviderName"] ?? String.Empty; }
            private set { this.ViewState["ProviderName"] = value; }
        }

        protected string ProviderAccountKey
        {
            get { return (string) this.ViewState["ProviderAccountKey"] ?? String.Empty; }
            private set { this.ViewState["ProviderAccountKey"] = value; }
        }

        private void RedirectOnFail()
        {
            this.Response.Redirect((this.User.Identity.IsAuthenticated) ? "~/Account/Manage" : "~/Account/Login");
        }

        protected void Page_Load()
        {
            // Process the result from an auth provider in the request
            this.ProviderName = IdentityHelper.GetProviderNameFromRequest(this.Request);
            if (String.IsNullOrEmpty(this.ProviderName))
            {
                RedirectOnFail();
                return;
            }
            if (!this.IsPostBack)
            {
                var manager = this.Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
                var signInManager = this.Context.GetOwinContext().Get<ApplicationSignInManager>();
                var loginInfo = this.Context.GetOwinContext().Authentication.GetExternalLoginInfo();
                if (loginInfo == null)
                {
                    RedirectOnFail();
                    return;
                }
                var user = manager.Find(loginInfo.Login);
                if (user != null)
                {
                    signInManager.SignIn(user, isPersistent: false, rememberBrowser: false);
                    IdentityHelper.RedirectToReturnUrl(this.Request.QueryString["ReturnUrl"], this.Response);
                }
                else if (this.User.Identity.IsAuthenticated)
                {
                    // Apply Xsrf check when linking
                    var verifiedloginInfo = this.Context.GetOwinContext().Authentication.GetExternalLoginInfo(IdentityHelper.XsrfKey, this.User.Identity.GetUserId());
                    if (verifiedloginInfo == null)
                    {
                        RedirectOnFail();
                        return;
                    }

                    var result = manager.AddLogin(this.User.Identity.GetUserId(), verifiedloginInfo.Login);
                    if (result.Succeeded)
                    {
                        IdentityHelper.RedirectToReturnUrl(this.Request.QueryString["ReturnUrl"], this.Response);
                    }
                    else
                    {
                        AddErrors(result);
                        return;
                    }
                }
                else
                {
                    this.email.Text = loginInfo.Email;
                }
            }
        }        
        
        protected void LogIn_Click(object sender, EventArgs e)
        {
            CreateAndLoginUser();
        }

        private void CreateAndLoginUser()
        {
            if (!this.IsValid)
            {
                return;
            }
            var manager = this.Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
            var signInManager = this.Context.GetOwinContext().GetUserManager<ApplicationSignInManager>();
            var user = new ApplicationUser() { UserName = this.email.Text, Email = this.email.Text };
            IdentityResult result = manager.Create(user);
            if (result.Succeeded)
            {
                var loginInfo = this.Context.GetOwinContext().Authentication.GetExternalLoginInfo();
                if (loginInfo == null)
                {
                    RedirectOnFail();
                    return;
                }
                result = manager.AddLogin(user.Id, loginInfo.Login);
                if (result.Succeeded)
                {
                    signInManager.SignIn(user, isPersistent: false, rememberBrowser: false);

                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // var code = manager.GenerateEmailConfirmationToken(user.Id);
                    // Send this link via email: IdentityHelper.GetUserConfirmationRedirectUrl(code, user.Id)

                    IdentityHelper.RedirectToReturnUrl(this.Request.QueryString["ReturnUrl"], this.Response);
                    return;
                }
            }
            AddErrors(result);
        }

        private void AddErrors(IdentityResult result) 
        {
            foreach (var error in result.Errors) 
            {
                this.ModelState.AddModelError("", error);
            }
        }
    }
}