using System.Drawing;
using System.Xml.Serialization;

namespace MillionaireGame.Core.Settings
{
    /// <summary>
    /// Settings for broadcast/streaming configuration
    /// </summary>
    public class BroadcastSettings
    {
        /// <summary>
        /// Background rendering mode
        /// </summary>
        [XmlElement("BackgroundMode")]
        public BackgroundMode Mode { get; set; } = BackgroundMode.Prerendered;

        /// <summary>
        /// Selected prerendered background path (relative to theme folder)
        /// </summary>
        [XmlElement("SelectedBackground")]
        public string? SelectedBackgroundPath { get; set; }

        /// <summary>
        /// Chroma key color in hex format (e.g., "#0000FF")
        /// </summary>
        [XmlElement("ChromaKeyColor")]
        public string ChromaKeyColorHex { get; set; } = "#0000FF"; // Default: Blue

        /// <summary>
        /// Gets the chroma key color as a Color object
        /// </summary>
        [XmlIgnore]
        public Color ChromaKeyColor
        {
            get
            {
                try
                {
                    return ColorTranslator.FromHtml(ChromaKeyColorHex);
                }
                catch
                {
                    return Color.Blue; // Fallback
                }
            }
            set
            {
                ChromaKeyColorHex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
            }
        }

        /// <summary>
        /// Check if a color conflicts with game UI colors
        /// </summary>
        public static bool IsColorConflict(Color chromaKey, out string conflictingElement)
        {
            const int threshold = 40; // RGB difference threshold

            var gameColors = new[]
            {
                (Color.FromArgb(0, 255, 0), "Correct Answer (Green)"),
                (Color.FromArgb(255, 0, 0), "Wrong Answer (Red)"),
                (Color.FromArgb(255, 255, 0), "Money Value (Yellow)"),
                (Color.FromArgb(255, 165, 0), "UI Elements (Orange)"),
                (Color.FromArgb(0, 255, 255), "Lifeline Indicator (Cyan)")
            };

            foreach (var (gameColor, elementName) in gameColors)
            {
                if (ColorDistance(chromaKey, gameColor) < threshold)
                {
                    conflictingElement = elementName;
                    return true;
                }
            }

            conflictingElement = string.Empty;
            return false;
        }

        /// <summary>
        /// Calculate color distance (simple Euclidean distance in RGB space)
        /// </summary>
        private static double ColorDistance(Color c1, Color c2)
        {
            int rDiff = c1.R - c2.R;
            int gDiff = c1.G - c2.G;
            int bDiff = c1.B - c2.B;
            return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
        }
    }

    /// <summary>
    /// Background rendering modes
    /// </summary>
    public enum BackgroundMode
    {
        /// <summary>
        /// Use prerendered theme background image
        /// </summary>
        Prerendered,

        /// <summary>
        /// Use solid color for chroma keying
        /// </summary>
        ChromaKey
    }
}
