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
        public List<ItemVM<string>> ToggleActions{ get; set; } = new List<ItemVM<string>>();

        public List<ItemVM<string>> ListenKeys{ get; set; } = new List<ItemVM<string>>();

        public ActionsVM Load()
        {
            MovementAcions = SuperDataBase.Actions.MovementAcions.Select(x => x.ToItemVM(ignoreNull:true)).ToList();
            FaceActions = SuperDataBase.Actions.FaceActions.Select(x => x.ToItemVM(ignoreNull: true)).ToList();
            SpeakActions = SuperDataBase.Actions.SpeakActions.Select(x => x.ToItemVM(ignoreNull: true)).ToList();
            SavedActions = SuperDataBase.Actions.SavedActions.Select(x => x.ToItemVM(ignoreNull: true)).ToList();
            SequenceActions = SuperDataBase.Actions.SequenceActions.Select(x => x.ToItemVM(ignoreNull: true)).ToList();
            ToggleActions= SuperDataBase.Actions.ToggleActions.Select(x => x.ToItemVM(ignoreNull: true)).ToList();

            ListenKeys = SuperDataBase.Actions.ListenKeys.Select(x => x.ToItemVM(ignoreNull: true)).ToList();
            return this;
        }
    }
}
