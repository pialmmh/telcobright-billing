using System;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using WebApplication1.Models;

namespace WebApplication1.Account
{
    public partial class TwoFactorAuthenticationSignIn : System.Web.UI.Page
    {
        private ApplicationSignInManager _signinManager;
        private ApplicationUserManager _manager;

        public TwoFactorAuthenticationSignIn()
        {
            this._manager = this.Context.GetOwinContext().GetUserManager<ApplicationUserManager>();
            this._signinManager = this.Context.GetOwinContext().GetUserManager<ApplicationSignInManager>();
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            var userId = this._signinManager.GetVerifiedUserId<ApplicationUser, string>();
            if (userId == null)
            {
                this.Response.Redirect("/Account/Error", true);
            }
            var userFactors = this._manager.GetValidTwoFactorProviders(userId);
            this.Providers.DataSource = userFactors.Select(x => x).ToList();
            this.Providers.DataBind();            
        }

        protected void CodeSubmit_Click(object sender, EventArgs e)
        {
            bool rememberMe = false;
            bool.TryParse(this.Request.QueryString["RememberMe"], out rememberMe);
            
            var result = this._signinManager.TwoFactorSignIn<ApplicationUser, string>(this.SelectedProvider.Value, this.Code.Text, isPersistent: rememberMe, rememberBrowser: this.RememberBrowser.Checked);
            switch (result)
            {
                case SignInStatus.Success:
                    IdentityHelper.RedirectToReturnUrl(this.Request.QueryString["ReturnUrl"], this.Response);
                    break;
                case SignInStatus.LockedOut:
                    this.Response.Redirect("/Account/Lockout");
                    break;
                case SignInStatus.Failure:
                default:
                    this.FailureText.Text = "Invalid code";
                    this.ErrorMessage.Visible = true;
                    break;
            }
        }

        protected void ProviderSubmit_Click(object sender, EventArgs e)
        {
            if (!this._signinManager.SendTwoFactorCode(this.Providers.SelectedValue))
            {
                this.Response.Redirect("/Account/Error");
            }

            var user = this._manager.FindById(this._signinManager.GetVerifiedUserId<ApplicationUser, string>());
            if (user != null)
            {
                var code = this._manager.GenerateTwoFactorToken(user.Id, this.Providers.SelectedValue);
            }

            this.SelectedProvider.Value = this.Providers.SelectedValue;
            this.sendcode.Visible = false;
            this.verifycode.Visible = true;
        }
    }
}