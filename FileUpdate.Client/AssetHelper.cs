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

namespace FileUpdate.Client
{
    public static class AssetHelper
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public static Difference GetDifference(string directory, Asset[] assets)
        {
            ArgumentException.ThrowIfNullOrEmpty(directory, nameof(directory));
            ArgumentNullException.ThrowIfNull(assets, nameof(assets));

            directory = Path.GetFullPath(directory);
            if (!directory.EndsWith(Path.DirectorySeparatorChar))
                directory += Path.DirectorySeparatorChar;

            LOGGER.Info($"正在检查 {directory} 目录下的文件完整性");

            List<Asset> deleteds = [];
            List<Asset> modifieds = [];
            foreach (Asset asset in assets)
            {
                string filePath = asset.GetEnvironmentPath(directory);
                if (!File.Exists(filePath))
                {
                    deleteds.Add(asset);
                    LOGGER.Info("找到缺失文件: " + asset.Path);
                    continue;
                }

                try
                {
                    using FileStream fileStream = File.OpenRead(filePath);
                    string hash = HashUtil.GetHashString(fileStream, HashType.SHA1);
                    int size = (int)fileStream.Length;

                    if (hash != asset.Hash || size != asset.Size)
                    {
                        modifieds.Add(asset);
                        LOGGER.Info("找到差异文件: " + asset.Path);
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    LOGGER.Warn($"文件 {asset.Path} 无法加载，已跳过", ex);
                }
            }

            LOGGER.Info($"共计找到 {deleteds.Count} 个缺失文件和 {modifieds.Count} 个差异文件");
            return new(deleteds, modifieds);
        }
    }
}
