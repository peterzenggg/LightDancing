using LightDancing.Enums;
using LightDancing.MusicAnalysis;
using System.Collections.Generic;

namespace LightDancing.Smart.Helper
{
    public class BackgroundDetail
    {
        private BackgroundShaders? mode;

        private BackgroundShaders? previousMode;

        public BackgroundShaders? Mode
        {
            get
            {
                return mode;
            }
            set
            {
                mode = value;
                if (value != null)
                {
                    previousMode = value;
                }
            }
        }

        public int Counter { get; set; }

        /// <summary>
        /// Reset all variable
        /// </summary>
        public void Reset()
        {
            Mode = null;
            Counter = 0;
        }

        /// <summary>
        /// Random a animation form BackgroundMode without the previousMode
        /// </summary>
        public void InitCurrentMode(List<BackgroundShaders> source)
        {
            Mode = previousMode.HasValue ? CommonMethods.RandomEnumFromSource(source, previousMode.Value) : CommonMethods.RandomEnumFromSource(source);
        }

        /// <summary>
        /// Get current animation
        /// </summary>
        /// <returns>current animation</returns>
        public BackgroundShaders? GetShader()
        {
            if (mode == null)
            {
                return null;
            }
            else
            {
                return (BackgroundShaders)mode;
            }
        }
    }
}
