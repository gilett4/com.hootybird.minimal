using System.Collections.Generic;

namespace HootyBird.Minimal.Tools
{
    public static class Settings
    {
        public static partial class InternalAppSettings
        {
            public static string MainMenuControllerName = "MainMenuCanvas";
            public static string GameplayMenuControllerName = "GameplayCanvas";
            /// <summary>
            /// Target framerate.
            /// </summary>
            public static int TargetFramerate = 120;
        }
    }
}
