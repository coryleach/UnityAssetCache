<p align="center">
<img align="center" src="https://raw.githubusercontent.com/coryleach/UnityPackages/master/Documentation/GameframeFace.gif" />
</p>
<h1 align="center">Gameframe.AssetCache 👋</h1>

<!-- BADGE-START ->
<img align="center" src="https://raw.githubusercontent.com/coryleach/UnityPackages/master/Documentation/GameframeFace.gif" />
</p>
<h1 align="center">Gameframe.AssetCache 👋</h1>

<!-- BADGE-START ->
<img align="center" src="https://raw.githubusercontent.com/coryleach/UnityPackages/master/Documentation/GameframeFace.gif" />
</p>
<h1 align="center">Gameframe.AssetCache 👋</h1>

<!-- BADGE-START -<!-- BADGE-END -->

Library for caching and unloading assets

## Quick Package Install

#### Using UnityPackageManager (for Unity 2019.3 or later)
Open the package manager window (menu: Window > Package Manager)<br/>
Select "Add package from git URL...", fill in the pop-up with the following link:<br/>
https://github.com/coryleach/UnityAssetCache.git#1.0.1<br/>

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

<!-- DOC-START -->
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
<!-- DOC-END -->

## Author

👤 **Cory Leach**

* Twitter: [@coryleach](https://twitter.com/coryleach)
* Github: [@coryleach](https://github.com/coryleach)


## Show your support
Give a ⭐️ if this project helped you!
<br />
Please consider supporting it either by contributing to the Github projects (submitting bug reports or features and/or creating pull requests) or by buying me coffee using any of the links below. Every little bit helps!
<br />

[![ko-fi](https://www.ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/coryleach)


***
_This README was generated with ❤️ by [Gameframe.Packages](https://github.com/coryleach/unitypackages)_
