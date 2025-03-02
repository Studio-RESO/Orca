namespace Orca
{
    public interface IAudioPlayback
    {
        bool IsPlaying { get; }
        void Play();
        void Pause();
        void Stop();
        void SetVolume(float volume);
        void SetPitch(float pitch);
    }
}
