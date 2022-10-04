using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gameframe.AssetCache
{
    /// <summary>
    /// Maintains a cache of assets
    /// </summary>
    /// <typeparam name="TAssetType">The kind of asset to be cached</typeparam>
    public class AssetCache<TAssetType> : KeyedAssetCache<string,TAssetType> where TAssetType : class
    {
        public AssetCache(IKeyedAssetLoader<string, TAssetType> loader) : base(loader) { }
    }

    /// <summary>
    /// Maintains a cache of assets with a specified key type and asset type
    /// </summary>
    /// <typeparam name="TAssetType">The kind of asset to be cached</typeparam>
    /// <typeparam name="TKeyType">The key used to reference the asset to be loaded/unloaded</typeparam>
    public class KeyedAssetCache<TKeyType,TAssetType> where TAssetType : class
    {
        /// <summary>
        /// Reference to a cached asset.
        /// Dispose() should be called to release the asset reference.
        /// </summary>
        public interface ICachedAsset : IDisposable
        {
            ICachedAsset Clone();
            TAssetType Asset { get; }
            int RefCount { get; }
        }

        protected class AssetCacheEntry : IDisposable
        {
            private readonly TKeyType _assetKey;
            public TKeyType AssetKey => _assetKey;

            private int _refCount = 0;
            public int RefCount => _refCount;

            public TAssetType Asset => _task?.Result;

            private Task<TAssetType> _task;
            public Task<TAssetType> Task => _task;

            private readonly Action<TAssetType> _assetDestroyer;

            private bool _disposed = false;

            public AssetCacheEntry(TKeyType assetKey, Task<TAssetType> getAssetTask, Action<TAssetType> assetDestroyer)
            {
                _assetKey = assetKey;
                _assetDestroyer = assetDestroyer;
                _task = getAssetTask;
            }

            public CachedAsset GetCachedAsset()
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("AssetCacheEntry cannot get asset after dispose.");
                }
                return new CachedAsset(this);
            }

            public void Increment()
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("AssetCacheEntry cannot increment after dispose.");
                }
                _refCount++;
            }

            public void Decrement()
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("AssetCacheEntry cannot decrement after dispose.");
                }
                _refCount--;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("AssetCacheEntry already disposed");
                }
                _disposed = true;
                if (!_task.IsCompleted)
                {
                    _task.ContinueWith((assetTask) => { _assetDestroyer(assetTask.Result); }, TaskContinuationOptions.OnlyOnRanToCompletion);
                }
                else
                {
                    _assetDestroyer(_task.Result);
                }
                _task = null;
            }
        }

        protected class CachedAsset : ICachedAsset
        {
            private readonly AssetCacheEntry _cacheEntry;
            private bool _disposed;

            public CachedAsset(AssetCacheEntry entry)
            {
                _disposed = false;
                _cacheEntry = entry;
                _cacheEntry.Increment();
            }

            public ICachedAsset Clone()
            {
                return new CachedAsset(_cacheEntry);
            }

            public TAssetType Asset
            {
                get
                {
                    if (_disposed)
                    {
                        throw new InvalidOperationException("CachedAsset has been disposed");
                    }
                    return _cacheEntry.Asset;
                }
            }

            public int RefCount => _cacheEntry.RefCount;

            public void Dispose()
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("CachedAsset has already been disposed");
                }
                _disposed = true;
                _cacheEntry.Decrement();
            }
        }

        protected readonly Dictionary<TKeyType, AssetCacheEntry> CacheDictionary = new Dictionary<TKeyType, AssetCacheEntry>();

        private readonly IKeyedAssetLoader<TKeyType,TAssetType> _assetLoader;

        private async Task<TAssetType> LocateAssetAsync(TKeyType assetKey)
        {
            var asset = await _assetLoader.LoadAsync(assetKey);
            return asset;
        }

        /// <summary>
        /// AssetCache Constructor
        /// </summary>
        /// <param name="loader">loads and unloads assets</param>
        public KeyedAssetCache(IKeyedAssetLoader<TKeyType,TAssetType> loader)
        {
            _assetLoader = loader;
        }

        /// <summary>
        /// Get Cached Asset
        /// </summary>
        /// <param name="assetKey">Key for the given asset. Should be handle-able by the asset locator</param>
        /// <returns>Task that returns the cached asset</returns>
        public async Task<ICachedAsset> GetAsync(TKeyType assetKey)
        {
            //Get or create cache entry
            if (!CacheDictionary.TryGetValue(assetKey, out var entry))
            {
                entry = new AssetCacheEntry(assetKey, LocateAssetAsync(assetKey), _assetLoader.Unload);
                CacheDictionary.Add(assetKey, entry);
            }

            //Wait for the task to finish
            if (!entry.Task.IsCompleted)
            {
                await entry.Task;
            }

            return entry.GetCachedAsset();
        }

        /// <summary>
        /// Called to clean up and unload assets that have no remaining references
        /// </summary>
        public void ClearUnusedAssets()
        {
            //Remove Unused entries
            var unusedEntries = CacheDictionary.Where((pair) => (pair.Value.RefCount <= 0)).ToList();
            foreach (var pair in unusedEntries)
            {
                CacheDictionary.Remove(pair.Key);
                pair.Value.Dispose();
            }
        }

    }
}
