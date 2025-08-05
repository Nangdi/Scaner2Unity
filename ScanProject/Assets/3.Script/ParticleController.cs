using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    public ParticleSystem[] particle;




    public void playParticle(int num)
    {
        particle[num].Play();
    }
}
