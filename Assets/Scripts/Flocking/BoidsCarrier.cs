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


        /// <summary>
        /// Sets the compute buffer data for the boids, and dispatches the compute shader to perform the calculations.
        /// </summary>
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

        /// <summary>
        /// Checks for audio reactive smoothness and updates the material's smoothness property based on the audio amplitude buffer.
        /// </summary>
        /// <remarks>
        /// If the <see cref="useMaterialSmoothness"/> flag is set to true and the audio amplitude buffer is greater than the <see cref="smoothnessThreshold"/>,
        /// the material's smoothness property is updated using the <see cref="Mathf.Lerp"/> function with the <see cref="minMaxValueSmoothness"/> values.
        /// If the <see cref="useMaterialSmoothness"/> flag is set to false, the material's smoothness property is set to 0.
        /// </remarks>
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

        /// <summary>
        /// Checks for boid transform behaviour and updates the positions, rotations, and scales of the boids. Scale is audio reactive.
        /// </summary>
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

        /// <summary>
        /// Checks for raycast type and performs a raycast to determine if a collision with terrain occurs.
        /// Updates the boid conduct values based on the result of the raycast.
        /// </summary>
        /// <param name="tempPos">The temporary position of the boid.</param>
        /// <param name="tempForward">The temporary forward direction of the boid.</param>
        /// <param name="i">The index of the boid in the array.</param>
        private void RaycastTypeCheck(Vector3 tempPos, Vector3 tempForward, int i) {
            var didHit = false;
            RaycastHit raycastHit = default;
            switch (raycastType) {
                case RaycastType.Synchronous:
                    Physics.Raycast(tempPos, tempForward, out raycastHit, raycastDistance, terrainMask);
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