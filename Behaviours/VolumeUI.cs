using UnityEngine;
using UnityEngine.InputSystem;

namespace GorillaAlbums.Behaviours
{
    public class VolumeUI : MonoBehaviour
    {
        private AudioSource _audioSource;
        private bool _visible = false;
        private Rect _windowRect = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 60, 300, 120);
        private float _volume = 1f;

        public static VolumeUI Create(AudioSource audioSource)
        {
            var go = new GameObject("GorillaAlbumsVolumeUI");
            Object.DontDestroyOnLoad(go);
            var ui = go.AddComponent<VolumeUI>();
            ui._audioSource = audioSource;
            ui._volume = audioSource.volume;
            return ui;
        }

        private void Update()
        {
            if (Keyboard.current != null && Keyboard.current.f6Key.wasPressedThisFrame)
                _visible = !_visible;
        }

        private void OnGUI()
        {
            if (!_visible) return;
            _windowRect = GUI.Window(9998, _windowRect, DrawWindow, "GorillaAlbums - Volume");
        }

        private void DrawWindow(int id)
        {
            GUILayout.Space(10);
            GUILayout.Label($"Volume: {Mathf.RoundToInt(_volume * 100)}%");
            float newVolume = GUILayout.HorizontalSlider(_volume, 0f, 1f);

            if (!Mathf.Approximately(newVolume, _volume))
            {
                _volume = newVolume;
                if (_audioSource != null)
                    _audioSource.volume = _volume;
            }

            GUILayout.Space(5);
            if (GUILayout.Button("Close"))
                _visible = false;

            GUI.DragWindow();
        }
    }
}
