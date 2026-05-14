using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 占位音效生成器
/// Tools → Generate Placeholder Audio → 一键生成所有音效
/// </summary>
public class AudioGenerator : EditorWindow
{
    [MenuItem("Tools/Generate Placeholder Audio")]
    private static void GenerateAll()
    {
        string folder = "Assets/Audio";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "Audio");

        // SFX 列表：文件名, 频率(Hz), 时长(秒), 波形类型
        GenerateSFX($"{folder}/SFX_UIClick.wav", 800, 0.08f, WaveType.Blip);
        GenerateSFX($"{folder}/SFX_Coin.wav", 1200, 0.15f, WaveType.Ding);
        GenerateSFX($"{folder}/SFX_Harvest.wav", 400, 0.25f, WaveType.Swoosh);
        GenerateSFX($"{folder}/SFX_Till.wav", 150, 0.20f, WaveType.Thud);
        GenerateSFX($"{folder}/SFX_Plant.wav", 600, 0.10f, WaveType.Pop);
        GenerateSFX($"{folder}/SFX_Buy.wav", 900, 0.20f, WaveType.Ching);
        GenerateSFX($"{folder}/SFX_Sell.wav", 700, 0.18f, WaveType.CoinDrop);
        GenerateSFX($"{folder}/SFX_Equip.wav", 500, 0.12f, WaveType.Clink);

        // 简单 BGM（8 秒循环）
        GenerateBGM($"{folder}/BGM_Spring.wav", 8f);

        AssetDatabase.Refresh();
        Debug.Log("所有占位音效已生成到 Assets/Audio/ 目录");
    }

    private enum WaveType { Blip, Ding, Swoosh, Thud, Pop, Ching, CoinDrop, Clink }

    private static void GenerateSFX(string path, float freq, float duration, WaveType type)
    {
        int sampleRate = 44100;
        int samples = Mathf.Max(1, (int)(sampleRate * duration));
        float[] data = new float[samples];

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float envelope = 1f;

            // 音量包络
            switch (type)
            {
                case WaveType.Blip:    // 短促点击
                    envelope = 1f - t / duration;
                    break;
                case WaveType.Ding:    // 清脆叮
                    envelope = Mathf.Exp(-t * 15f);
                    break;
                case WaveType.Swoosh:  // 摩擦/收获
                    envelope = Mathf.Sin(t * Mathf.PI / duration);
                    freq = 200 + t * 800;
                    break;
                case WaveType.Thud:    // 沉闷低音
                    envelope = Mathf.Exp(-t * 10f);
                    freq = 80 + t * 200;
                    break;
                case WaveType.Pop:     // 轻快啵
                    envelope = Mathf.Exp(-t * 20f);
                    freq = 400 + t * 300;
                    break;
                case WaveType.Ching:   // 收银台
                    envelope = Mathf.Exp(-t * 8f);
                    freq = freq + Mathf.Sin(t * 2000) * 300;
                    break;
                case WaveType.CoinDrop: // 硬币掉落
                    envelope = Mathf.Exp(-t * 12f);
                    freq = 1500 - t * 3000;
                    break;
                case WaveType.Clink:   // 金属碰撞
                    envelope = Mathf.Exp(-t * 18f);
                    freq = freq + Mathf.Sin(t * 3000) * 200;
                    break;
            }

            float sample = Mathf.Sin(2 * Mathf.PI * freq * t) * envelope;

            // 添加噪声增加质感
            if (type == WaveType.Swoosh || type == WaveType.Thud)
                sample += (Random.value - 0.5f) * 0.1f * envelope;

            data[i] = Mathf.Clamp(sample, -1f, 1f);
        }

        WriteWav(path, data, sampleRate);
    }

    private static void GenerateBGM(string path, float duration)
    {
        int sampleRate = 44100;
        int samples = (int)(sampleRate * duration);
        float[] data = new float[samples];

        // 简单和弦进行: C - G - Am - F
        float[] melodyNotes = { 262, 294, 330, 349, 392, 440, 494, 523 }; // C4 大调
        float bpm = 100f;
        float beatDuration = 60f / bpm;

        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;

            // 当前节拍
            int beatIndex = (int)(t / beatDuration) % 16;
            float beatT = (t % beatDuration) / beatDuration;

            // 简化和弦根音
            float harmonyFreq = 0;
            int chord = beatIndex / 4; // 每4拍换和弦
            switch (chord % 4)
            {
                case 0: harmonyFreq = 262; break; // C
                case 1: harmonyFreq = 392; break; // G
                case 2: harmonyFreq = 440; break; // Am
                case 3: harmonyFreq = 349; break; // F
            }

            // 旋律
            int noteIndex = beatIndex % melodyNotes.Length;
            float melodyFreq = melodyNotes[noteIndex];
            float melodyEnv = Mathf.Exp(-beatT * 4f);

            // 低音
            float bass = Mathf.Sin(2 * Mathf.PI * harmonyFreq * 0.5f * t) * 0.3f;

            // 和弦
            float chordWave = 0;
            for (int h = 1; h <= 3; h++)
                chordWave += Mathf.Sin(2 * Mathf.PI * harmonyFreq * h * t) * 0.15f;

            // 旋律
            float melody = Mathf.Sin(2 * Mathf.PI * melodyFreq * t) * 0.2f * melodyEnv;

            // 节奏（底鼓）
            float kick = 0;
            if (beatT < 0.1f)
                kick = Mathf.Sin(2 * Mathf.PI * 60 * t) * 0.4f * (1 - beatT * 10);

            data[i] = Mathf.Clamp(bass + chordWave + melody + kick, -1f, 1f);
        }

        WriteWav(path, data, sampleRate);
    }

    private static void WriteWav(string path, float[] data, int sampleRate)
    {
        int channels = 1;
        int bitsPerSample = 16;
        int byteRate = sampleRate * channels * bitsPerSample / 8;
        int blockAlign = channels * bitsPerSample / 8;
        int dataSize = data.Length * blockAlign;
        int fileSize = 36 + dataSize;

        using (var fs = new FileStream(path, FileMode.Create))
        using (var bw = new BinaryWriter(fs))
        {
            // RIFF 头
            bw.Write(new char[] { 'R', 'I', 'F', 'F' });
            bw.Write(fileSize);
            bw.Write(new char[] { 'W', 'A', 'V', 'E' });

            // fmt 块
            bw.Write(new char[] { 'f', 'm', 't', ' ' });
            bw.Write(16);                // 子块大小
            bw.Write((short)1);          // PCM 格式
            bw.Write((short)channels);    // 声道数
            bw.Write(sampleRate);         // 采样率
            bw.Write(byteRate);           // 字节率
            bw.Write((short)blockAlign);  // 块对齐
            bw.Write((short)bitsPerSample); // 位深度

            // data 块
            bw.Write(new char[] { 'd', 'a', 't', 'a' });
            bw.Write(dataSize);

            foreach (float sample in data)
            {
                short intSample = (short)(Mathf.Clamp(sample, -1f, 1f) * 32767);
                bw.Write(intSample);
            }
        }
    }
}
