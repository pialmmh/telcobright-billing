using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using DevExpress.XtraReports.UI;

namespace ReportGenerator.reports.invoice.igw
{
    public partial class InternationalIncomingToForeignCarrier : DevExpress.XtraReports.UI.XtraReport
    {
        public InternationalIncomingToForeignCarrier(InvoiceDataForReport invoiceDataForReport)
        {
            InitializeComponent();
            generateReport(invoiceDataForReport);
        }

        private void generateReport(InvoiceDataForReport invoiceDataForReport)
        {
            #region Page Header
            #endregion

            #region Report Body
            #endregion

            #region Report Footer
            xrLabelPaymentAdvice.Text = "Account Name: Bangla Tel Ltd.\r\n" +
                "A / C No. 13251120032737\r\n" +
                "Bank Name: Prime Bank Limited, Banani Branch\r\n" +
                "Bank Address: House # 62, Block # E, Kemal Ataturk Avenue,\r\n" +
                "Banani, Dhaka 1213, Bangladesh\r\n" +
                "SWIFT code: PRBLBDDH020";

            xrLabelAddress.Text = "Red Crescent Borak Tower, Level-M, 37/3/A, Eskaton Garden Road, Dhaka-1000, Bagnladesh\r\n" +
                "PABX: +88028332924, 9334781, 9334782, Fax: +8802833275, Email: info @banglatel.com.bd\r\n" +
                "Website : www.banglatel.com.bd";
            #endregion
        }
    }
}
