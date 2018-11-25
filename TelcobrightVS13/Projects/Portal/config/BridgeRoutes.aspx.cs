using MediationModel;
using System;
using System.Web.UI.WebControls;

public partial class BridgeRoutes : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }

    protected void EntityDataConversionRates_QueryCreated(object sender, QueryCreatedEventArgs e)
    {

    }

    protected void GridViewConversionRates_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonEdit");
                lnkBtn.CommandArgument = e.Row.RowIndex.ToString();

                lnkBtn = (LinkButton)e.Row.FindControl("LinkButtonDelete");
                lnkBtn.CommandArgument = e.Row.RowIndex.ToString();
            }
        }
    }

    protected void btnAdd_Click(object sender, EventArgs e)
    {
        using (PartnerEntities context = new PartnerEntities())
        {
            context.bridgedroutes.Add(new bridgedroute()
            {
                routeName = txtTGName.Text,
                inPartner = Convert.ToInt32(ddlistIncomingPartner.SelectedValue),
                outPartner = Convert.ToInt32(ddlistOutgoingPartner.SelectedValue)
            });
            context.SaveChanges();
            GridViewConversionRates.DataBind();
        }
    }

}
