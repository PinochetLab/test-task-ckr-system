using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace TestTask.EditorTools.SceneBuilder
{
    /// <summary>
    /// Генерирует короткий звук клика (WAV), чтобы не тащить в проект сторонние ассеты.
    /// </summary>
    internal static class ClickSoundGenerator
    {
        private const int SampleRate = 44100;
        private const float Duration = 0.09f;
        private const float StartFrequency = 1200f;
        private const float EndFrequency = 650f;

        public static AudioClip GetOrCreate(string assetPath)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
            if (clip != null)
                return clip;

            string fullPath = Path.Combine(Directory.GetCurrentDirectory(), assetPath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            WriteWav(fullPath);

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
            return AssetDatabase.LoadAssetAtPath<AudioClip>(assetPath);
        }

        private static void WriteWav(string path)
        {
            int sampleCount = (int)(SampleRate * Duration);
            const short channels = 1;
            const short bitsPerSample = 16;
            int dataSize = sampleCount * channels * bitsPerSample / 8;

            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write(channels);
            writer.Write(SampleRate);
            writer.Write(SampleRate * channels * bitsPerSample / 8);
            writer.Write((short)(channels * bitsPerSample / 8));
            writer.Write(bitsPerSample);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            double phase = 0d;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / SampleRate;
                float progress = (float)i / sampleCount;
                float frequency = Mathf.Lerp(StartFrequency, EndFrequency, progress);
                phase += 2d * Mathf.PI * frequency / SampleRate;

                float attack = Mathf.Clamp01(i / 60f);
                float envelope = Mathf.Exp(-t * 45f) * attack;
                float sample = (float)System.Math.Sin(phase) * envelope * 0.7f;

                writer.Write((short)(Mathf.Clamp(sample, -1f, 1f) * short.MaxValue));
            }
        }
    }
}
