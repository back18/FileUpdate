using FileUpdate.Common;
using log4net.Core;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FileUpdate.Server
{
    public class FileUpdateServer : UnmanagedRunnable
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public FileUpdateServer(string address, ushort port, string assetsDirectory, IEnumerable<Asset> assets)
        {
            ArgumentException.ThrowIfNullOrEmpty(address, nameof(address));
            ArgumentNullException.ThrowIfNull(assets, nameof(assets));
            ArgumentException.ThrowIfNullOrEmpty(assetsDirectory, nameof(assetsDirectory));

            _address = address;
            _port = port;
            _assetsDirectory = assetsDirectory;
            _assets = new(assets.ToDictionary(item => item.Path, item => item));

            string assetListJson = JsonConvert.SerializeObject(assets);
            _assetListJsonBytes = Encoding.UTF8.GetBytes(assetListJson);

            _listener = new();
            _listener.Prefixes.Add(URL);
        }

        private readonly string _address;

        private readonly ushort _port;

        private readonly string _assetsDirectory;

        private readonly ReadOnlyDictionary<string, Asset> _assets;

        private readonly byte[] _assetListJsonBytes;

        private readonly HttpListener _listener;

        public string URL => $"http://{_address}:{_port}/";

        protected override void DisposeUnmanaged()
        {
            _listener.Close();
        }

        protected override void Run()
        {
            _listener.Start();

            LOGGER.InfoFormat("http服务器已从 {0} 启动！", URL);

            while (IsRunning)
            {
                HttpListenerContext context = _listener.GetContext();
                Task.Run(() =>
                {
                    HandleRequest(context);
                    context.Response.Close();
                });
            }
        }

        private void HandleRequest(HttpListenerContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            string url = context.Request.RawUrl ?? string.Empty;

            if (url == HttpHelper.ASSET_LIST_URL)
            {
                ResponseUploadAssetListJson(context);
            }
            else if (url.StartsWith(HttpHelper.ASSETS_URL))
            {
                string assetPath = url[HttpHelper.ASSETS_URL.Length..];
                if (!_assets.TryGetValue(assetPath, out var asset))
                    goto NotFound;

                ResponseUploadAsset(context, asset);
            }
            else
            {
                goto NotFound;
            }

            return;
            NotFound:
            ResponseNotFound(context.Response);
            LOGGER.Warn($"找不到 {context.Request.RemoteEndPoint} 请求的URI {context.Request.Url}");
        }

        private void ResponseUploadAssetListJson(HttpListenerContext context)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            using MemoryStream memoryStream = new(_assetListJsonBytes);
            ResponseUpload(context.Response, memoryStream, "assetlist.json");
            LOGGER.Info($"上传资源列表到 {context.Request.RemoteEndPoint}");
        }

        private void ResponseUploadAsset(HttpListenerContext context, Asset asset)
        {
            ArgumentNullException.ThrowIfNull(context, nameof(context));

            try
            {
                Stopwatch stopwatch = Stopwatch.StartNew();
                string filePath = asset.GetEnvironmentPath(_assetsDirectory);
                using FileStream fileStream = File.OpenRead(filePath);
                ResponseUpload(context.Response, fileStream, Path.GetFileName(filePath));
                LOGGER.Info($"上传文件 {asset.Path} 到 {context.Request.RemoteEndPoint} 耗时 {stopwatch.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                ResponseInternalServerError(context.Response);
                LOGGER.Error($"{context.Request.RemoteEndPoint} 请求 {context.Request.Url} 时引发了异常", ex);
            }
        }

        private static void ResponseUpload(HttpListenerResponse response, Stream stream, string name)
        {
            ArgumentNullException.ThrowIfNull(response, nameof(response));
            ArgumentNullException.ThrowIfNull(stream, nameof(stream));
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "application/octet-stream";
            response.ContentLength64 = stream.Length;
            response.Headers.Add("Content-Disposition", $"attachment; filename={name}");

            if (stream.CanSeek && stream.Position != 0)
                stream.Seek(0, SeekOrigin.Begin);

            byte[] buffer = new byte[4096];
            while (true)
            {
                int length = stream.Read(buffer, 0, buffer.Length);
                if (length <= 0)
                    break;
                response.OutputStream.Write(buffer, 0, length);
            }
        }

        private static void ResponseNotFound(HttpListenerResponse response)
        {
            ArgumentNullException.ThrowIfNull(response, nameof(response));

            response.StatusCode = (int)HttpStatusCode.NotFound;
        }

        private static void ResponseInternalServerError(HttpListenerResponse response)
        {
            ArgumentNullException.ThrowIfNull(response, nameof(response));

            response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
    }
}
