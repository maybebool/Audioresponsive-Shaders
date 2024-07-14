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
        [SerializeField] protected float raycastDistance = 100f;
        [SerializeField] protected float collisionAdjustment = 50f;
        [SerializeField] protected RaycastType raycastType = RaycastType.Synchronous;

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
            compute.SetBuffer(0, BoidBodies, boidBuffer);

            SetComputeParameters();

            var threadGroups = Mathf.CeilToInt(amountOfBoids / (float)threadGroupSize);
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


            if (useMaterialSmoothness) {
                if (audioData.amplitudeBuffer > smoothnessThreshold) {
                    var lerpValue = Mathf.Lerp(minMaxValueSmoothness.x, minMaxValueSmoothness.y,
                        audioData.amplitudeBuffer);
                    material.SetFloat(Smoothness, 1 - lerpValue);
                }
            }
            else {
                material.SetFloat(Smoothness,0);
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