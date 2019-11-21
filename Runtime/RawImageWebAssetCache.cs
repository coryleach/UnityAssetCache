using UnityEngine;
using UnityEngine.UI;

namespace Gameframe.AssetCache
{
    /// <summary>
    /// Uses an internal static WebTextureCache to load a texture from the web and cache it
    /// </summary>
    public class RawImageWebAssetCache : MonoBehaviour
    {
        public RawImage image;
        public string url;
        public bool cleanCacheOnDisable = true;

        private static readonly WebTextureCache Cache = new WebTextureCache();

        private WebTextureCache.ICachedAsset cachedAsset;

        private void OnEnable()
        {
            image.enabled = false;
            Load();
        }

        private void OnDisable()
        {
            Unload();
            if (cleanCacheOnDisable)
            {
                Clean();
            }
        }

        private async void Load()
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
            
            var task = Cache.GetAsync(url);
            await task;

            if (!enabled || !gameObject.activeInHierarchy)
            {
                task.Result.Dispose();
                return;
            }

            if (cachedAsset != null)
            {
                Unload();
            }

            cachedAsset = task.Result;

            image.texture = cachedAsset.Asset;
            image.enabled = true;
        }


        private void Unload()
        {
            if (cachedAsset == null)
            {
                return;
            }

            cachedAsset.Dispose();
            cachedAsset = null;
        }

        public static void Clean()
        {
            Cache.ClearUnusedAssets();
        }

        private void OnValidate()
        {
            if (image == null)
            {
                image = GetComponent<RawImage>();
            }
        }
    }
}