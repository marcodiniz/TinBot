using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.Extensions.OptionsModel;
using Newtonsoft.Json;
using TinBot.Portable;
using TinBot.Web.ViewModels;

namespace TinBot.Web
{
    public static class SuperDataBase
    {
        public static string ActionsFile { get; set; }
        public static ActionsContainer Actions { get; set; } = new ActionsContainer();
        public static List<ActionContainer> Queue { get; set; } = new List<ActionContainer>();
        public static string ConfigsFile { get; set; }
        public static Configs Configs { get; set; } = new Configs();

        public static void Configure(IHostingEnvironment env, IOptions<AppConfig> config)
        {
            ActionsFile = env.WebRootPath + config.Value.ActionsFile;
            ConfigsFile= env.WebRootPath + config.Value.ConfigsFile;
            LoadActions();
            LoadConfigs();
        }

        public static void LoadActions()
        {
            var file = new FileInfo(ActionsFile);
            var reader = file.OpenText();
            var actionString = reader.ReadToEnd();
            Actions = JsonConvert.DeserializeObject<ActionsContainer>(actionString);
            reader.Dispose();
        }

        public static void SaveActions()
        {
            var file = new FileInfo(ActionsFile);
   
            var writer = file.CreateText();
            var str = JsonConvert.SerializeObject(Actions, Formatting.Indented);
            foreach (var line in str.Split('\n'))
            {
                writer.Write(line);
                writer.Flush();
            }
            writer.Dispose();
        }

        public static void LoadConfigs()
        {
            var file = new FileInfo(ConfigsFile);
            var reader = file.OpenText();
            var str = reader.ReadToEnd();
            Configs = JsonConvert.DeserializeObject<Configs>(str);
            reader.Dispose();
        }

        public static void SaveConfigs()
        {
            var file = new FileInfo(ConfigsFile);

            var writer = file.CreateText();
            var str = JsonConvert.SerializeObject(Configs, Formatting.Indented);
            foreach (var line in str.Split('\n'))
            {
                writer.Write(line);
                writer.Flush();
            }
            writer.Dispose();
        }
    }
}
