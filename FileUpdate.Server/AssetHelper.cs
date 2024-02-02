using FileUpdate.Common;
using log4net.Core;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUpdate.Server
{
    public static class AssetHelper
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public static Asset[] Build(string directory)
        {
            ArgumentException.ThrowIfNullOrEmpty(directory, nameof(directory));

            directory = Path.GetFullPath(directory);
            if (!directory.EndsWith(Path.DirectorySeparatorChar))
                directory += Path.DirectorySeparatorChar;

            LOGGER.Info($"将从 {directory} 目录加载所有文件");

            List<Asset> result = [];
            string[] files = DirectoryUtil.GetAllFiles(directory);
            foreach (string file in files)
            {
                try
                {
                    using FileStream fileStream = File.OpenRead(file);
                    string path = Asset.FromEnvironmentPath(directory, file);
                    string hash = HashUtil.GetHashString(fileStream, HashType.SHA1);
                    int size = (int)fileStream.Length;
                    result.Add(new(path, hash, size));
                    LOGGER.Info("已加载文件: " + path);
                }
                catch (Exception ex)
                {
                    string path = Asset.FromEnvironmentPath(directory, file);
                    LOGGER.Warn($"文件 {path} 无法加载，已跳过", ex);
                }
            }

            LOGGER.Info($"成功加载 {result.Count} 个文件");
            return result.ToArray();
        }
    }
}
