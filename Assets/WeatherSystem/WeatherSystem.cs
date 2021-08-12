using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherSystem : MonoBehaviour
{
    [Header("Weather Objects")]
    public GameObject RainEffectPrefab;
    public GameObject SnowEffectPrefab;
    public GameObject LightningPanel;
    public GameObject GlobalLighting;
 

    [Header("Sound")]
    public GameObject WeatherSystemSound;
    

    [Header("Sound Effects")]
    public AudioClip Sunny, Raining, ThunderStorm, Lightning, StrongWinds;
    public enum States { Sunny = 0, Overcast = 1, Rainy = 2, ThunderStorm = 3, Snow = 4 }

    [Header("Current State")]
    public States State = States.Sunny;
    public bool IsTransitioning;

    [Header("Weather Sequencer")]
    public States[] WeatherSequence;

    [Header("Weather Controller")]
    public float StateTransitioningDuration, WeatherTimeSpeed;
    [Range(1, 2000)]
    public int MaxRainParticles, MaxSnowParticles, NoOfParticles;


    [Header("State Lighting Settings")]
    [Range(0.001f, 1.0f)]
    public float SunnyStateLighting;
    [Range(0.001f, 1.0f)]
    public float OvercastStateLighting;
    [Range(0.001f, 1.0f)]
    public float RainyStateLighting;
    [Range(0.001f, 1.0f)]
    public float ThunderStormStateLighting;
    [Range(0.001f, 1.0f)]
    public float SnowStateLighting;
    public float LightningDelay;


    private bool isRaining, isStorming, isSnowing, Transitioning, isLightning;
    private int DesiredRainParticles, WeatherSequnceState, lightningFlash;
    private float WeatherMaxTime,TransitioningTime, DesiredLighting, CurrentStateLighting, lightningTime;

   

    //Settings particles to 0 and adjusting max light and initiating audio.
    void Start()
    {
        NoOfParticles = 0;
        CurrentStateLighting = 1;
        WeatherSystemSound.GetComponent<AudioSource>().clip = Sunny;
        WeatherSystemSound.GetComponent<AudioSource>().Play();
    }

    // 
    void Update()
    {
        // When in state increase and update noofparticles and initiate the particle effect prefab.
        if (isRaining)
        {
          
            if (NoOfParticles < DesiredRainParticles)
            {
                NoOfParticles += 1;
                ParticleSystem particlesystem = RainEffectPrefab.GetComponent<ParticleSystem>();
                var particle = particlesystem.main;
                particle.maxParticles = NoOfParticles;
                //AudioSourceObject.GetComponent<AudioSource>().clip = Raining;
                //AudioSourceObject.GetComponent<AudioSource>().Play();
            }
        }

        //When in state sets the lightning time and resets it according to what lightningTime is defined in inspector and plays the lighning effect on screen
        if (isStorming)
        {
            lightningTime += Time.deltaTime;
            if (lightningTime >= LightningDelay)
            {
                LightningStruck();
                lightningTime = 0;
            }
        }
        // when in both states sets the lightning panel on and off
        if (isStorming && isLightning)
        {

            lightningFlash++;
            if (lightningFlash > 250)
            {
                LightningPanel.SetActive(true);
            }
            if (lightningFlash > 270)
            {
                LightningPanel.SetActive(false);
                isLightning = false;
            }
        }

        //When in state updates the particles and initiates the prefabs
        if (isSnowing)
        {
            if (NoOfParticles < MaxSnowParticles)
            {
                NoOfParticles += 1;
                ParticleSystem ps = SnowEffectPrefab.GetComponent<ParticleSystem>();
                var main = ps.main;
                main.maxParticles = NoOfParticles;

                //AudioSourceObject.GetComponent<AudioSource>().clip = StrongWinds;
                //AudioSourceObject.GetComponent<AudioSource>().Play();
            }

        }
        //When in state resets the tranisition settings and sets up other state settings which are not active.
        if (Transitioning)
        {
            TransitioningTime += Time.deltaTime;
            if (TransitioningTime > StateTransitioningDuration)
            {
                TransitioningTime = 0;
                Transitioning = false;
                IsTransitioning = false;
                CurrentStateLighting = DesiredLighting;
            }

            if (!isRaining && !isSnowing)
            {
                if (NoOfParticles > 0)
                { NoOfParticles -= 200; }
                ParticleSystem ps = RainEffectPrefab.GetComponent<ParticleSystem>();
                var main = ps.main;
                main.maxParticles = NoOfParticles;

                ps = SnowEffectPrefab.GetComponent<ParticleSystem>();
                main = ps.main;
                main.maxParticles = NoOfParticles;


            }

            //Helps slow down the transitioning effect between states.
            if (DesiredLighting > CurrentStateLighting)
            {
                CurrentStateLighting += 0.001f;
            }
            if (DesiredLighting < CurrentStateLighting)
            {
                CurrentStateLighting -= 0.001f;
            }

            GlobalLighting.GetComponent<UnityEngine.Experimental.Rendering.Universal.Light2D>().intensity = CurrentStateLighting;
        }

        if (!Transitioning)
        {
            WeatherMaxTime += Time.deltaTime;
            if(WeatherMaxTime > WeatherTimeSpeed)
            {
                WeatherMaxTime = 0;
                StartTransition();
                IsTransitioning = true;
            }
    
        }
               
    }

    //Function to start the tranitions and switch the states accordingly.
    private void StartTransition()
    {
        Transitioning = true;
        if(WeatherSequnceState == WeatherSequence.Length -1)
        {
            WeatherSequnceState = 0;
        }
        else
        {
            WeatherSequnceState += 1;
        }
        
        State = WeatherSequence[WeatherSequnceState];

        switch (State)
        {
            case States.Sunny:
                DesiredLighting = SunnyStateLighting;
                isRaining = false;
                isSnowing = false;
                DesiredRainParticles = 0;
                WeatherSystemSound.GetComponent<AudioSource>().clip = Sunny;
                WeatherSystemSound.GetComponent<AudioSource>().Play();
                isStorming = false;
                RainEffectPrefab.SetActive(false);
                SnowEffectPrefab.SetActive(false);
                break;
            case States.Overcast:
                DesiredLighting = OvercastStateLighting;
                isRaining = false;
                isSnowing = false;
                WeatherSystemSound.GetComponent<AudioSource>().Pause();
                isStorming = false;
                break;
            case States.Rainy:
                DesiredLighting = RainyStateLighting;
                isRaining = true;
                DesiredRainParticles = MaxRainParticles / 2;
                isSnowing = false;
                NoOfParticles = 0;
                RainEffectPrefab.SetActive(true);
                SnowEffectPrefab.SetActive(false);
                WeatherSystemSound.GetComponent<AudioSource>().clip = Raining;
                WeatherSystemSound.GetComponent<AudioSource>().Play();
                isStorming = false;
                break;
            case States.ThunderStorm:
                isRaining = true;
                DesiredRainParticles = MaxRainParticles;
                isSnowing = false;
                NoOfParticles = 0;
                RainEffectPrefab.SetActive(true);
                SnowEffectPrefab.SetActive(true);
                WeatherSystemSound.GetComponent<AudioSource>().clip = ThunderStorm;
                WeatherSystemSound.GetComponent<AudioSource>().Play();
                isStorming = true;
                break;
            case States.Snow:
                DesiredLighting = SnowStateLighting;
                isRaining = false;
                isSnowing = true;
                NoOfParticles = 0;
                SnowEffectPrefab.SetActive(true);
                RainEffectPrefab.SetActive(false);
                WeatherSystemSound.GetComponent<AudioSource>().clip = StrongWinds;
                WeatherSystemSound.GetComponent<AudioSource>().Play();
                isStorming = false;
                break;
            default:
                break;
        }

    }

   //function to simulate lightning during storm state.
    private void LightningStruck()
    {
        WeatherSystemSound.GetComponent<AudioSource>().PlayOneShot(Lightning);
        lightningFlash = 0;
        isLightning = true;
    }
}
