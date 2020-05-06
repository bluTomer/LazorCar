using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager RT { get; private set; }

    [SerializeField] public AudioSource[] Sounds;

    private void Awake()
    {
        RT = this;
    }

    public void PlaySound(int index)
    {
        if (index > 0 && index < Sounds.Length)
        {
            Sounds[index].Play();
        }
    }
}
