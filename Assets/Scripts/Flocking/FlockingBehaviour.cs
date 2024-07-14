using AudioAnalysis;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Flocking {
    public abstract class FlockingBehaviour : MonoBehaviour {
        
        // avoiding hard code errors
        #region Constants for Declarations

        protected const int ThreadGroupSize = 128;
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
        [SerializeField] public bool useAudioBasedSpeed;
        [SerializeField] protected float speedValueMin;
        [SerializeField] protected float speedValueMax;
        
        [Space]
        [SerializeField] public bool useScale;
        [SerializeField] protected Vector2 minMaxValueScale;
        
        [Space]
        [SerializeField] protected AudioData audioData;
        
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
        
        [Header("Boid Collision Behaviour")]
        [SerializeField] protected LayerMask terrainMask;
        [SerializeField] protected RaycastType raycastType = RaycastType.Synchronous;
        [SerializeField] protected float collisionAdjustment = 50f;
        [SerializeField] protected float raycastDistance = 100f;

        protected int audioBand;
        protected Transform[] boidsArray;
        [HideInInspector] [SerializeField] protected ComputeShader compute;
        
        private Vector3 _targetPointPos;
        private Vector3 _repulsionPointPos;
        private NativeArray<RaycastHit> _rayHits;

        #endregion

        protected enum RaycastType {
            Synchronous,
            Asynchronous,
            None
        }
        
        protected abstract void InitializeBoidData();

        protected virtual void Awake() {
            boidsArray = new Transform[amountOfBoids];
            _rayHits = new NativeArray<RaycastHit>(boidsArray.Length, Allocator.Persistent);
            InitialiseBuffer();
            
            for (int i = 0; i < boidsArray.Length; i++) {
                SpawnBoid(i);
            }
            
            InitializeBoidData();
        }

        protected void OnValidate() {
            InitialiseBuffer();
        }

        protected virtual void OnDestroy() {
            _rayHits.Dispose();
        }

        
        
        protected void CheckForTargetAndRepulsionPointBehaviour() {
            _targetPointPos = targetPoint == null ? transform.position : targetPoint.position;
            _repulsionPointPos = repulsionPoint == null ? transform.position : repulsionPoint.position;
        }

        
        protected void SetComputeParameters() {
            compute.SetVector(TargetPointPos, _targetPointPos);
            compute.SetVector(RepulsionPointPos, _repulsionPointPos);
            compute.SetFloat(DeltaTime, Time.deltaTime * 3);
            compute.SetFloat(AmplitudeAudioBuffer, audioData.amplitudeBuffer);
            compute.SetFloat(SpeedValueMin, speedValueMin);
            compute.SetFloat(SpeedValueMax, speedValueMax);
            compute.SetFloat(Speed, speed);
            compute.SetBool(UseAudioBasedSpeed, useAudioBasedSpeed);
        }


        private void InitialiseBuffer() {
            compute.SetInt(NumBoids, amountOfBoids);
            compute.SetFloat(LocalArea, localArea);
            compute.SetFloat(NoCompressionArea, noCompressionArea);
            compute.SetFloat(RepulsionArea, repulsionArea);
            compute.SetFloat(SeparationFactor, separationFactor);
            compute.SetFloat(AlignmentFactor, alignmentFactor);
            compute.SetFloat(CohesionFactor, cohesionFactor);
            compute.SetFloat(LeadershipWeight, leadershipWeight);
            compute.SetFloat(CenterWeight, centerWeight);
            compute.SetFloat(SteeringSpeed, steeringSpeed);
        }

        private void SpawnBoid(int index) {
            var boidInstance = Instantiate(boidPrefab, transform);
            boidInstance.transform.localPosition = new Vector3(Random.Range(-spawningField, spawningField),
                Random.Range(-spawningField, spawningField), Random.Range(-spawningField, spawningField));

            boidsArray[index] = boidInstance.transform;
        }
    }
}