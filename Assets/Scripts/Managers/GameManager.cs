using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utilities;
using GabesCommonUtility;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        private bool _inGame;
    
        private static GameManager Instance;
    
        public static bool InGame => Instance._inGame;

        [SerializeField] private float intendedDelay = 1;
    
        [SerializeField] private int loadingSceneIndex = 3; // Index of the loading scene
        
        //Honestly, this should be in another script, but I cant be bothered.
        [Header("Sounds")]
        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip roundStart;
        [SerializeField] private AudioClip stealthMusic;
        [SerializeField] private AudioClip chaseMusic;
        [SerializeField] private AudioClip mainMusic;

        [SerializeField] private AudioVolumeRangeSet[] audioSets;
        
        //Should not be here, but this is the fastest way to do it
        [Header("Buttons")]
        [SerializeField] private Button toMainMenu;
        [SerializeField] private Button quitGame;
        

        public static readonly Dictionary<string, AudioVolumeRangeSet> SoundsDictionary =
            new Dictionary<string, AudioVolumeRangeSet>();

        private const float StartTime = 6.4f;

        public static int NumChickens { get; private set; }
        public static int NumChickensSaved { get; private set; }

        public static float TimeInLevel { get; private set; }

        private void Awake()
        {
            if (Instance && Instance != this)
            {
                Destroy(Instance.gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            Instance = this;

            source.time = StartTime;
            
            SettingsManager.SaveFile.onMusicVolumeChanged += x =>
            {
                source.volume = x;
            };

            SoundsDictionary.Clear();
            foreach (var set in audioSets)
            {
                SoundsDictionary.Add(set.tag, set);
            }

            toMainMenu.gameObject.SetActive(false);
            quitGame.gameObject.SetActive(false);
            
            //Because the first scene counts as nothing :)
            #if !UNITY_EDITOR
            if (SceneManager.loadedSceneCount <= 1)
            {
                StartCoroutine(InitialSceneLoad());
            }
            
            toMainMenu.gameObject.SetActive(false);
            #if !UNITY_WEBGL // You can't do this in webgl
            quitGame.gameObject.SetActive(true);
            #endif
            #endif
        }

        private IEnumerator InitialSceneLoad()
        {
            // Load loading screen
            yield return SceneManager.LoadSceneAsync(loadingSceneIndex, LoadSceneMode.Additive);
            
            // Wait for LoadingScreen to initialize
            yield return new WaitUntil(() => LoadingScreen.Instance != null);
            
            // Load main menu
            yield return SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            
            // Close transition (reveal main menu)
            LoadingScreen.Instance.PlayCloseTransition();
            yield return new WaitForSeconds(1f); // Wait for transition to complete
            
            // Unload loading screen
            yield return SceneManager.UnloadSceneAsync(loadingSceneIndex);
        }

        public static void PlayUISound(AudioClip clip)
        {
            Instance.source.PlayOneShot(clip, SettingsManager.currentSettings.SoundVolume);
        }

        private IEnumerator LoadGameImpl()
        {
            _inGame = true;
            NumChickens = 0;
            NumChickensSaved = 0;
            toMainMenu.gameObject.SetActive(true);
            quitGame.gameObject.SetActive(false);
            
            // Load loading screen
            yield return SceneManager.LoadSceneAsync(loadingSceneIndex, LoadSceneMode.Additive);
            yield return new WaitUntil(() => LoadingScreen.Instance != null);
            
            // Open transition (cover the screen)
            bool transitionComplete = false;
            LoadingScreen.Instance.PlayOpenTransition(() => transitionComplete = true);
            yield return new WaitUntil(() => transitionComplete);
            
            source.Stop();
            
            DateTime currentTime = DateTime.Now;
            
            // Unload main menu
            yield return SceneManager.UnloadSceneAsync(1);
            
            // Load game scene
            yield return SceneManager.LoadSceneAsync(2, LoadSceneMode.Additive);
            
            yield return ReadyGame(currentTime);
            
            // Unload loading screen
            yield return SceneManager.UnloadSceneAsync(loadingSceneIndex);
        }

        private IEnumerator ReadyGame(DateTime startTime)
        {
            var timeSpan = DateTime.Now.Subtract(startTime);
            float s = intendedDelay - timeSpan.Seconds;
            if (s > 0) yield return new WaitForSeconds(s);
            
            // Close transition (reveal the game/menu)
            if (LoadingScreen.Instance != null)
            {
                bool transitionComplete = false;
                LoadingScreen.Instance.PlayCloseTransition(() => transitionComplete = true);
                yield return new WaitUntil(() => transitionComplete);
            }

            if (_inGame)
            {
                source.PlayOneShot(roundStart, SettingsManager.currentSettings.SoundVolume);
                source.clip = stealthMusic;
            }
            else
            {
                source.clip = mainMusic;
                source.time = StartTime;
            }
            source.Play();
            TimeInLevel = 0;
        }

        private IEnumerator LoadMenuImpl()
        {
            _inGame = false;
            toMainMenu.gameObject.SetActive(false);
            #if !UNITY_WEBGL // You can't do this in webgl
            quitGame.gameObject.SetActive(true);
            #endif
            
            // Load loading screen
            yield return SceneManager.LoadSceneAsync(loadingSceneIndex, LoadSceneMode.Additive);
            yield return new WaitUntil(() => LoadingScreen.Instance != null);
            
            // Open transition
            bool transitionComplete = false;
            LoadingScreen.Instance.PlayOpenTransition(() => transitionComplete = true);
            yield return new WaitUntil(() => transitionComplete);
            
            source.Stop();

            DateTime currentTime = DateTime.Now;

            // Unload game scene
            yield return SceneManager.UnloadSceneAsync(2);
            
            // Load main menu
            yield return SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
            
            yield return ReadyGame(currentTime);
            
            // Unload loading screen
            yield return SceneManager.UnloadSceneAsync(loadingSceneIndex);
        }

        public static void LoadGame()
        {
            Instance.StartCoroutine(Instance.LoadGameImpl());
        }

        public static void LoadMainMenu()
        {
            Instance.StartCoroutine(Instance.LoadMenuImpl());
        }

        public static void CloseGame()
        {
            Application.Quit();
        }

        public static void TransitionGameMusic(bool isChasing, float duration)
        {
            Instance.StartCoroutine(Instance.source.TransitionSound(isChasing ? Instance.chaseMusic : Instance.stealthMusic, duration));
        }

        public static void RegisterAIEscape()
        {
            ++NumChickensSaved;
        }

        public static void RegisterAIChicken()
        {
            ++NumChickens;
        }
        private void LateUpdate()
        {
            if (!_inGame) return;

            TimeInLevel += Time.deltaTime;
        }
    }

    [Serializable]
    public struct AudioVolumeRangeSet
    {
        public AudioClip clip;
        [Range(0, 1)] public float volume;
        [Min(0)] public float rangeMultiplier;
        public string tag; // Alternative (which would be for the best) is to make a custom editor... and That's not happenening, atleast not right now.
    }
}