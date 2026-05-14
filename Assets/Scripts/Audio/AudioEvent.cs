/// <summary>
/// 音频事件枚举
/// </summary>
public enum AudioEvent
{
    PlaySFX,       // 播放音效（参数：音效名称）
    PlayBGM,       // 播放背景音乐（参数：音乐名称）
    StopBGM,       // 停止背景音乐
    SetSFXVolume,  // 设置音效音量（参数：0-1）
    SetBGMVolume,  // 设置音乐音量（参数：0-1）
}
