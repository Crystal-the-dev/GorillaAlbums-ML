using System.Collections;
using MelonLoader;
using GorillaAlbums.Tools;
using GorillaAlbums.Behaviours;
using UnityEngine;
using Object = UnityEngine.Object;

[assembly: MelonInfo(typeof(GorillaAlbums.Plugin), "GorillaAlbums", "1.0.5", "elligurt")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]

namespace GorillaAlbums
{
    public class Plugin : MelonMod
    {
        private bool _initialized;

        public override void OnInitializeMelon()
        {
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        }

        private void OnPlayerSpawned()
        {
            if (_initialized) return;
            _initialized = true;

            MelonCoroutines.Start(SetupCoroutine());
        }

        private IEnumerator SetupCoroutine()
        {
            MelonLogger.Msg("[GorillaAlbums] Initializing...");

            ImageManager.CreateImageFolder();

            var bundleRequest = AssetLoader.LoadBundleCoroutine();
            yield return bundleRequest;

            if (AssetLoader.LoadedBundle == null)
            {
                MelonLogger.Error("[GorillaAlbums] Failed to load asset bundle!");
                yield break;
            }

            AssetBundleRequest assetRequest = AssetLoader.LoadedBundle.LoadAssetAsync<GameObject>("GorillaAlbums");
            yield return assetRequest;

            GameObject prefab = assetRequest.asset as GameObject;
            if (prefab == null)
            {
                MelonLogger.Error("[GorillaAlbums] Failed loading shelf prefab!");
                yield break;
            }

            GameObject shelf = Object.Instantiate(prefab);
            shelf.SetActive(true);

            Transform playerObj = shelf.transform.Find("GorillaAlbumsPlayer");
            if (playerObj != null)
            {
                playerObj.gameObject.SetActive(true);

                Transform shelfStopperLower = playerObj.Find("ShelfStopperLower");
                if (shelfStopperLower != null)
                    shelfStopperLower.gameObject.SetActive(true);
            }

            Transform mediaController = shelf.transform.Find("MediaController");
            if (mediaController != null)
            {
                mediaController.gameObject.SetActive(true);
                MediaControllerBehaviour.Create(mediaController, playerObj);
            }

            shelf.transform.position = new Vector3(-64.7606f, 12.1637f, -84.5819f);
            shelf.transform.rotation = Quaternion.Euler(0f, 271.3724f, 0f);

            ErrorManager.CheckShelf(shelf);
            ImageManager.ApplyImages(shelf);

            MelonLogger.Msg("[GorillaAlbums] Setup complete.");
        }
    }
}
