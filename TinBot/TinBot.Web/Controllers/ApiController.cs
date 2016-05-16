using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Newtonsoft.Json;
using TinBot.Portable;
using TinBot.Web.ViewModels;

// For more information on enabling Web API for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace TinBot.Web.Controllers
{
    [Route("api/[action]")]
    public class ApiController : Controller
    {
        [HttpGet]
        public JsonResult Library()
        {
            return Json(SuperDataBase.Actions);
        }

        [HttpGet]
        [HttpDelete]
        public JsonResult Queue()
        {
            if (Request.Method == "GET")
            {
                SuperDataBase.LoadActions(); //temporary
                var queue = SuperDataBase.Queue.ToList();
                return Json(queue);
            }
            else if (Request.Method == "DELETE")
            {
                SuperDataBase.Queue.Clear();
                return Json("OK");
            }

            return null;
        }


        [HttpPost]
        public ActionResult Checkin([FromBody]CheckinVM checkin)
        {
            Console.WriteLine(checkin != null);
            Console.WriteLine(checkin.resource.author.uniqueName);

            var username = SuperDataBase.Configs.GetRealUserName(checkin.resource.author.uniqueName);
            var display = SuperDataBase.Configs.GetDisplayName(username);

            if (username == null || display == null)
                return Ok("no user");

            var actionPart1 =
                SuperDataBase.Actions.SpeakActions.Where(x => x.Name.StartsWith("TCheckinFirst")).GetRandom().DeepCopy();
            var actionPart2 =
                SuperDataBase.Actions.SpeakActions.Where(x => x.Name.StartsWith("TCheckinLast")).GetRandom().DeepCopy();
            var actionMove = SuperDataBase.Actions["B" + username];
            actionPart2.Text = actionPart2.Text.Replace("{userdisplay}", display)
                .Replace("{comment}", checkin.resource.comment);


            var seq = new SequenceAction();
            seq.AddPararellActions(actionPart1);
            seq.AddPararellActions(actionMove);
            seq.AddPararellActions(actionPart2);

            SuperDataBase.Queue.Add(new ActionContainer(seq));

            return Ok("queued");
        }

        [HttpPost]
        public ActionResult Build([FromBody]BuildVM build)
        {
            Console.WriteLine(build != null);

            var username = SuperDataBase.Configs.GetRealUserName(build.resource.lastChangedBy.uniqueName)
                           ?? SuperDataBase.Configs.GetRealUserName(build.resource.requestedFor.uniqueName)
                           ?? SuperDataBase.Configs.GetRealUserName(build.resource.requestedBy.uniqueName);
            var display = SuperDataBase.Configs.GetDisplayName(username);

            if (username == null || display == null)
                return Ok("no user");

            var branch = build.resource.sourceBranch.Split('/').Last();

            var part1name = build.resource.result.Equals("succeeded")
                ? "TBuildSuccessFirst"
                : "TBuildFailFirst";
            var part2name = build.resource.result.Equals("succeeded")
                ? "TBuildSuccessLast"
                : "TBuildFailLast";
            var actionPart1 =
                SuperDataBase.Actions.SpeakActions.Where(x => x.Name.StartsWith(part1name))
                    .GetRandom()
                    .DeepCopy();
            var actionPart2 =
                SuperDataBase.Actions.SpeakActions.Where(x => x.Name.StartsWith(part2name))
                    .GetRandom()
                    .DeepCopy();
            var actionMove = SuperDataBase.Actions["B" + username];

            actionPart1.Text = actionPart1.Text.Replace("{branch}", branch);
            actionPart2.Text = actionPart2.Text.Replace("{userdisplay}", display);

            var seq = new SequenceAction();
            seq.AddPararellActions(actionPart1);
            seq.AddPararellActions(actionMove);
            seq.AddPararellActions(actionPart2);

            SuperDataBase.Queue.Add(new ActionContainer(seq));

            return Ok("queued");
        }

        [HttpPost]
        public JsonResult Slack(string text, string user_name)
        {
            var display = SuperDataBase.Configs.GetDisplayName(user_name);
            if (string.IsNullOrWhiteSpace(display))
                return Json(new { text = $"Não encontrei esse tal de {user_name} no meu registro!" });

            var messageAction = (SuperDataBase.Actions["TMessage"] as SpeakAction).DeepCopy();
            if (messageAction == null)
                return Json(new { text = $"Não encontrei a Ação de mensagem em meus registros" });

            text = text.ToLower();
            if (text.Contains("marco")
                && new[] { "bixa", "gay", "viado", "rapazes", "meninos" }.Any(x => text.Contains(x)))
                text = text.Replace("marco", display);

            var message = messageAction.Text.Replace("{recado}", text.Substring("Tinbot".Length)).Replace("{sender}", display);
            messageAction.Text = message;

            SuperDataBase.Queue.Add(new ActionContainer(messageAction));

            return Json(new { text = "Recado à caminho!" });
        }
    }
}
