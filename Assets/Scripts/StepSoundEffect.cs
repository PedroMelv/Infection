using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSoundEffect : MonoBehaviourPun
{
    [SerializeField] private AudioClip[] stepClips;
    
    [Space]

    [SerializeField] private float timeToStep;
    [SerializeField, Range(1,8)] private int stepAmount;
    [Space]
    [SerializeField,Range(0f,1f)] private float volume;
    [SerializeField,Range(0f,1f)] private float spatialBlend;
    [SerializeField] private float maxDistance;

    private float curStepTimer;

    public bool playStepTimer = false;
    public float playSpeed = 1f;

    private void Update()
    {
        if (playStepTimer == false) return;

        if(curStepTimer >= timeToStep / stepAmount)
        {
            PlayStepSound();
            curStepTimer= 0;
        }
        else
        {
            curStepTimer += Time.deltaTime * playSpeed;
        }
    }

    private void PlayStepSound()
    {
        photonView.RPC(nameof(RPC_PlayStepSound), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayStepSound()
    {
        AudioClip stepClip = stepClips[Random.Range(0, stepClips.Length)];

        GameObject stepSound = new GameObject(stepClip.name);

        stepSound.transform.position = transform.position - Vector3.down;

        AudioSource source = stepSound.AddComponent<AudioSource>();

        source.clip = stepClip;
        source.volume = volume;

        source.maxDistance = maxDistance;
        source.spatialBlend = spatialBlend;

        source.Play();

        Destroy(stepSound, stepClip.length + .1f);
    }
}
