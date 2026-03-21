using System.IO;
using System.Linq;
using UnityEngine;
using MelonLoader.Utils;

namespace GorillaAlbums.Behaviours
{
    public static class ImageManager
    {
        private static string rootFolder;

        public static void CreateImageFolder()
        {
            rootFolder = Path.Combine(MelonEnvironment.ModsDirectory, "AlbumCovers");
            Directory.CreateDirectory(rootFolder);

            for (int i = 1; i <= 4; i++)
                Directory.CreateDirectory(Path.Combine(rootFolder, $"Album{i}"));
        }

        public static void ApplyImages(GameObject parent)
        {
            if (string.IsNullOrEmpty(rootFolder))
                rootFolder = Path.Combine(MelonEnvironment.ModsDirectory, "AlbumCovers");

            for (int i = 1; i <= 4; i++)
            {
                Transform record = parent.transform.FindDeepChild($"Record{i}");
                if (record == null) continue;

                string folder = Path.Combine(rootFolder, $"Album{i}");

                var images = Directory.GetFiles(folder, "*.*")
                    .Where(f => f.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase)
                             || f.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
                    .ToArray();

                if (images.Length == 0) continue; 

                string imagePath = images[0];

                byte[] bytes = File.ReadAllBytes(imagePath);
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.LoadImage(bytes);
                tex.Apply();

                Renderer[] renderers = record.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer rend in renderers)
                {
                    foreach (Material mat in rend.materials)
                    {
                        if (mat.HasProperty("_BaseMap"))
                            mat.SetTexture("_BaseMap", tex);
                        if (mat.HasProperty("_MainTex"))
                            mat.SetTexture("_MainTex", tex);
                    }
                }
            }
        }

        public static Transform FindDeepChild(this Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;

                Transform result = child.FindDeepChild(name);
                if (result != null) return result;
            }
            return null;
        }
    }
}