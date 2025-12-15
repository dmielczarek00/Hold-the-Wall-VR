using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioPlay
{
    private const float DefaultPitchMin = 0.95f;
    private const float DefaultPitchMax = 1.05f;


    public static void PlaySound(AudioSource source, AudioClip[] clips)
    {
        if (source == null) return;
        if (clips == null || clips.Length == 0) return;

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null) return;

        PlayInternal(source, clip, DefaultPitchMin, DefaultPitchMax);
    }

    public static void PlaySound(AudioSource source, List<AudioClip> clips)
    {
        if (clips == null || clips.Count == 0) return;
        PlaySound(source, clips[Random.Range(0, clips.Count)]);
    }

    public static void PlaySound(AudioSource source, AudioClip clip)
    {
        if (source == null) return;
        if (clip == null) return;

        PlayInternal(source, clip, DefaultPitchMin, DefaultPitchMax);
    }

    private static void PlayInternal(
        AudioSource source,
        AudioClip clip,
        float pitchMin,
        float pitchMax)
    {
        float originalPitch = source.pitch;
        source.pitch = Random.Range(pitchMin, pitchMax);

        source.PlayOneShot(clip);

        source.pitch = originalPitch;
    }
}
