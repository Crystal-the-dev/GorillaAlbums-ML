using System.IO;
using System.Linq;
using UnityEngine;
using MelonLoader.Utils;

namespace GorillaAlbums.Behaviours
{
    public static class ErrorManager
    {
        public static void CheckShelf(GameObject shelf)
        {
            string rootFolder = Path.Combine(MelonEnvironment.ModsDirectory, "AlbumCovers");

            for (int i = 1; i <= 4; i++)
            {
                Transform record = shelf.transform.FindDeepChild($"Record{i}");
                if (record == null) continue;

                string folder = Path.Combine(rootFolder, $"Album{i}");
                Transform errorGO = record.FindDeepChild("Error");

                bool hasError = true;
                if (Directory.Exists(folder))
                {
                    var images = Directory.GetFiles(folder, "*.*")
                        .Where(f => f.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                                 || f.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
                        .ToArray();
                    hasError = images.Length == 0;
                }

                if (errorGO != null)
                    errorGO.gameObject.SetActive(hasError);
            }
        }
    }
}
