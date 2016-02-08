using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Server.Kestrel;
using Microsoft.Extensions.OptionsModel;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json;
using TinBot.Portable;
using TinBot.Web.ViewModels;

namespace TinBot.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IOptions<AppConfig> _settings;
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IOptions<AppConfig> settings, IHostingEnvironment hostingEnvironment)
        {
            _settings = settings;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpGet]
        public IActionResult Actions()
        {
            return View(new ActionsVM().Load());
        }

        [HttpPost]
        public ViewResult Actions(ActionsVM vm)
        {
            SuperDataBase.Actions.FaceActions =
                vm.FaceActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<FaceAction>(x.Item))
                    .ToList();
            SuperDataBase.Actions.MovementAcions =
                vm.MovementAcions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<MovementAcion>(x.Item))
                    .ToList();
            SuperDataBase.Actions.SpeakActions =
                vm.SpeakActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<SpeakAction>(x.Item))
                    .ToList();
            SuperDataBase.Actions.SavedActions =
                vm.SavedActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<SavedAction>(x.Item))
                    .ToList();
            SuperDataBase.Actions.SequenceActions =
                vm.SequenceActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<SequenceAction>(x.Item))
                    .ToList();

            SuperDataBase.SaveActions();
            return View(new ActionsVM().Load());
        }
    }
}
