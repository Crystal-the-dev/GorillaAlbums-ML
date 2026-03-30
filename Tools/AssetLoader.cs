using System.Collections;
using System.IO;
using UnityEngine;

namespace GorillaAlbums.Tools
{
    public class AssetLoader
    {
        public static AssetBundle LoadedBundle { get; private set; }

        public static IEnumerator LoadBundleCoroutine()
        {
            if (LoadedBundle != null)
                yield break;

            Stream stream = typeof(AssetLoader).Assembly.GetManifestResourceStream("GorillaAlbums.Content.shelves");
            if (stream == null)
            {
                MelonLoader.MelonLogger.Error("[GorillaAlbums] Embedded asset bundle 'GorillaAlbums.Content.shelves' not found!");
                yield break;
            }

            byte[] bundleBytes;
            using (var ms = new System.IO.MemoryStream())
            {
                stream.CopyTo(ms);
                bundleBytes = ms.ToArray();
            }
            stream.Dispose();

            AssetBundleCreateRequest request = AssetBundle.LoadFromMemoryAsync(bundleBytes);
            yield return request;

            LoadedBundle = request.assetBundle;

            if (LoadedBundle == null)
                MelonLoader.MelonLogger.Error("[GorillaAlbums] AssetBundle loaded but is null!");
        }
    }
}
