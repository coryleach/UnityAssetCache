<h1 align="center">Welcome to com.gameframe.assetcache 👋</h1>
<p>
  <img alt="Version" src="https://img.shields.io/badge/version-1.0.1-blue.svg?cacheSeconds=2592000" />
  <a href="https://twitter.com/coryleach">
    <img alt="Twitter: coryleach" src="https://img.shields.io/twitter/follow/coryleach.svg?style=social" target="_blank" />
  </a>
</p>

> Library for caching and unloading assets

## Quick Package Install

#### Using UnityPackageManager (for Unity 2019.1 or later)

Find the manifest.json file in the Packages folder of your project and edit it to look like this:
```js
{
  "dependencies": {
    "com.gameframe.assetcache": "https://github.com/coryleach/UnityAssetCache.git#1.0.1",
    ...
  },
}
```

## Usage

```c#
//Create an asset cache (like a WebTextureCache)
var cache = new WebTextureCache();

//Get a cached asset asynchronously
var cachedAsset = await cache.GetAsync(url);

//Use the asset
image.texture = cachedAsset.Asset;

//Dispose the reference so the cache knows you no longer are using it
cachedAsset.Dispose();
cachedAsset = null;

//Clean the cache so all assets with zero references are unloaded
cache.ClearUnusedAssets();
```

## Author

👤 **Cory Leach**

* Twitter: [@coryleach](https://twitter.com/coryleach)
* Github: [@coryleach](https://github.com/coryleach)

## Show your support

Give a ⭐️ if this project helped you!

***
_This README was generated with ❤️ by [readme-md-generator](https://github.com/kefranabg/readme-md-generator)_
