using GabesCommonUtility.Settings.Scriptables;
using UnityEngine;
using AudioSettings = GabesCommonUtility.Settings.Scriptables.AudioSettings;

namespace GabesCommonUtility.Settings
{
   public static class Settings
   {
      public static AudioSettings AudioSettings { get; private set; }
      public static ControlsSettings ControlsSettings { get; private set; }
      public static GameSettings GameSettings { get; private set; }
      public static LanguageSettings LanguageSettings { get; private set; }
      public static VideoSettings VideoSettings { get; private set; }

      [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
      private static void InitializeSettings()
      {
         AudioSettings = Resources.Load<AudioSettings>("AudioSettings");
         ControlsSettings = Resources.Load<ControlsSettings>("ControlsSettings");
         GameSettings = Resources.Load<GameSettings>("GameSettings");
         LanguageSettings = Resources.Load<LanguageSettings>("LanguageSettings");
         VideoSettings = Resources.Load<VideoSettings>("VideoSettings");
      }
   }
}
