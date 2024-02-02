using FileUpdate.Common;
using log4net.Core;
using QuanLib.Core;
using QuanLib.Logging;

namespace FileUpdate.Server.ConsoleTerminal
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
            List<Asset> assets = [];
            try
            {
                Config.LoadInstance(InstantiateArgs.Empty);
                assets.AddRange(AssetHelper.Build(Config.Instance.Directory));
                LOGGER.Info("初始化完成！");
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("初始化失败！", ex);
                Console.ReadLine();
                return;
            }

            FileUpdateServer fileUpdateServer = new(Config.Instance.Address, Config.Instance.Port, Config.Instance.Directory, assets);
            fileUpdateServer.Start("HttpSetver Thread");
            fileUpdateServer.WaitForStop();
        }
    }
}
