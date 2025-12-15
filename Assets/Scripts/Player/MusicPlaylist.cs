using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlaylist : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] playlist;

    private int _currentIndex;

    void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (playlist == null || playlist.Length == 0)
            return;

        _currentIndex = 0;
        PlayCurrent();
    }

    void Update()
    {
        if (!audioSource.isPlaying && audioSource.clip != null)
        {
            PlayNext();
        }
    }

    private void PlayCurrent()
    {
        audioSource.clip = playlist[_currentIndex];
        audioSource.Play();
    }

    private void PlayNext()
    {
        _currentIndex = (_currentIndex + 1) % playlist.Length;
        PlayCurrent();
    }
}