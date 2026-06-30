using UnityEngine;

public class TorchFireEffect : MonoBehaviour
{
    [Header("Placement")]
    [SerializeField] private Vector3 localOffset = new Vector3(0f, 2.25f, 0f);
    [SerializeField] private float flameScale = 1f;

    [Header("Flame")]
    [SerializeField] private bool playOnAwake = true;
    [SerializeField] private int maxParticles = 90;
    [SerializeField] private float particleRate = 34f;
    [SerializeField] private Color bottomColor = new Color(1f, 0.92f, 0.35f, 0.9f);
    [SerializeField] private Color middleColor = new Color(1f, 0.38f, 0.04f, 0.75f);
    [SerializeField] private Color topColor = new Color(0.35f, 0.04f, 0.01f, 0f);

    [Header("Light")]
    [SerializeField] private bool createLight = true;
    [SerializeField] private Color lightColor = new Color(1f, 0.55f, 0.2f, 1f);
    [SerializeField] private float lightRange = 5f;
    [SerializeField] private float lightIntensity = 1.3f;
    [SerializeField] private bool flickerLight = true;
    [SerializeField] private float flickerAmount = 0.35f;
    [SerializeField] private float flickerSpeed = 8f;

    private const string FlameObjectName = "Generated_Torch_Flame";
    private const string LightObjectName = "Generated_Torch_Light";

    private ParticleSystem flameParticles;
    private Light flameLight;
    private Material runtimeFlameMaterial;

    private void Awake()
    {
        EnsureEffect();
    }

    private void OnEnable()
    {
        EnsureEffect();
    }

    private void OnValidate()
    {
        flameScale = Mathf.Max(0.1f, flameScale);
        maxParticles = Mathf.Max(5, maxParticles);
        particleRate = Mathf.Max(0f, particleRate);
        lightRange = Mathf.Max(0f, lightRange);
        lightIntensity = Mathf.Max(0f, lightIntensity);
        flickerAmount = Mathf.Max(0f, flickerAmount);
        flickerSpeed = Mathf.Max(0f, flickerSpeed);

        if (Application.isPlaying && isActiveAndEnabled)
        {
            EnsureEffect();
        }
    }

    private void Update()
    {
        if (flameLight == null || !flickerLight)
        {
            return;
        }

        float seed = Mathf.Abs(GetInstanceID()) * 0.013f;
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, seed);
        flameLight.intensity = Mathf.Max(0f, lightIntensity + (noise - 0.5f) * flickerAmount);
    }

    private void EnsureEffect()
    {
        flameParticles = GetOrCreateChildComponent<ParticleSystem>(FlameObjectName);
        ConfigureFlameTransform(flameParticles.transform);
        ConfigureParticleSystem(flameParticles);

        ParticleSystemRenderer renderer = flameParticles.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            ConfigureRenderer(renderer);
        }

        if (createLight)
        {
            flameLight = GetOrCreateChildComponent<Light>(LightObjectName);
            ConfigureFlameTransform(flameLight.transform);
            ConfigureLight(flameLight);
        }
        else
        {
            Transform lightTransform = transform.Find(LightObjectName);
            if (lightTransform != null)
            {
                lightTransform.gameObject.SetActive(false);
            }
        }

        if (playOnAwake && Application.isPlaying && flameParticles != null && !flameParticles.isPlaying)
        {
            flameParticles.Play();
        }
    }

    private T GetOrCreateChildComponent<T>(string childName) where T : Component
    {
        Transform child = transform.Find(childName);
        if (child == null)
        {
            GameObject childObject = new GameObject(childName);
            childObject.transform.SetParent(transform, false);
            child = childObject.transform;
        }

        if (!child.gameObject.activeSelf)
        {
            child.gameObject.SetActive(true);
        }

        T component = child.GetComponent<T>();
        if (component == null)
        {
            component = child.gameObject.AddComponent<T>();
        }

        return component;
    }

    private void ConfigureFlameTransform(Transform flameTransform)
    {
        flameTransform.localPosition = localOffset;
        flameTransform.localRotation = Quaternion.identity;
        flameTransform.localScale = Vector3.one;
    }

    private void ConfigureParticleSystem(ParticleSystem particles)
    {
        bool wasPlaying = particles.isPlaying;
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        ParticleSystem.MainModule main = particles.main;
        main.loop = true;
        main.playOnAwake = playOnAwake;
        main.startDelay = 0f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f * flameScale, 0.75f * flameScale);
        main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f * flameScale, 0.55f * flameScale);
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f * flameScale, 0.42f * flameScale);
        main.startRotation = new ParticleSystem.MinMaxCurve(-0.35f, 0.35f);
        main.gravityModifier = -0.08f;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        main.maxParticles = maxParticles;

        ParticleSystem.EmissionModule emission = particles.emission;
        emission.enabled = true;
        emission.rateOverTime = particleRate;

        ParticleSystem.ShapeModule shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 12f;
        shape.radius = 0.12f * flameScale;
        shape.length = 0.25f * flameScale;

        ParticleSystem.ColorOverLifetimeModule color = particles.colorOverLifetime;
        color.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(bottomColor, 0f),
                new GradientColorKey(middleColor, 0.45f),
                new GradientColorKey(topColor, 1f)
            },
            new[]
            {
                new GradientAlphaKey(bottomColor.a, 0f),
                new GradientAlphaKey(middleColor.a, 0.45f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        color.color = gradient;

        ParticleSystem.SizeOverLifetimeModule size = particles.sizeOverLifetime;
        size.enabled = true;
        AnimationCurve sizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.65f),
            new Keyframe(0.35f, 1f),
            new Keyframe(1f, 0.15f)
        );
        size.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

        ParticleSystem.VelocityOverLifetimeModule velocity = particles.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.Local;
        velocity.y = new ParticleSystem.MinMaxCurve(0.45f * flameScale, 0.9f * flameScale);
        velocity.x = new ParticleSystem.MinMaxCurve(-0.08f * flameScale, 0.08f * flameScale);
        velocity.z = new ParticleSystem.MinMaxCurve(-0.08f * flameScale, 0.08f * flameScale);

        ParticleSystem.NoiseModule noise = particles.noise;
        noise.enabled = true;
        noise.strength = 0.18f * flameScale;
        noise.frequency = 1.4f;
        noise.scrollSpeed = 1.2f;

        if (Application.isPlaying && (playOnAwake || wasPlaying) && !particles.isPlaying)
        {
            particles.Play();
        }
    }

    private void ConfigureRenderer(ParticleSystemRenderer renderer)
    {
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        renderer.sortingFudge = 1f;

        if (runtimeFlameMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Particles/Standard Unlit");
            }

            if (shader != null)
            {
                runtimeFlameMaterial = new Material(shader);
                runtimeFlameMaterial.name = "Runtime_Torch_Flame_Material";
                if (runtimeFlameMaterial.HasProperty("_BaseColor"))
                {
                    runtimeFlameMaterial.SetColor("_BaseColor", Color.white);
                }

                if (runtimeFlameMaterial.HasProperty("_Color"))
                {
                    runtimeFlameMaterial.SetColor("_Color", Color.white);
                }
            }
        }

        if (runtimeFlameMaterial != null)
        {
            renderer.sharedMaterial = runtimeFlameMaterial;
        }
    }

    private void ConfigureLight(Light targetLight)
    {
        targetLight.type = LightType.Point;
        targetLight.color = lightColor;
        targetLight.range = lightRange;
        targetLight.intensity = lightIntensity;
        targetLight.shadows = LightShadows.None;
    }
}
