using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TOH.Common.Data
{
    public class ServerConfig
    {
        public string TcpServerHost { get; set; }
        public int TcpServerPort { get; set; }

        public string ServiceProtocol { get; set; }
        public string ServiceHost { get; set; }
        public int ServicePort { get; set; }
    }

    public class UnitConfig
    {
        public int Id { get; set; }
        public UnitType Type { get; set; }
        public UnitGrade Grade { get; set; }
        public UnitElement Element { get; set; }
        public string Name { get; set; }
        public Dictionary<UnitStatType, int> Stats { get; set; }
        public Dictionary<UnitSkillSlot, int> Skills { get; set; }
    }

    public class SkillConfig
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SkillTarget Target { get; set; }
        public int Cooldown { get; set; }
        public List<object> Actions { get; set; }
    }
    public enum ConfigManagerState
    {
        None,
        Initializing,
        Initialized
    }

    public sealed class ConfigManager
    {
        private static readonly Lazy<ConfigManager> lazy = new Lazy<ConfigManager>(() => new ConfigManager(), true);

        public static ConfigManager Instance { get { return lazy.Value; } }

        public string ConfigDataPath { get; private set; } = "Assets/Config";

        public bool Initialized { get { return State == ConfigManagerState.Initialized; } }

        public ServerConfig ServerConfig { get; private set; }

        public ReadOnlyCollection<SkillModel> Skills { get; private set; } = new ReadOnlyCollection<SkillModel>(new List<SkillModel>());
        public ReadOnlyCollection<UnitModel> Units { get; private set; } = new ReadOnlyCollection<UnitModel>(new List<UnitModel>());
        public Dictionary<int, Dictionary<UnitStatType, double>> LevelConfig { get; private set; } = new Dictionary<int, Dictionary<UnitStatType, double>>();

        public ConfigManagerState State { get; private set; }

        private ConfigManager()
        {
            State = ConfigManagerState.None;
        }

        public async Task Initialize(string datatPath = null)
        {
            if (!string.IsNullOrEmpty(datatPath))
                ConfigDataPath = datatPath;

            State = ConfigManagerState.Initializing;

            await LoadServerConfig();
            await LoadUnitsData();
            await LoadUnitLevelData();

            State = ConfigManagerState.Initialized;
        }

        private static T LoadConfigConfig<T>(string filename)
        {
            var filePath = Path.Combine(Instance.ConfigDataPath, filename);
            var contents = File.ReadAllText(filePath);

            return JsonConvert.DeserializeObject<T>(contents);
        }

        private Task LoadServerConfig()
        {
            ServerConfig = LoadConfigConfig<ServerConfig>("ServerConfig.json");

            return Task.CompletedTask;
        }

        private Task LoadSkillsData()
        {
            var skills = new List<SkillModel>();

            var skillConfigs = LoadConfigConfig<List<SkillConfig>>("Skills.json");

            foreach (var skillConfig in skillConfigs)
            {
                var skillActions = new List<SkillAction>();

                foreach (var skillActionConfig in skillConfig.Actions)
                {
                    var skillActionObject = skillActionConfig as JObject;

                    var baseSkillAction = skillActionObject.ToObject<SkillAction>();

                    if (baseSkillAction != null)
                    {
                        switch (baseSkillAction.Type)
                        {
                            case SkillActionType.Damage:
                                var damageSkillAction = skillActionObject.ToObject<DamageSkillAction>();
                                if (damageSkillAction != null)
                                {
                                    skillActions.Add(damageSkillAction);
                                }
                                break;

                            case SkillActionType.Heal:
                                var healSkillAction = skillActionObject.ToObject<HealSkillAction>();
                                if (healSkillAction != null)
                                {
                                    skillActions.Add(healSkillAction);
                                }
                                break;
                        }
                    }
                }

                var skill = SkillModel.Create(skillConfig, skillActions);

                skills.Add(skill);
            }

            Skills = new ReadOnlyCollection<SkillModel>(skills);

            return Task.CompletedTask;
        }

        private Task LoadUnitsData()
        {
            // Units config depend on skill data so ensure it is loaded
            LoadSkillsData();

            var units = new List<UnitModel>();

            var unitConfigs = LoadConfigConfig<List<UnitConfig>>("Units.json");

            foreach (var unitConfig in unitConfigs)
            {
                var unitSkills = new Dictionary<UnitSkillSlot, SkillModel>();

                foreach (var skillData in unitConfig.Skills)
                {
                    var skill = Skills.FirstOrDefault(s => s.Id == skillData.Value);

                    if (skill != null)
                    {
                        unitSkills.Add(skillData.Key, skill);
                    }
                }

                var unit = new UnitModel
                {
                    UnitId = unitConfig.Id,
                    Type = unitConfig.Type,
                    Grade = unitConfig.Grade,
                    Element = unitConfig.Element,
                    Name = unitConfig.Name,
                    Stats = unitConfig.Stats,
                    Skills = unitSkills
                };

                units.Add(unit);
            }

            Units = new ReadOnlyCollection<UnitModel>(units);

            return Task.CompletedTask;
        }

        private Task LoadUnitLevelData()
        {
            LevelConfig = LoadConfigConfig<Dictionary<int, Dictionary<UnitStatType, double>>>("UnitLevel.json");

            return Task.CompletedTask;
        }

        public UnitModel GetUnit(int id)
        {
            return Units.FirstOrDefault(u => u.UnitId == id);
        }
    }
}
