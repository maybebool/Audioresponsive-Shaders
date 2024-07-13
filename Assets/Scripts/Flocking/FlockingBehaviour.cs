using AudioAnalysis;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Flocking {
    public abstract class FlockingBehaviour : MonoBehaviour {
        #region Constants for Declarations

        // avoiding hard code errors
        private const string TargetPointPos = "targetPointPos";
        private const string RepulsionPointPos = "repulsionPointPos";
        private const string DeltaTime = "DeltaTime";
        private const string NumBoids = "numBoids";
        private const string LocalArea = "LocalArea";
        private const string NoClumpingArea = "NoClumpingArea";
        private const string RepulsionArea = "RepulsionArea";
        private const string SeparationMultiplier = "separationMultiplier";
        private const string AlignmentMultiplier = "alignmentMultiplier";
        private const string CohesionMultiplier = "cohesionMultiplier";
        private const string LeadershipMultiplier = "leadershipMultiplier";
        private const string TargetPointMultiplier = "targetPointMultiplier";
        private const string AmplitudeAudioBuffer = "amplitudeBuffer";
        private const string SpeedValueMin = "speedValueMin";
        private const string SpeedValueMax = "speedValueMax";
        private const string UseAudioBasedSpeed = "useAudioBasedSpeed";
        private const string UseColorAudio = "useColorAudio";
        protected const string Smoothness = "_Smoothness";
        // private const string Color1r = "color1r";
        // private const string Color1g = "color1g";
        // private const string Color1b = "color1b";
        // private const string Color2r = "color2r";
        // private const string Color2g = "color2g";
        // private const string Color2b = "color2b";
        // private const string ComputeColorR = "computeColorR";
        // private const string ComputeColorG = "computeColorG";
        // private const string ComputeColorB = "computeColorB";
        private const string ColorMultiplier = "colorMultiplier";
        private const string Speed = "Speed";
        private const string SteeringSpeed = "steeringSpeed";

        #endregion


        #region Variables

        [SerializeField] public AudioData audioData;
        [HideInInspector] public int audioBand;
        [SerializeField] public bool useColorAudio;
        [SerializeField] protected float colorMultiplier;
        [SerializeField] protected float colorMultiplier2;
        //[HideInInspector] public float color1r,color1g,color1b, color2r,color2g, color2b,computeColorR,computeColorG,computeColorB;
        [SerializeField] protected Color color1;
        [SerializeField] protected Color color2;

        protected const int threadGroupSize = 128;
        [HideInInspector] [SerializeField] protected Material defaultMat;
        [HideInInspector] [SerializeField] protected ComputeShader compute;
        protected Vector3 targetPointPos;
        protected Vector3 repulsionPointPos;
        protected NativeArray<RaycastCommand> rayCommands;
        protected NativeArray<RaycastHit> rayHits;
        protected JobHandle jobHandle;
        protected Transform[] boidsArray;

        [Header("Boid Preset")] 
        [SerializeField] protected GameObject boidPrefab;
        // [SerializeField] private Vector2 scaleValueMinMax;
        //[SerializeField] private bool useScale;
        [SerializeField] private bool useAudioBasedSpeed;
        [SerializeField] public float speedValueMin;
        [SerializeField] public float speedValueMax;

        [SerializeField] protected bool useDefaultMaterial = true;

        [Header("Spawning")] [SerializeField] protected int spawnBoids = 2048;
        [SerializeField] protected float spawnRange = 50;

        [Header("Movement")] 
        [SerializeField] protected float speed = 20f;
        [SerializeField] protected float steeringSpeed = 2f;
        [SerializeField] protected float localArea = 40f;
        [SerializeField] protected float noClumpingArea = 5f;
        [SerializeField] protected float repulsionArea = 5f;
        [SerializeField] protected Transform targetPoint;
        [SerializeField] protected Transform repulsionPoint;

        [Header("Behaviours")] 
        [SerializeField] protected float separationMultiplier = 0.6f;
        [SerializeField] protected float alignmentMultiplier = 0.3f;
        [SerializeField] protected float cohesionMultiplier = 0.1f;
        [SerializeField] protected float leadershipMultiplier = 0.01f;
        [SerializeField] protected float targetPointMultiplier = 0.001f;

        [Header("Misc")] 
        [Range(0f, 100f)] [SerializeField] protected float simulationSpeed = 1f;

        #endregion

        protected enum RaycastType {
            Synchronous,
            Asynchronous,
            None
        }

        //Gizmos Stuff
        protected float lastSpawnRange;
        protected Vector3[] debugPositions = new Vector3[0];

        /// <summary>
        /// Abstract property. Gets the size which our compute buffer needs to be to support supplied data
        /// </summary>
        protected abstract int ComputeBufferSize { get; }

        protected abstract void InitializeBoidData();

        protected virtual void Awake() {
            // if (useDefaultMaterial == true) {
            //     boidPrefab.GetComponent<Renderer>().sharedMaterial = GetDefaultMaterial();
            // }

            //Boids initialisaton, adds all boids to an array.
            boidsArray = new Transform[spawnBoids];
            rayCommands = new NativeArray<RaycastCommand>(boidsArray.Length, Allocator.Persistent);
            rayHits = new NativeArray<RaycastHit>(boidsArray.Length, Allocator.Persistent);

            //Initialises boids buffer to send to compute shader
            InitialiseBuffer();

            //Spawns all boids
            for (int i = 0; i < boidsArray.Length; i++) {
                SpawnBoid(i);
            }

            // Call InitializeBoidData overrides in inheriting classes
            InitializeBoidData();
        }

        protected void OnValidate() {
            InitialiseBuffer();
        }

        protected virtual void OnDestroy() {
            //Garbage collection.
            jobHandle.Complete();
            rayCommands.Dispose();
            rayHits.Dispose();
        }

        /// <summary>
        /// Call to validate target and repulsion points
        /// </summary>
        protected void ValidateTargetAndRepulsionPoints() {
            if (targetPoint == null) targetPointPos = transform.position;
            else targetPointPos = targetPoint.position;
            if (repulsionPoint == null) repulsionPointPos = transform.position;
            else repulsionPointPos = repulsionPoint.position;
        }

        /// <summary>
        /// Sets compute parameters which need to be set every Update() call
        /// </summary>
        protected void SetComputeParameters() {
            compute.SetVector(TargetPointPos, targetPointPos);
            compute.SetVector(RepulsionPointPos, repulsionPointPos);
            compute.SetFloat(DeltaTime, Time.deltaTime * simulationSpeed);
            compute.SetFloat(AmplitudeAudioBuffer, audioData.amplitudeBuffer);
            compute.SetFloat(SpeedValueMin, speedValueMin);
            compute.SetFloat(SpeedValueMax, speedValueMax);
            compute.SetFloat(Speed, speed);
            compute.SetFloat(ColorMultiplier, colorMultiplier);
            // compute.SetFloat(Color1r,color1r);
            // compute.SetFloat(Color1g,color1g);
            // compute.SetFloat(Color1b,color1b);
            // compute.SetFloat(Color2r,color2r);
            // compute.SetFloat(Color2g,color2g);
            // compute.SetFloat(Color2b,color2b);
            // compute.SetFloat(ComputeColorR, computeColorR);
            // compute.SetFloat(ComputeColorG, computeColorG);
            // compute.SetFloat(ComputeColorB, computeColorB);
        }

        /// <summary>
        /// Sets initial compute parameters which only need to be set on initialize
        /// </summary>
        protected virtual void InitialiseBuffer() {
            compute.SetInt(NumBoids, spawnBoids);
            compute.SetFloat(LocalArea, localArea);
            compute.SetFloat(NoClumpingArea, noClumpingArea);
            compute.SetFloat(RepulsionArea, repulsionArea);
            compute.SetFloat(SeparationMultiplier, separationMultiplier);
            compute.SetFloat(AlignmentMultiplier, alignmentMultiplier);
            compute.SetFloat(CohesionMultiplier, cohesionMultiplier);
            compute.SetFloat(LeadershipMultiplier, leadershipMultiplier);
            compute.SetFloat(TargetPointMultiplier, targetPointMultiplier);
            compute.SetBool(UseAudioBasedSpeed, useAudioBasedSpeed);
            compute.SetBool(UseColorAudio, useColorAudio);
            compute.SetFloat(SteeringSpeed, steeringSpeed);
        }

        protected void SpawnBoid(int index) {
            var boidInstance = Instantiate(boidPrefab, transform);
            boidInstance.transform.localPosition = new Vector3(Random.Range(-spawnRange, spawnRange),
                Random.Range(-spawnRange, spawnRange), Random.Range(-spawnRange, spawnRange));

            boidsArray[index] = boidInstance.transform;
        }

        protected virtual void OnDrawGizmosSelected() {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawWireCube(transform.position, new Vector3(spawnRange * 2, spawnRange * 2, spawnRange * 2));

            if (debugPositions.Length == spawnBoids) {
                for (int i = 0; i < spawnBoids; i++) {
                    Gizmos.DrawWireSphere
                    (
                        new Vector3(transform.position.x + debugPositions[i].x,
                            transform.position.y + debugPositions[i].y, transform.position.z + debugPositions[i].z),
                        0.5f
                    );
                }
            }

            if (spawnBoids == 0) return;
            var regeneratePositions = debugPositions.Length != spawnBoids || lastSpawnRange != spawnRange;
            if (!regeneratePositions && debugPositions[0].x != 0 && debugPositions[0].y != 0 &&
                debugPositions[0].z != 0) return;

            debugPositions = new Vector3[spawnBoids];
            lastSpawnRange = spawnRange;
            for (int i = 0; i < spawnBoids; i++) {
                debugPositions[i] = new Vector3(
                    Random.Range(debugPositions[i].x - spawnRange, debugPositions[i].x + spawnRange),
                    Random.Range(debugPositions[i].y - spawnRange, debugPositions[i].y + spawnRange),
                    Random.Range(debugPositions[i].z - spawnRange, debugPositions[i].z + spawnRange));
            }
        }
    }
}