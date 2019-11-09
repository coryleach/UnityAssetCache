using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Gameframe.AssetCache
{
    internal class WebTextureLoader : IAssetLoader<Texture2D>
    {
        public async Task<Texture2D> LoadAsync(string assetPath)
        {
            using (var request = UnityWebRequestTexture.GetTexture(assetPath))
            {
                var asyncOp = request.SendWebRequest();

                while (!asyncOp.isDone)
                {
                    await Task.Yield();
                }

                if (request.isNetworkError)
                {
                    throw new Exception($"Network Error: {request.error}");
                }

                if (request.isHttpError)
                {
                    throw new HttpRequestException(request.error);
                }

                return DownloadHandlerTexture.GetContent(request);
            }
        }

        public void Unload(Texture2D asset)
        {
            Resources.UnloadAsset(asset);
        }
    }

    /// <summary>
    /// Cache for loading textures from Web URLs
    /// </summary>
    public class WebTextureCache : AssetCache<Texture2D>
    {
        public WebTextureCache() : base(new WebTextureLoader()) { }
    }
}