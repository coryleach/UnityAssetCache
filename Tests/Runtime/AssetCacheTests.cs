using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Cache.Tests
{
    public class AssetCacheTests
    {
        private AssetCache<Texture2D> CreateCache()
        {
            var cache = new AssetCache<Texture2D>
            {
                //Assign a locator that gets a texture with a little bit of a delay to simulate network delay
                AssetLocator = delegate(string s)
                {
                    Texture2D tex = new Texture2D(32, 32);
                    return Task.Run(() =>
                    {
                        Task.Delay(100);
                        return tex;
                    });
                },
                AssetDestroyer = delegate(Texture2D tex)
                {
                    UnityEngine.Object.Destroy(tex);
                    Resources.UnloadAsset(tex);
                }
            };
            return cache;
        }
        
        [Test]
        public void CreateAsssetCache()
        {
            var cache = CreateCache();
            Assert.IsTrue(cache != null);
        }

        [UnityTest]
        public IEnumerator GetAsset()
        {
            var cache = CreateCache();
            var cachedAssetTask = cache.GetAsync("txone");

            while (!cachedAssetTask.IsCompleted)
            {
                yield return null;
            }

            var cachedAsset = cachedAssetTask.Result;

            Assert.IsTrue(cachedAsset.Asset != null);
            Assert.IsTrue(cachedAsset.RefCount == 1);
        }
        
        [UnityTest]
        public IEnumerator DisposeDecrementsRefCount()
        {
            var cache = CreateCache();
            var cachedAssetTask = cache.GetAsync("txone");

            while (!cachedAssetTask.IsCompleted)
            {
                yield return null;
            }

            var cachedAsset = cachedAssetTask.Result;
            Assert.IsTrue(cachedAsset.Asset != null);
            Assert.IsTrue(cachedAsset.RefCount == 1);
            
            cachedAsset.Dispose();
            
            Assert.IsTrue(cachedAsset.RefCount == 0);
        }
    
        [UnityTest]
        public IEnumerator GetAssetTwice()
        {
            var cache = CreateCache();
            var url = "txone";
            var cachedAssetTask = cache.GetAsync(url);

            while (!cachedAssetTask.IsCompleted)
            {
                yield return null;
            }

            var cachedAsset1 = cachedAssetTask.Result;
            Assert.IsTrue(cachedAsset1.Asset != null);
            Assert.IsTrue(cachedAsset1.RefCount == 1);
            
            cachedAssetTask = cache.GetAsync(url);

            while (!cachedAssetTask.IsCompleted)
            {
                yield return null;
            }

            var cachedAsset2 = cachedAssetTask.Result;
            //Confirm that the two cached assets refer to the same thing but are themselves different
            Assert.IsTrue(cachedAsset1 != cachedAsset2);
            Assert.IsTrue(cachedAsset2.Asset == cachedAsset1.Asset);
            Assert.IsTrue(cachedAsset1.RefCount == 2);
            Assert.IsTrue(cachedAsset2.RefCount == 2);

        }
        
        [UnityTest]
        public IEnumerator ExceptionOnDisposeTwice()
        {
            var cache = CreateCache();
            var url = "txone";
            var cachedAssetTask = cache.GetAsync(url);

            while (!cachedAssetTask.IsCompleted)
            {
                yield return null;
            }

            var cachedAsset = cachedAssetTask.Result;
            var copiedReference = cachedAsset;

            Assert.IsTrue(cachedAsset.Asset != null);
            Assert.IsTrue(cachedAsset.RefCount == 1);
            
            cachedAsset.Dispose();
            Assert.IsTrue(cachedAsset.RefCount == 0);
            
            Assert.Throws<InvalidOperationException>(() =>
            {
                copiedReference.Dispose();
            });
        }
        
        [UnityTest]
        public IEnumerator CanClone()
        {
            var cache = CreateCache();
            var url = "txone";
            var cachedAssetTask = cache.GetAsync(url);

            while (!cachedAssetTask.IsCompleted)
            {
                yield return null;
            }

            var cachedAsset = cachedAssetTask.Result;
            var copiedReference = cachedAsset.Clone();

            Assert.IsTrue(cachedAsset.Asset != null);
            Assert.IsTrue(cachedAsset.RefCount == 2);
            
            cachedAsset.Dispose();
            Assert.IsTrue(cachedAsset.RefCount == 1);
            
            copiedReference.Dispose();
            Assert.IsTrue(cachedAsset.RefCount == 0);
        }
        
        [UnityTest]
        public IEnumerator CanClean()
        {
            var cache = CreateCache();
            cache.AssetDestroyer = delegate(Texture2D texture)
            {
                UnityEngine.Object.Destroy(texture);
                Debug.Log("Destroyed!");
            };
                
            var url = "txone";
            var cachedAssetTask = cache.GetAsync(url);

            while (!cachedAssetTask.IsCompleted)
            {
                yield return null;
            }

            var cachedAsset = cachedAssetTask.Result;
            var copiedReference = cachedAsset.Clone();

            var asset = cachedAsset.Asset;
            
            Assert.IsTrue(cachedAsset.Asset != null);
            Assert.IsTrue(cachedAsset.RefCount == 2);
            
            cachedAsset.Dispose();
            Assert.IsTrue(cachedAsset.RefCount == 1);
            
            cache.ClearUnusedAssets();
            
            //Ensure cached asset was not destroyed yet
            Assert.IsFalse(asset == null);
            
            //Ensure cached asset gets destroyed once ref count hits 0
            copiedReference.Dispose();
            Assert.IsTrue(cachedAsset.RefCount == 0);
            cache.ClearUnusedAssets();
            
            //Must wait a frame after the unload/destroy for asset reference null check to return true
            yield return null;
            Assert.IsTrue(asset == null);
        }
        
    }
    
}
