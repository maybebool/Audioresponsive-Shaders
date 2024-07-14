using System;
using UnityEngine;

namespace Flocking {
    public class BoidsCarrier : FlockingBehaviour {
        
        
        protected virtual int BufferSizeCalc => BoidConductValues.Size;
        private BoidConductValues[] _boidValues;

        protected override void InitializeBoidData() {
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

            CheckForTargetAndRepulsionPointBehaviour();

            HandleComputeBufferData();

            CheckForBoidsTransfromBehaviour();
            
            CheckForAudioReactiveSmoothness();
        }

        private void HandleComputeBufferData() {
            var boidBuffer = new ComputeBuffer(boidsArray.Length, BufferSizeCalc);
            boidBuffer.SetData(_boidValues);
            compute.SetBuffer(0, BoidBodies, boidBuffer);

            SetComputeParameters();

            var threadGroups = Mathf.CeilToInt(amountOfBoids / (float)ThreadGroupSize);
            compute.Dispatch(0, threadGroups, 1, 1);

            boidBuffer.GetData(_boidValues);
            boidBuffer.Release();
        }

        private void CheckForAudioReactiveSmoothness() {
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
        }

        private void CheckForBoidsTransfromBehaviour() {
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
        }

        private void RaycastTypeCheck(Vector3 tempPos, Vector3 tempFwd, int i) {
            var didHit = false;
            RaycastHit raycastHit = default;
            switch (raycastType) {
                case RaycastType.Synchronous:
                    Physics.Raycast(tempPos, tempFwd, out raycastHit, raycastDistance, terrainMask);
                    didHit = raycastHit.collider != null;
                    break;
                case RaycastType.None:
                    break;
                case RaycastType.Asynchronous:
                default:
                    throw new ArgumentOutOfRangeException();
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