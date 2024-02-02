using Newtonsoft.Json;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUpdate.Server
{
    public class Config : IDataModelOwner<Config, Config.DataModel>, ISingleton<Config, InstantiateArgs>
    {
        public const string CONFIGFILE_PATH = "config.json";

        private Config() : this(DataModel.CreateDefault()) { }

        private Config(DataModel model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            Directory = model.Directory;
            Address = model.Address;
            Port = (ushort)model.Port;
        }

        private static readonly object _slock = new();

        public static bool IsInstanceLoaded => _Instance is not null;

        public static Config Instance => _Instance ?? throw new InvalidOperationException("实例未加载");
        private static Config? _Instance;

        public string Directory { get; }

        public string Address { get; }

        public ushort Port { get; }

        public static Config LoadInstance(InstantiateArgs args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            lock (_slock)
            {
                if (_Instance is not null)
                    throw new InvalidOperationException("试图重复加载单例实例");

                if (File.Exists(CONFIGFILE_PATH))
                {
                    string json = File.ReadAllText(CONFIGFILE_PATH);
                    DataModel model = DataModel.CreateDefault();
                    JsonConvert.PopulateObject(json, model);
                    DataModel.Validate(model, CONFIGFILE_PATH);
                    Config config = FromDataModel(model);
                    _Instance = config;
                    return _Instance;
                }
                else
                {
                    Config config = new();
                    string json = config.ToJson();
                    File.WriteAllText(CONFIGFILE_PATH, json);
                    _Instance = config;
                    return _Instance;
                }
            }
        }

        private string ToJson()
        {
            DataModel model = ToDataModel();
            return JsonConvert.SerializeObject(model);
        }

        private static Config FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

            DataModel model = JsonConvert.DeserializeObject<DataModel>(json) ?? throw new FormatException();
            return FromDataModel(model);
        }

        public DataModel ToDataModel()
        {
            return new()
            {
                Directory = Directory,
                Address = Address,
                Port = Port
            };
        }

        public static Config FromDataModel(DataModel model)
        {
            return new(model);
        }

        public class DataModel : IDataModel<DataModel>
        {
            [Required(ErrorMessage = "配置项缺失")]
            public required string Directory { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public required string Address { get; set; }

            [Range(0, 65535, ErrorMessage = "值的范围应该为0~65535")]
            public required int Port { get; set; }

            public static DataModel CreateDefault()
            {
                return new()
                {
                    Directory = ".",
                    Address = "127.0.0.1",
                    Port = 80
                };
            }

            public static void Validate(DataModel model, string name)
            {
                ArgumentNullException.ThrowIfNull(model, nameof(model));
                ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

                List<ValidationResult> results = [];
                if (!Validator.TryValidateObject(model, new(model), results, true))
                {
                    StringBuilder message = new();
                    message.AppendLine();
                    int count = 0;
                    foreach (var result in results)
                    {
                        string memberName = result.MemberNames.FirstOrDefault() ?? string.Empty;
                        message.AppendLine($"[{memberName}]: {result.ErrorMessage}");
                        count++;
                    }

                    if (count > 0)
                    {
                        message.Insert(0, $"解析“{name}”时遇到{count}个错误：");
                        throw new ValidationException(message.ToString().TrimEnd());
                    }
                }
            }
        }
    }
}
