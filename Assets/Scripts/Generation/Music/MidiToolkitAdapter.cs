using UnityEngine;
using MidiPlayerTK;
using MidiGenPlay;

public class MidiToolkitAdapter : MonoBehaviour, IPlayMidi
{
    MidiFilePlayer _player;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (GetComponentInChildren<MidiFilePlayer>(true) == null)
        {
            Debug.LogError(
                $"{name} requires a child with MidiFilePlayer; please add one.",
                this
            );
        }
    }
#endif

    void Awake()
    {
        // look for the player anywhere in our children
        _player = GetComponentInChildren<MidiFilePlayer>();
        if (_player == null)
            Debug.LogError(
                $"[{name}] needs a child GameObject with MidiFilePlayer on it.",
                this
            );
    }

    public void Stop() => _player?.MPTK_Stop();
    public void Play(byte[] data) => _player?.MPTK_Play(data);
}
