using System.Collections.Generic;
using AudioAnalysis;
using UnityEngine;
using UnityEngine.Serialization;


namespace Flocking {
    public class BoidsCarrier : FlockingBehaviour {
        
        private BoidConductValues[] _boidValues;
        //public AudioData audioData;

        [Header("Collisions")]
        [SerializeField] protected LayerMask terrainMask;
        [SerializeField] private Vector2 minMaxValueScale;
        [SerializeField] private Vector2 minMaxValueSmothness;
        [SerializeField] protected float raycastDistance = 100f;
        [SerializeField] protected float collisionAdjustment = 50f;
        [SerializeField] protected RaycastType raycastType = RaycastType.Synchronous;
        [SerializeField] private bool useScale;
        [SerializeField] private string stringName;
        
        public Material _material;
        private Material[] _audioMaterial;
        
        
        
        public string _colorName1;
        private Color[] _color1;
        
        [Range(0f,1f)] public float smoothnessThreshold;
        public float _colorMultiplier1;
        

        protected override int ComputeBufferSize => BoidConductValues.Size;

        protected override void InitializeBoidData() {
            // Initialize boid data for overriding class
            _boidValues = new BoidConductValues[boidsArray.Length];

            for (int i = 0; i < boidsArray.Length; i++) {
                _boidValues[i].position = boidsArray[i].transform.position;
                _boidValues[i].forward = boidsArray[i].transform.right;
            }
        }
        
        private void Start() {
           // _material.SetColor(_colorName1, Color.green);
            // _audioMaterial = new Material[8];
            // _color1 = new Color[8];
            // for (int i = 0; i < 8; i++)
            // {
            //     //_color1[i] = _gradient1.((1f / 8f) * i);
            //     _audioMaterial[i] = new Material(_material);
            // }
            var countBand = 0;
            for (int i = 0; i < boidsArray.Length; i++)
            {
                var band = countBand % 8;
               audioBand = band;
                countBand++;
            }
        }

        private void Update() {

            ValidateTargetAndRepulsionPoints();

            var boidBuffer = new ComputeBuffer(boidsArray.Length, ComputeBufferSize);
            boidBuffer.SetData(_boidValues);
            compute.SetBuffer(0, "boids", boidBuffer);

            SetComputeParameters();

            var threadGroups = Mathf.CeilToInt(spawnBoids / (float)threadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(_boidValues);
            boidBuffer.Release();
            jobHandle.Complete();

            for (int i = 0; i < _boidValues.Length; i++) {
                var boidTransform = boidsArray[i].transform;
                var tempPos = boidTransform.position;
                var tempFwd = boidTransform.forward;

                RaycastTypeCheck(tempPos, tempFwd, i);


                boidTransform.position = _boidValues[i].position;
                boidTransform.rotation = Quaternion.LookRotation(_boidValues[i].forward);


                if (!useScale) continue;
                var scale = Mathf.Lerp(minMaxValueScale.x, minMaxValueScale.y,
                    audioData.audioBandBuffer[audioBand]);
                boidsArray[i].localScale = new Vector3(scale, scale, scale);
            }


            if (useColorAudio) {
                if (audioData.amplitudeBuffer > smoothnessThreshold) {
                    var lerpValue = Mathf.Lerp(minMaxValueSmothness.x, minMaxValueSmothness.y,
                        audioData.amplitudeBuffer);
                    _material.SetFloat(Smoothness, 1 - lerpValue);
                }
            }
            else {
                _material.SetFloat(Smoothness,0);
            }

            
            //Schedule raycast commands for next update tick.
            jobHandle = RaycastCommand.ScheduleBatch(rayCommands, rayHits, 1);
        }

        private void RaycastTypeCheck(Vector3 tempPos, Vector3 tempFwd, int i) {
            var didHit = false;
            RaycastHit raycastHit = default;
            switch (raycastType) {
                case RaycastType.Synchronous:
                    Physics.Raycast(tempPos, tempFwd, out raycastHit, raycastDistance, terrainMask);
                    didHit = raycastHit.collider != null;
                    break;
                case RaycastType.Asynchronous:
                    rayCommands[i] = new RaycastCommand(tempPos, tempFwd, raycastDistance, terrainMask, 1);
                    raycastHit = rayHits[i];
                    didHit = raycastHit.collider != null;
                    break;
                case RaycastType.None:
                    break;
            }
            if (didHit) {
                var avoidanceFactor = 1 - (Vector3.Distance(tempPos, raycastHit.point) / raycastDistance);
                var raySteering = (raycastHit.point + raycastHit.normal - tempPos).normalized;
                _boidValues[i].raySteer = raySteering;
                _boidValues[i].steering = collisionAdjustment * avoidanceFactor;
            }
            else {
                _boidValues[i].raySteer = Vector3.zero;
                _boidValues[i].steering = steeringSpeed;
            }
        }
    }
}