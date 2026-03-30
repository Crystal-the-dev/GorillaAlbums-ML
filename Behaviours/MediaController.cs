using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using MelonLoader;
using MelonLoader.Utils;
using TMPro;

namespace GorillaAlbums.Behaviours
{
    public class AlbumButton : GorillaPressableButton
    {
        public System.Action OnPressed;

        public override void ButtonActivation()
        {
            base.ButtonActivation();
            OnPressed?.Invoke();
        }
    }

    public class MediaControllerBehaviour : MonoBehaviour
    {
        private AudioSource _audioSource;
        private AudioClip[] _clips = new AudioClip[4];
        private string[] _titles = new string[4];
        private int _currentIndex = 0;
        private bool _isPaused = true;
        private TextMeshPro _songText;

        private static readonly int GorillaInteractableLayer = LayerMask.NameToLayer("GorillaInteractable");

        public static MediaControllerBehaviour Create(Transform mediaController, Transform playerRoot)
        {
            var go = new GameObject("MediaControllerLogic");
            go.transform.SetParent(mediaController, false);
            var behaviour = go.AddComponent<MediaControllerBehaviour>();

            behaviour.SetupButton(mediaController.Find("LeftButton"), behaviour.OnLeft);
            behaviour.SetupButton(mediaController.Find("RightButton"), behaviour.OnRight);

            Transform pauseBtn = null;
            foreach (Transform child in mediaController)
            {
                if (child.name == "Pause/Unpause")
                {
                    pauseBtn = child;
                    break;
                }
            }
            behaviour.SetupButton(pauseBtn, behaviour.OnPauseToggle);

            if (playerRoot != null)
            {
                Transform nowPlaying = playerRoot.FindDeepChild("NowPlaying");
                if (nowPlaying != null)
                {
                    Transform songChild = nowPlaying.Find("Song");
                    if (songChild != null)
                        behaviour._songText = songChild.GetComponent<TextMeshPro>();
                }
            }

            return behaviour;
        }

        private void SetupButton(Transform buttonTransform, System.Action callback)
        {
            if (buttonTransform == null)
            {
                MelonLogger.Warning("[GorillaAlbums] Button transform is null, skipping.");
                return;
            }

            buttonTransform.gameObject.layer = GorillaInteractableLayer;

            if (buttonTransform.GetComponent<Collider>() == null)
            {
                var box = buttonTransform.gameObject.AddComponent<BoxCollider>();
                box.size = Vector3.one * 0.1f;
                box.isTrigger = true;
            }

            var existing = buttonTransform.GetComponent<GorillaPressableButton>();
            if (existing != null)
                Destroy(existing);

            var btn = buttonTransform.gameObject.AddComponent<AlbumButton>();
            btn.OnPressed = callback;

            MelonLogger.Msg($"[GorillaAlbums] Setup button: {buttonTransform.name}");
        }

        private void Start()
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.spatialBlend = 1f;
            _audioSource.maxDistance = 10f;
            _audioSource.rolloffMode = AudioRolloffMode.Linear;
            _audioSource.loop = false;

            VolumeUI.Create(_audioSource);

            MelonCoroutines.Start(LoadAllClips());
        }

        private IEnumerator LoadAllClips()
        {
            string root = Path.Combine(MelonEnvironment.ModsDirectory, "AlbumCovers");

            for (int i = 0; i < 4; i++)
            {
                string folder = Path.Combine(root, $"Album{i + 1}");

                string jsonPath = Path.Combine(folder, "info.json");
                if (File.Exists(jsonPath))
                {
                    AlbumInfo info = JsonUtility.FromJson<AlbumInfo>(File.ReadAllText(jsonPath));
                    _titles[i] = info?.title ?? $"Album {i + 1}";
                }
                else
                {
                    _titles[i] = $"Album {i + 1}";
                }

                string songPath = Path.Combine(folder, "song.mp3");
                if (!File.Exists(songPath)) continue;

                using (UnityWebRequest req = UnityWebRequestMultimedia.GetAudioClip("file:///" + songPath.Replace("\\", "/"), AudioType.MPEG))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        _clips[i] = DownloadHandlerAudioClip.GetContent(req);
                        MelonLogger.Msg($"[GorillaAlbums] Loaded: {_titles[i]}");
                    }
                    else
                    {
                        MelonLogger.Warning($"[GorillaAlbums] Failed Album{i + 1}: {req.error}");
                    }
                }
            }

            UpdateSongText();
        }

        private void PlayCurrent()
        {
            if (_clips[_currentIndex] == null) return;
            _audioSource.clip = _clips[_currentIndex];
            _audioSource.Play();
            _isPaused = false;
            UpdateSongText();
        }

        private void UpdateSongText()
        {
            if (_songText != null)
                _songText.text = _titles[_currentIndex] ?? "";
        }

        private void Update()
        {
            if (_audioSource.clip != null && !_audioSource.isPlaying && !_isPaused)
            {
                int next = (_currentIndex + 1) % 4;
                for (int i = 0; i < 4; i++)
                {
                    if (_clips[next] != null) break;
                    next = (next + 1) % 4;
                }
                if (_clips[next] != null && next != _currentIndex)
                {
                    _currentIndex = next;
                    PlayCurrent();
                }
            }
        }

        private void OnLeft()
        {
            int prev = (_currentIndex - 1 + 4) % 4;
            for (int i = 0; i < 4; i++)
            {
                if (_clips[prev] != null) break;
                prev = (prev - 1 + 4) % 4;
            }
            if (_clips[prev] != null)
            {
                _currentIndex = prev;
                PlayCurrent();
            }
        }

        private void OnRight()
        {
            int next = (_currentIndex + 1) % 4;
            for (int i = 0; i < 4; i++)
            {
                if (_clips[next] != null) break;
                next = (next + 1) % 4;
            }
            if (_clips[next] != null)
            {
                _currentIndex = next;
                PlayCurrent();
            }
        }

        private void OnPauseToggle()
        {
            if (_isPaused)
            {
                if (_clips[_currentIndex] == null) return;

                if (_audioSource.clip == null)
                {
                    PlayCurrent();
                    return;
                }

                _audioSource.UnPause();
                _isPaused = false;
            }
            else
            {
                _audioSource.Pause();
                _isPaused = true;
            }
        }

        [System.Serializable]
        private class AlbumInfo
        {
            public string title;
        }
    }
}
