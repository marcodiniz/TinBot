using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using TinBot.Portable;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TinBot.Web.Controllers
{
    [Route("api/[action]")]
    public class ActionsController : Controller
    {
        [HttpGet]
        public JsonResult Library()
        {
            return Json(SuperDataBase.Actions);
        }

        [HttpGet]
        public JsonResult Queue()
        {
            return Json(new List<ActionsContainer>{ SuperDataBase.Actions});
        }
    }
}
