using BasketApi;
using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using static BasketApi.Utility;
using System.Data.Entity;

namespace WebApplication1.Areas.Admin.Controllers
{

    [RoutePrefix("api/Common")]
    public class CommonController : ApiController
    {
        [HttpGet]
        [Route("GetAllCities")]
        public async Task<IHttpActionResult> GetAllCities(int? Page = 0, int? Items = 10)
        {
            try
            {
                using (SkriblContext ctx = new SkriblContext())
                {
                    return Ok(new CustomResponse<List<Cities>>
                    {
                        Message = Global.ResponseMessages.Success,
                        StatusCode = (int)HttpStatusCode.OK,
                        Result = ctx.Cities.Where(x => !x.IsDeleted).ToList()
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(Utility.LogError(ex));
            }
        }
    }
}
