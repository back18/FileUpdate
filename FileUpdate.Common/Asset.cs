using Newtonsoft.Json;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUpdate.Common
{
    public readonly struct Asset
    {
        public const char DIRECTORY_SEPARATOR_CHAR = '/';

        public Asset(string path, string hash, int size)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentException.ThrowIfNullOrEmpty(hash, nameof(hash));
            ThrowHelper.ArgumentOutOfMin(0, size, nameof(size));

            Path = path;
            Hash = hash;
            Size = size;
        }

        public Asset(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            Path = model.Path;
            Hash = model.Hash;
            Size = model.Size;
        }

        public string Path { get; }

        public string Hash { get; }

        public int Size { get; }

        public string GetEnvironmentPath()
        {
            char directorySeparatorChar = System.IO.Path.DirectorySeparatorChar;
            if (DIRECTORY_SEPARATOR_CHAR == directorySeparatorChar)
                return Path;

            return Path.Replace(DIRECTORY_SEPARATOR_CHAR, directorySeparatorChar);
        }

        public string GetEnvironmentPath(string directory)
        {
            ArgumentException.ThrowIfNullOrEmpty(directory, nameof(directory));

            string path = GetEnvironmentPath();
            return System.IO.Path.Combine(directory, path);
        }

        public static string FromEnvironmentPath(string environmentPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(environmentPath, nameof(environmentPath));

            char directorySeparatorChar = System.IO.Path.DirectorySeparatorChar;
            if (directorySeparatorChar == DIRECTORY_SEPARATOR_CHAR)
                return environmentPath;

            return environmentPath.Replace(directorySeparatorChar, DIRECTORY_SEPARATOR_CHAR);
        }

        public static string FromEnvironmentPath(string directory, string environmentPath)
        {
            ArgumentException.ThrowIfNullOrEmpty(directory, nameof(directory));
            ArgumentException.ThrowIfNullOrEmpty(environmentPath, nameof(environmentPath));
            if (!environmentPath.StartsWith(directory))
                throw new ArgumentException($"路径“{environmentPath}”不属于文件夹“{directory}”", nameof(environmentPath));

            char directorySeparatorChar = System.IO.Path.DirectorySeparatorChar;
            if (!directory.EndsWith(directorySeparatorChar))
                directory += directorySeparatorChar;

            return FromEnvironmentPath(environmentPath[directory.Length..]);
        }

        public override string ToString()
        {
            return Path;
        }

        public static Asset FromJson(string json)
        {
            ArgumentException.ThrowIfNullOrEmpty(json, nameof(json));

            Model model = JsonConvert.DeserializeObject<Model>(json) ?? throw new FormatException();
            return new(model);
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(ToModel());
        }

        public Model ToModel()
        {
            return new()
            {
                Path = Path,
                Hash = Hash,
                Size = Size
            };
        }

        public class Model
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            public string Path { get; set; }

            public string Hash { get; set; }

            public int Size { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
