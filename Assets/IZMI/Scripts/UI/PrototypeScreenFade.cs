using System.Collections;
using UnityEngine;

namespace Izmi
{
    public sealed class PrototypeScreenFade : MonoBehaviour
    {
        private static PrototypeScreenFade instance;
        private Texture2D blackTexture;
        private float alpha;

        public static IEnumerator FadeTo(float targetAlpha, float duration)
        {
            EnsureInstance();

            var startAlpha = instance.alpha;
            var elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                instance.alpha = Mathf.Lerp(
                    startAlpha,
                    targetAlpha,
                    Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            instance.alpha = targetAlpha;
        }

        private static void EnsureInstance()
        {
            if (instance != null)
            {
                return;
            }

            var fadeObject = new GameObject("Screen Fade");
            instance = fadeObject.AddComponent<PrototypeScreenFade>();
            DontDestroyOnLoad(fadeObject);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            blackTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            blackTexture.SetPixel(0, 0, Color.black);
            blackTexture.Apply();
        }

        private void OnGUI()
        {
            if (alpha <= 0.001f || blackTexture == null)
            {
                return;
            }

            var previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.DrawTexture(
                new Rect(0f, 0f, Screen.width, Screen.height),
                blackTexture,
                ScaleMode.StretchToFill);
            GUI.color = previousColor;
        }

        private void OnDestroy()
        {
            if (blackTexture != null)
            {
                Destroy(blackTexture);
            }
        }
    }
}
