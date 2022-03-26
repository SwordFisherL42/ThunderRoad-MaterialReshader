using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using UnityEngine;

namespace EyesOfIbad
{
    public class Settings
    {
        const float normalizationFactor = 25.5f;
        const float alphaFactor = 1f;
        Color eyeColor;
        public float[] eyeColorRGB;
        public float glowIntensityHDR;

        public Color GetColor() { return eyeColor; }

        [OnDeserialized]
        void SetColor(StreamingContext context)
        {
            eyeColor = new Color(eyeColorRGB[0] / normalizationFactor, eyeColorRGB[1] / normalizationFactor, eyeColorRGB[2] / normalizationFactor, alphaFactor);
        }

        public static Settings ReadFromDisk(string settingsFile)
        {
            string json = File.ReadAllText(Path.Combine(Application.dataPath, settingsFile));
            return JsonConvert.DeserializeObject<Settings>(json);
        }

        public override string ToString()
        {
            string repr = base.ToString();
            foreach (FieldInfo field in GetType().GetFields())
                repr += $"\n{field.Name}: {field.GetValue(this)}";
            return repr;
        }
    }
}