using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cache
{
    /// <summary>
    /// Maintains a cache of assets
    /// </summary>
    /// <typeparam name="TAssetType">The kind of asset to be cached</typeparam>
    public class AssetCache<TAssetType> where TAssetType : UnityEngine.Object
    {
        public interface ICachedAsset : IDisposable
        {
            ICachedAsset Clone();
            TAssetType Asset { get; }
            int RefCount { get; }
        }

        protected class AssetCacheEntry : IDisposable
        {
            private string url;
            public string Url => url;

            private int refCount = 0;
            public int RefCount => refCount;

            public TAssetType Asset => task?.Result;

            private Task<TAssetType> task;
            public Task<TAssetType> Task => task;

            private Action<TAssetType> assetDestroyer;

            private bool disposed = false;

            public AssetCacheEntry(string url, Task<TAssetType> getAssetTask, Action<TAssetType> assetDestroyer)
            {
                this.url = url;
                this.assetDestroyer = assetDestroyer;
                task = getAssetTask;
            }

            public CachedAsset GetCachedAsset()
            {
                if (disposed)
                {
                    throw new InvalidOperationException("AssetCacheEntry cannot get asset after dispose.");
                }
                return new CachedAsset(this);
            }

            public void Increment()
            {
                if (disposed)
                {
                    throw new InvalidOperationException("AssetCacheEntry cannot increment after dispose.");
                }
                refCount++;
            }

            public void Decrement()
            {
                if (disposed)
                {
                    throw new InvalidOperationException("AssetCacheEntry cannot decrement after dispose.");
                }
                refCount--;
            }

            public void Dispose()
            {
                if (disposed)
                {
                    throw new InvalidOperationException("AssetCacheEntry already disposed");
                }
                disposed = true;
                if (!task.IsCompleted)
                {
                    task.ContinueWith((assetTask) => { assetDestroyer(assetTask.Result); }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                else
                {
                    assetDestroyer(task.Result);
                }
                task = null;
            }
        }

        protected class CachedAsset : ICachedAsset
        {
            private readonly AssetCacheEntry cacheEntry;
            private bool disposed;

            public CachedAsset(AssetCacheEntry entry)
            {
                disposed = false;
                cacheEntry = entry;
                cacheEntry.Increment();
            }

            public ICachedAsset Clone()
            {
                return new CachedAsset(cacheEntry);
            }

            public TAssetType Asset
            {
                get
                {
                    if (disposed)
                    {
                        throw new InvalidOperationException("CachedAsset has been disposed");
                    }
                    return cacheEntry.Asset;
                }
            }

            public int RefCount => cacheEntry.RefCount;

            public void Dispose()
            {
                if (disposed)
                {
                    throw new InvalidOperationException("CachedAsset has already been disposed");
                }
                disposed = true;
                cacheEntry.Decrement();
            }
        }

        protected Dictionary<string, AssetCacheEntry> dictionary = new Dictionary<string, AssetCacheEntry>();

        private Func<string, Task<TAssetType>> assetLocator = null;

        public Func<string, Task<TAssetType>> AssetLocator
        {
            get => assetLocator;
            set => assetLocator = value;
        }

        private Action<TAssetType> assetDestroyer = null;
        public Action<TAssetType> AssetDestroyer
        {
            get => assetDestroyer;
            set => assetDestroyer = value;
        }

        private async Task<TAssetType> LocateAssetAsync(string url)
        {
            var asset = await AssetLocator(url);
            return asset;
        }

        public async Task<ICachedAsset> GetAsync(string url)
        {
            //Get or create cache entry
            if (!dictionary.TryGetValue(url, out var entry))
            {
                entry = new AssetCacheEntry(url, LocateAssetAsync(url), assetDestroyer);
                dictionary.Add(url, entry);
            }

            //Wait for the task to finish
            if (!entry.Task.IsCompleted)
            {
                await entry.Task;
            }

            return entry.GetCachedAsset();
        }

        public void ClearUnusedAssets()
        {
            //Remove Unused entries
            var unusedEntries = dictionary.Where((pair) => (pair.Value.RefCount <= 0)).ToList();
            foreach (var pair in unusedEntries)
            {
                dictionary.Remove(pair.Key);
                pair.Value.Dispose();
            }
        }
    }
}
