using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThirdPersonCharaterController
{
    public class GroundCollisionAttribute : MonoBehaviour
    {
        public const float DEFAULT_STAND_ANGLE = 80.0f;
        public const float DEFAULT_SLOPE_LIMIT = 80.0f;

        public float StandAngle = 80.0f;
        public float SlopeLimit = 80.0f;
    }
}
