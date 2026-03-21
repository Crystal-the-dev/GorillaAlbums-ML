using System.Threading.Tasks;
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
        private GameObject _shelfPrefab;
        private bool _initialized;
        
        public override void OnInitializeMelon()
        {
            GorillaTagger.OnPlayerSpawned(OnPlayerSpawned);
        }

        private void OnPlayerSpawned()
        {
            if (_initialized) return;

            _initialized = true;
            _ = Setup();
        }

        private async Task Setup()
        {
            MelonLogger.Msg("[GorillaAlbums] Initializing...");

            ImageManager.CreateImageFolder();

            _shelfPrefab = await AssetLoader.LoadAsset<GameObject>("GorillaAlbums");

            if (_shelfPrefab == null)
            {
                Debug.LogError("[GorillaAlbums] Failed loading shelf prefab!");
                return;
            }
           
            GameObject shelf = Object.Instantiate(_shelfPrefab);
            shelf.SetActive(true);

            shelf.transform.position = new Vector3(-64.7606f, 12.1637f, -84.5819f);
            shelf.transform.rotation = Quaternion.Euler(0f, 271.3724f, 0f);

            ErrorManager.CheckShelf(shelf);
            ImageManager.ApplyImages(shelf);

            MelonLogger.Msg("[GorillaAlbums] Setup complete.");
        }
    }
}