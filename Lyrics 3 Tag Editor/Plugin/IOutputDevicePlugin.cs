using NAudio.Wave;

namespace Lyrics_3_Tag_Editor.Plugin
{
    public interface IOutputDevicePlugin
    {
        IWavePlayer Create(int latency);
    }
}
