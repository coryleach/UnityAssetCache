using System.Threading.Tasks;

namespace Gameframe.AssetCache
{
    /// <summary>
    /// Interface for loading and unloading assets in AssetCache
    /// </summary>
    /// <typeparam name="TAssetType">UnityEngine.Object asset type</typeparam>
    public interface IAssetLoader<TAssetType> where TAssetType : class
    {
        /// <summary>
        /// Loads an asset asynchronously
        /// </summary>
        /// <param name="assetPath">path to the asset to be loaded</param>
        /// <returns>Task that returns the asset type</returns>
        Task<TAssetType> LoadAsync(string assetPath);

        /// <summary>
        /// Unloads an asset
        /// </summary>
        /// <param name="asset">Asset to be unloaded</param>
        void Unload(TAssetType asset);
    }
}
