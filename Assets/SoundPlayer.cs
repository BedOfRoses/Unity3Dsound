using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    // Audio-related variables
    AudioSource audioSource;
    const int buffer = 1024;
    float[] audioData;
    private int currentPosition = 0;

    // Sound properties
    [SerializeField, Header("3D sound")] float maxDistance = 200.0f;
    [SerializeField] public float orbitSpeed = 20f;
    [SerializeField] public Transform player;
    [SerializeField] float Radius = 5.0f;

    // Use Awake to read and load the audio file
    void Awake()
    {
        // Load an audio file from a text file
        string audioFilePath = ReadAudioFilePathFromFile();
        audioSource = GetComponent<AudioSource>();

        // 3D audio settings
        audioSource.spatialize = true;
        audioSource.spatialBlend = 1.0f; // Full 3D spatialization
        audioSource.spatializePostEffects = true;
        audioSource.maxDistance = 400.0f;

        if (!string.IsNullOrEmpty(audioFilePath))
        {
            audioData = LoadAudioData(audioFilePath);
            PlayAudio();
        }
        else
        {
            Debug.LogError("Failed to read audio file path.");
        }
    }

    // Function to play loaded audio data
    void PlayAudio()
    {
        if (audioData != null && audioData.Length > 0)
        {
            audioSource = GetComponent<AudioSource>();
            audioSource.clip = AudioClip.Create("LoadedClip", audioData.Length, 1, 44100, false);
            audioSource.clip.SetData(audioData, 0);
            audioSource.Play(0);
        }
        else
        {
            Debug.LogError("Failed to load audio data.");
        }
    }

    // Function to load audio data from a file
    float[] LoadAudioData(string filePath)
    {
        try
        {
            // Load WAV file 
            byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);
            int headerOffset = 44; // Assuming a standard WAV header size
            float[] audioData = new float[(fileBytes.Length - headerOffset) / 2];

            // Convert audio file bytes to floating-point audio samples
            for (int i = 0; i < audioData.Length; i++)
            {
                // Convert bytes to short and normalize to floating-point audio samples
                audioData[i] = (short) (fileBytes[headerOffset + i * 2] | (fileBytes[headerOffset + i * 2 + 1] << 8)) /
                               32768.0f; // Refer to footnotes Part1
            }

            return audioData;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to load audio data: {ex.Message}");
            return null;
        }
    }

    // Function to read the audio file path from a file
    string ReadAudioFilePathFromFile()
    {
        // Read and construct the audio file path
        string p1 = Application.dataPath;
        p1 = p1.Replace("/Assets", "/Assets/Audios/myfile.wav");
        Debug.Log(p1.ToString());
        return p1.ToString();
    }

    // LateUpdate function to adjust audio properties based on player's distance from the sound source
    private void LateUpdate()
    {
        RotateAround();
        Vector3 playerPosition = Camera.main.transform.position; // Player position
        Vector3 soundPosition = transform.position; // Source of audio (rotating cube)
        float distance = Vector3.Distance(playerPosition, soundPosition);
        audioSource.volume = Mathf.Clamp01(1.0f - distance / maxDistance); // Adjust volume based on distance
    }

    // Function to orbit the sound source around the player
    void RotateAround()
    {
        float angle = Time.time * orbitSpeed;
        Vector3 offset = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle)) * Radius;
        Vector3 newPosition = player.transform.position + offset;
        transform.position = newPosition;
        transform.LookAt(player); // Make the sound source face the player while rotating
        Debug.Log("New Position: " + newPosition);
    }
}