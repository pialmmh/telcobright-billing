using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using MediationModel;
using MySql.Data.MySqlClient;
using PortalApp._portalHelper;
using TelcobrightMediation;

namespace WebApiController.Controllers
{
    [RoutePrefix("api")]
    public class DropDownController : ApiController
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["partner"].ConnectionString;
        private readonly TelcobrightConfig _tbc = PageUtil.GetTelcobrightConfig();
        private readonly PartnerEntities _context = new PartnerEntities();

        [HttpGet]
        [Route("getServiceGroups")]
        public IHttpActionResult GetServiceGroups()
        {
            try
            {
                Dictionary<int,string> serviceGroups= ServiceGroupPopulatorForDropDown.Populate(_tbc);
                return Ok(serviceGroups);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        [HttpGet]
        [Route("getAllPartners")]
        public IHttpActionResult GetAllPartners()
        {
            try
            {
                List<partner> partners = _context.partners.ToList();
                return Ok(partners);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
        [HttpGet]
        [Route("getAllPartnerNames")]
        public IHttpActionResult GetAllPartnerNames()
        {
            try
            {
                Dictionary<int,string> partners = _context.partners.ToDictionary(p => p.idPartner, p => p.PartnerName);
                partners.Add(-1,"Select");
                return Ok(partners.OrderBy(r => r.Key));
                //return Ok(partners);
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }
    }
}
