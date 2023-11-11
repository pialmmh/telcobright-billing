using System.IO;
using System.Web;
using System.Web.Services;
using System.Web.SessionState;

namespace PortalApp.Handler
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class PdfHandler : IHttpHandler, IReadOnlySessionState
    {
        private string documentFullname;

        public PdfHandler(string documentFullname)
        {
            this.documentFullname = documentFullname;
        }

        public void ProcessRequest(HttpContext context)
        {
            CreateImage(context);
        }

        private void CreateImage(HttpContext context)
        {
            if (File.Exists(documentFullname))
            {

                byte[] buffer;

                using (FileStream fileStream = new FileStream(documentFullname, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (BinaryReader reader = new BinaryReader(fileStream))
                {
                    buffer = reader.ReadBytes((int)reader.BaseStream.Length);
                }

                context.Response.ContentType = "application/pdf";
                context.Response.AddHeader("Content-Length", buffer.Length.ToString());
                context.Response.BinaryWrite(buffer);
                context.Response.End();

            }
            else
            {
                context.Response.Write("Unable to find the document you requested.");
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}