using UnityEngine;

namespace Flocking {
    
    public struct BoidConductValues {
        public Vector3 position { get; set; }
        public Vector3 forward { get; set; }
        public Vector3 raySteer { get; set; }
        public float steering { get; set; }
        public static int Size => (sizeof(float) * 3 * 3) + sizeof(float);
    }
}