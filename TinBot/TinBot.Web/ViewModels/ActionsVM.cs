using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TinBot.Portable;

namespace TinBot.Web.ViewModels
{
    public class ItemVM<T>
    {
        public T Item { get; set; }

        public string Description { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class ActionsVM
    {
        public List<ItemVM<string>> MovementAcions { get; set; } = new List<ItemVM<string>>();
        public List<ItemVM<string>> FaceActions { get; set; } = new List<ItemVM<string>>();
        public List<ItemVM<string>> SpeakActions { get; set; } = new List<ItemVM<string>>();
        public List<ItemVM<string>> SavedActions { get; set; } = new List<ItemVM<string>>();
        public List<ItemVM<string>> SequenceActions { get; set; } = new List<ItemVM<string>>();

        public ActionsVM Load()
        {
            MovementAcions = SuperDataBase.Actions.MovementAcions.Select(x => x.FromTinBotAction()).ToList();
            FaceActions = SuperDataBase.Actions.FaceActions.Select(x => x.FromTinBotAction()).ToList();
            SpeakActions = SuperDataBase.Actions.SpeakActions.Select(x => x.FromTinBotAction()).ToList();
            SavedActions = SuperDataBase.Actions.SavedActions.Select(x => x.FromTinBotAction()).ToList();
            SequenceActions = SuperDataBase.Actions.SequenceActions.Select(x => x.FromTinBotAction()).ToList();

            return this;
        }
    }
}
