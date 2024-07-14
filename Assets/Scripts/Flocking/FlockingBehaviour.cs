using AudioAnalysis;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;

namespace Flocking {
    public abstract class FlockingBehaviour : MonoBehaviour {
        // avoiding hard code errors
        #region Constants for Declarations

        protected const int threadGroupSize = 128;
        protected const string Smoothness = "_Smoothness";
        protected const string BoidBodies = "boid_bodies";
        
        // Audio reactive constants
        private const string AmplitudeAudioBuffer = "amplitudeBuffer";
        private const string SpeedValueMin = "speedValueMin";
        private const string SpeedValueMax = "speedValueMax";
        private const string UseAudioBasedSpeed = "useAudioBasedSpeed";
        
        // Boid Behaviour constants
        private const string Speed = "speed";
        private const string SteeringSpeed = "steeringSpeed";
        private const string NumBoids = "numBoids";
        private const string TargetPointPos = "targetPointPos";
        private const string RepulsionPointPos = "repulsionPointPos";
        private const string NoCompressionArea = "noCompressionArea";
        private const string SeparationFactor = "separationFactor";
        private const string AlignmentFactor = "alignmentFactor";
        private const string CohesionFactor = "cohesionFactor";
        private const string LeadershipWeight = "leadershipWeight";
        
        // Level Behaviour Constants
        private const string LocalArea = "localArea";
        private const string RepulsionArea = "repulsionArea";
        private const string CenterWeight = "centerWeight";
        private const string DeltaTime = "deltaTime";

        #endregion


        #region Variables

        [Header("Audio reactive Settings")]
        [Space]
        [SerializeField] public bool useMaterialSmoothness;
        [SerializeField] protected Vector2 minMaxValueSmoothness;
        [Range(0f,1f)] [SerializeField] protected float smoothnessThreshold;
        [SerializeField] protected Material material;
        [Space]
        [SerializeField] private bool useAudioBasedSpeed;
        [SerializeField] public float speedValueMin;
        [SerializeField] public float speedValueMax;
        [Space]
        [SerializeField] protected bool useScale;
        [SerializeField] protected Vector2 minMaxValueScale;
        [Space]
        [SerializeField] public AudioData audioData;
        
        
        private Vector3 targetPointPos;
        private Vector3 repulsionPointPos;
        protected NativeArray<RaycastCommand> rayCommands;
        protected NativeArray<RaycastHit> rayHits;
        protected JobHandle jobHandle;
        protected Transform[] boidsArray;

        [Header("Boid Settings")]
        [SerializeField] protected GameObject boidPrefab;
        [SerializeField] protected int amountOfBoids = 500;
        [SerializeField] protected float spawningField = 200;
        [Space]
        [SerializeField] protected float speed = 10f;
        [SerializeField] protected float steeringSpeed = 2f;
        [SerializeField] protected float separationFactor = 0.6f;
        [SerializeField] protected float alignmentFactor = 0.3f;
        [SerializeField] protected float cohesionFactor = 0.1f;
        [Space]
        [SerializeField] protected float localArea = 40f;
        [SerializeField] protected float noCompressionArea = 5f;
        [SerializeField] protected float repulsionArea = 5f;
        [SerializeField] protected Transform targetPoint;
        [SerializeField] protected Transform repulsionPoint;
        [SerializeField] protected float leadershipWeight = 0.01f;
        [SerializeField] protected float centerWeight = 0.0001f;

        [HideInInspector] [SerializeField] protected ComputeShader compute;
        [HideInInspector] public int audioBand;
        

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

            //Boids initialisaton, adds all boids to an array.
            boidsArray = new Transform[amountOfBoids];
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
            compute.SetFloat(DeltaTime, Time.deltaTime * 3);
            compute.SetFloat(AmplitudeAudioBuffer, audioData.amplitudeBuffer);
            compute.SetFloat(SpeedValueMin, speedValueMin);
            compute.SetFloat(SpeedValueMax, speedValueMax);
            compute.SetFloat(Speed, speed);
        }

        /// <summary>
        /// Sets initial compute parameters which only need to be set on initialize
        /// </summary>
        protected virtual void InitialiseBuffer() {
            compute.SetInt(NumBoids, amountOfBoids);
            compute.SetFloat(LocalArea, localArea);
            compute.SetFloat(NoCompressionArea, noCompressionArea);
            compute.SetFloat(RepulsionArea, repulsionArea);
            compute.SetFloat(SeparationFactor, separationFactor);
            compute.SetFloat(AlignmentFactor, alignmentFactor);
            compute.SetFloat(CohesionFactor, cohesionFactor);
            compute.SetFloat(LeadershipWeight, leadershipWeight);
            compute.SetFloat(CenterWeight, centerWeight);
            compute.SetBool(UseAudioBasedSpeed, useAudioBasedSpeed);
            compute.SetFloat(SteeringSpeed, steeringSpeed);
        }

        protected void SpawnBoid(int index) {
            var boidInstance = Instantiate(boidPrefab, transform);
            boidInstance.transform.localPosition = new Vector3(Random.Range(-spawningField, spawningField),
                Random.Range(-spawningField, spawningField), Random.Range(-spawningField, spawningField));

            boidsArray[index] = boidInstance.transform;
        }

        protected virtual void OnDrawGizmosSelected() {
            Gizmos.color = new Color(1, 0, 0, 0.5f);
            Gizmos.DrawWireCube(transform.position, new Vector3(spawningField * 2, spawningField * 2, spawningField * 2));

            if (debugPositions.Length == amountOfBoids) {
                for (int i = 0; i < amountOfBoids; i++) {
                    Gizmos.DrawWireSphere
                    (
                        new Vector3(transform.position.x + debugPositions[i].x,
                            transform.position.y + debugPositions[i].y, transform.position.z + debugPositions[i].z),
                        0.5f
                    );
                }
            }

            if (amountOfBoids == 0) return;
            var regeneratePositions = debugPositions.Length != amountOfBoids || lastSpawnRange != spawningField;
            if (!regeneratePositions && debugPositions[0].x != 0 && debugPositions[0].y != 0 &&
                debugPositions[0].z != 0) return;

            debugPositions = new Vector3[amountOfBoids];
            lastSpawnRange = spawningField;
            for (int i = 0; i < amountOfBoids; i++) {
                debugPositions[i] = new Vector3(
                    Random.Range(debugPositions[i].x - spawningField, debugPositions[i].x + spawningField),
                    Random.Range(debugPositions[i].y - spawningField, debugPositions[i].y + spawningField),
                    Random.Range(debugPositions[i].z - spawningField, debugPositions[i].z + spawningField));
            }
        }
    }
}