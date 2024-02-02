using Downloader;
using FileUpdate.Common;
using log4net.Core;
using Newtonsoft.Json;
using QuanLib.Core.Extensions;
using QuanLib.Downloader;
using QuanLib.IO;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileUpdate.Client
{
    public class FileUpdateClient
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public FileUpdateClient(string rootUrl, string assetsDirectory)
        {
            ArgumentException.ThrowIfNullOrEmpty(rootUrl, nameof(rootUrl));
            ArgumentException.ThrowIfNullOrEmpty(assetsDirectory, nameof(assetsDirectory));

            _rootUrl = rootUrl;
            _assetsDirectory = assetsDirectory;
        }

        private readonly string _rootUrl;

        private readonly string _assetsDirectory;

        public async Task<Asset[]> GetAssetListAsync()
        {
            string url = Url.Combine(_rootUrl, HttpHelper.ASSET_LIST_URL);

            Stream stream = await DownloadAsync(url);
            string json = stream.ToUtf8Text();
            var models = JsonConvert.DeserializeObject<Asset.Model[]>(json) ?? throw new FormatException();
            Asset[] assets = new Asset[models.Length];
            for (int i = 0; i < models.Length; i++)
                assets[i] = new(models[i]);

            return assets;
        }

        public async Task DownloadAssetAsync(Asset asset)
        {
            string url = Url.Combine(_rootUrl, HttpHelper.ASSETS_URL, asset.Path);
            string path = asset.GetEnvironmentPath(_assetsDirectory);
            await DownloadAsync(url, path);
            LOGGER.Info("已下载: " + url);
        }

        private static async Task<Stream> DownloadAsync(string url, string? path = null)
        {
            ArgumentNullException.ThrowIfNull(url, nameof(url));

            DownloadTask downloadTask = new(url, path);
            Stream? stream = await downloadTask.StartAsync();
            if (stream is null || downloadTask.Download.Status == DownloadStatus.Failed)
                throw new InvalidOperationException("下载失败！");

            return stream;
        }
    }
}
