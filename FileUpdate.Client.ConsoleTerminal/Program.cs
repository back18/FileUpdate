using FileUpdate.Common;
using log4net.Core;
using QuanLib.Core;
using QuanLib.Logging;

namespace FileUpdate.Client.ConsoleTerminal
{
    public static class Program
    {
        private static LogImpl LOGGER => LogManager.Instance.GetLogger();

        static Program()
        {
            Thread.CurrentThread.Name = "Main Thread";
            LogHelper.Load();
        }

        private static void Main(string[] args)
        {
            try
            {
                Config.LoadInstance(InstantiateArgs.Empty);
                LOGGER.Info("初始化完成！");
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("初始化失败！", ex);
                Console.ReadLine();
                return;
            }

            try
            {
                FileUpdateClient fileUpdateClient = new(Config.Instance.Url, Config.Instance.Directory);
                Asset[] assets = fileUpdateClient.GetAssetListAsync().Result;
                LOGGER.Info($"成功从服务器获取到资源列表，共计 {assets.Length} 个文件：");

                Difference difference = AssetHelper.GetDifference(Config.Instance.Directory, assets);

                if (difference.Deleteds.Count > 0)
                {
                    LOGGER.Info("开始下载缺失的文件...");
                    foreach (Asset delete in difference.Deleteds)
                    {
                        fileUpdateClient.DownloadAssetAsync(delete).Wait();
                    }
                }

                if (difference.Modifieds.Count > 0)
                {
                    LOGGER.Info("开始下载差异的文件...");
                    foreach (Asset modified in difference.Modifieds)
                    {
                        fileUpdateClient.DownloadAssetAsync(modified).Wait();
                    }
                }
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("程序异常：", ex);
                Console.ReadLine();
                return;
            }

            LOGGER.Info("程序运行完成！");
            Console.ReadLine();
            return;
        }
    }
}
