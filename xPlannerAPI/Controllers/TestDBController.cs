using System.Linq;
using System.Web.Http;
using xPlannerCommon.Models;

namespace xPlannerAPI.Controllers
{
    public class TestDBController : ApiController
    {
        [Route("api/testdb")]
        public int GetAll()
        {
            audaxwareEntities xx = new audaxwareEntities();
            var total = xx.clients.Count();
            return total;
        }
    }
}
