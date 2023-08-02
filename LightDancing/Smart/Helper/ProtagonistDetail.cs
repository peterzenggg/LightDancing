using LightDancing.Enums;
using LightDancing.MusicAnalysis;
using System.Collections.Generic;

namespace LightDancing.Smart.Helper
{
    public class ProtagonistDetail
    {
        private ProtagonistShaders? mode;

        private ProtagonistShaders? previousMode;

        public ProtagonistShaders? Mode
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
        /// Random a animation from source without the previousMode
        /// </summary>
        /// <param name="source">Animation source</param>
        public void InitCurrentMode(List<ProtagonistShaders> source)
        {
            Mode = previousMode.HasValue ? CommonMethods.RandomEnumFromSource(source, previousMode.Value) : CommonMethods.RandomEnumFromSource(source);
        }

        /// <summary>
        /// Get current animation
        /// </summary>
        /// <returns>current animation</returns>
        public ProtagonistShaders? GetShader()
        {
            if (mode == null)
            {
                return null;
            }
            else
            {
                return (ProtagonistShaders)mode;
            }
        }
    }
}
