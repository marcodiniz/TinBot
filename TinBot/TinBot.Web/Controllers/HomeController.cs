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
using Newtonsoft.Json.Linq;
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

        public ViewResult Index()
        {
            return View();
        }


        [HttpGet]
        public ViewResult Actions()
        {
            var vm = new ActionsVM().Load();
            for (int i = 0; i < 5; i++)
            {
                vm.MovementAcions.Add(new MovementAcion().ToItemVM(true));
                vm.FaceActions.Add(new FaceAction().ToItemVM(true));
                vm.SavedActions.Add(new SavedAction().ToItemVM(true));
                vm.SequenceActions.Add(new SequenceAction().ToItemVM(true));
                vm.SpeakActions.Add(new SpeakAction().ToItemVM(true));
                vm.ToggleActions.Add(new ToggleAction().ToItemVM(true));

                vm.ListenKeys.Add(new ListenKey().ToItemVM(true));
            }
            return View(vm);
        }

        [HttpPost]
        public ActionResult Actions(ActionsVM vm)
        {
            SuperDataBase.Actions.FaceActions =
                vm.FaceActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<FaceAction>(x.Item))
                    .OrderBy(x=>x.Name)
                    .ToList();
            SuperDataBase.Actions.MovementAcions =
                vm.MovementAcions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<MovementAcion>(x.Item))
                    .OrderBy(x=>x.Name)
                    .ToList();
            SuperDataBase.Actions.SpeakActions =
                vm.SpeakActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<SpeakAction>(x.Item))
                    .OrderBy(x=>x.Name)
                    .ToList();
            SuperDataBase.Actions.SavedActions =
                vm.SavedActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<SavedAction>(x.Item))
                    .OrderBy(x=>x.Name)
                    .ToList();
            SuperDataBase.Actions.SequenceActions =
                vm.SequenceActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<SequenceAction>(x.Item))
                    .OrderBy(x=>x.Name)
                    .ToList();
            SuperDataBase.Actions.ToggleActions =
                vm.ToggleActions.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<ToggleAction>(x.Item))
                    .OrderBy(x=>x.Name)
                    .ToList();

            SuperDataBase.Actions.ListenKeys=
                vm.ListenKeys.Where(x => !x.IsDeleted)
                    .Select(x => JsonConvert.DeserializeObject<ListenKey>(x.Item))
                    .ToList();

            SuperDataBase.SaveActions();
            return RedirectToAction("Actions");
        }

        [HttpGet]
        public ViewResult Configs()
        {
            SuperDataBase.Configs.UserNameToDisplayName.Add(new NamesMapping());
            SuperDataBase.Configs.UserNamesToUserName.Add(new NamesMapping());
            return View(SuperDataBase.Configs);
        }

        [HttpPost]
        public RedirectToActionResult Configs(Configs configs)
        {
            configs.UserNamesToUserName.RemoveAll(x => string.IsNullOrEmpty(x.From) || string.IsNullOrEmpty(x.To));
            configs.UserNameToDisplayName.RemoveAll(x => string.IsNullOrEmpty(x.From) || string.IsNullOrEmpty(x.To));

            SuperDataBase.Configs = configs;

            var actionIPT =
                SuperDataBase.Actions.SpeakActions.Where(
                    x => x.Name.StartsWith(configs.IPT >= configs.IPTGoodLimit ? "TGoodMark" : "TBadMark")).GetRandom().DeepCopy();
            var actionIET =
                SuperDataBase.Actions.SpeakActions.Where(
                    x => x.Name.StartsWith(configs.IET <= configs.IETGoodLimit? "TGoodMark" : "TBadMark")).GetRandom().DeepCopy();

            actionIET.Text = actionIET.Text.Replace("{current}", configs.IET.ToString()).Replace("{mark}", "erros");
            actionIPT.Text = actionIPT.Text.Replace("{current}", configs.IPT.ToString()).Replace("{mark}", "produtividade");


            SuperDataBase.Actions.SequenceActions.RemoveAll(x => x.Name.Equals("SMarks"));
            var actionMarks = new SequenceAction("SMarks");
            actionMarks.AddPararellActions(actionIET);
            actionMarks.AddPararellActions(new SavedAction("Rest"));
            actionMarks.AddPararellActions(actionIPT);
            SuperDataBase.Actions.SequenceActions.Add(actionMarks);

            SuperDataBase.Actions.SequenceActions.RemoveAll(x => x.Name.Equals("SRD"));
            var actionRD = new SequenceAction("SRD");
            actionRD.AddPararellActions(SuperDataBase.Actions.SpeakActions.Where(x=>x.Name.StartsWith("TMorningTeam")).GetRandom());
            actionRD.AddPararellActions(new SavedAction("Rest"));
            actionRD.AddPararellActions(actionIET);
            actionRD.AddPararellActions(new SavedAction("Rest"));
            actionRD.AddPararellActions(actionIPT);
            actionRD.AddPararellActions(new SavedAction("Rest"));
            actionRD.AddPararellActions(SuperDataBase.Actions.SpeakActions.Where(x => x.Name.StartsWith("TTip")).GetRandom());
            SuperDataBase.Actions.SequenceActions.Add(actionRD);

            SuperDataBase.SaveConfigs();
            SuperDataBase.SaveActions();

            return RedirectToAction("Configs");
        }

        [HttpGet]
        [HttpPost]
        public JsonResult Enqueue(string json)
        {
            var jObject = (JObject)JsonConvert.DeserializeObject<JObject>(json);
            var typeInt = jObject["Type"].ToObject<int>();
            var type = (EActionType)typeInt;
            TinBotAction action = null;
            switch (type)
            {
                case EActionType.Speak:
                    action = jObject.ToObject<SpeakAction>();
                    break;
                case EActionType.Face:
                    action = jObject.ToObject<FaceAction>();
                    break;
                case EActionType.Move:
                    action = jObject.ToObject<MovementAcion>();
                    break;
                case EActionType.Saved:
                    action = jObject.ToObject<SavedAction>();
                    break;
                case EActionType.Sequence:
                    action = jObject.ToObject<SequenceAction>();
                    break;
                case EActionType.Toggle:
                    action = jObject.ToObject<ToggleAction>();
                    break;
            }
            SuperDataBase.Queue.Add(new ActionContainer(action));

            return Json("OK");
        }
    }
}
