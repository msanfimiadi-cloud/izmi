#if UNITY_EDITOR
using System;
using System.IO;
using System.Net.Http;
using UnityEditor;
using UnityEngine;

namespace Izmi.Editor
{
    public static class EarthAssetDownloader
    {
        private const string SourceUrl =
            "https://svs.gsfc.nasa.gov/vis/a000000/a002900/a002915/bluemarble-2048.png";

        private const string AssetFolder = "Assets/Resources/Earth";
        private const string AssetPath = AssetFolder + "/BlueMarble.png";

        [MenuItem("IZMI/Download NASA Earth Texture")]
        public static void Download()
        {
            try
            {
                Directory.CreateDirectory(AssetFolder);

                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromMinutes(3);
                var bytes = client.GetByteArrayAsync(SourceUrl).GetAwaiter().GetResult();

                File.WriteAllBytes(AssetPath, bytes);
                AssetDatabase.ImportAsset(AssetPath, ImportAssetOptions.ForceUpdate);
                ConfigureImporter();

                Debug.Log(
                    "IZMI: NASA Blue Marble texture downloaded and imported. " +
                    "Credit: NASA/Goddard Space Flight Center Scientific Visualization Studio.");

                EditorUtility.DisplayDialog(
                    "IZMI",
                    "Текстура Земли NASA загружена. Теперь можно запустить сцену GlobePrototype.",
                    "Готово");
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorUtility.DisplayDialog(
                    "IZMI — ошибка загрузки",
                    "Не удалось загрузить текстуру NASA. Проверьте интернет-соединение и повторите попытку.\n\n" +
                    exception.Message,
                    "Закрыть");
            }
        }

        private static void ConfigureImporter()
        {
            if (AssetImporter.GetAtPath(AssetPath) is not TextureImporter importer)
            {
                return;
            }

            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.mipmapEnabled = true;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
    }
}
#endif
