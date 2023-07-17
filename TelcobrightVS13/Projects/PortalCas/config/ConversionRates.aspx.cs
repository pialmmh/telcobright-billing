using MediationModel;
using System;
using System.Web.UI.WebControls;

public partial class ConversionRates : System.Web.UI.Page
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
            context.uom_conversion_dated.Add(new uom_conversion_dated() {
                UOM_ID = ddlistFromCurrency.SelectedValue,
                UOM_ID_TO = ddlistToCurrency.SelectedValue,
                FROM_DATE = Convert.ToDateTime(txtDateFrom.Text),
                THRU_DATE = String.IsNullOrWhiteSpace(txtDateTo.Text) ? (DateTime?)null : Convert.ToDateTime(txtDateTo.Text),
                CONVERSION_FACTOR = Convert.ToDecimal(txtConversionRate.Text),
                PURPOSE_ENUM_ID = ddlPurpose.SelectedValue
            });
            context.SaveChanges();
            GridViewConversionRates.DataBind();
        }
    }

    protected void ddlistFromCurrency_DataBound(object sender, EventArgs e)
    {
        ddlistFromCurrency.SelectedValue = "USD";
    }

    protected void ddlistToCurrency_DataBound(object sender, EventArgs e)
    {
        ddlistToCurrency.SelectedValue = "BDT";
    }
}
