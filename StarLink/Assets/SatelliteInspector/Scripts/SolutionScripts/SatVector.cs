using UnityEngine;

namespace SatelliteInspector.Scripts.SolutionScripts
{
    public class SatVector : ScriptableObject
    {
        /// <summary>
        /// ECI system position
        /// </summary>
        public Vector3 PositionECI;
        /// <summary>
        /// ECI system velocity
        /// </summary>
        public Vector3 VelocityECI;

        /// <summary>
        /// Create new result
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="vel"></param>
        public void CreateResult(Vector3 pos, Vector3 vel) 
        {
            PositionECI = pos;
            VelocityECI = vel;
        }
        /// <summary>
        /// Scale vectors
        /// </summary>
        /// <param name="multiplePosition">multiple</param>
        /// <param name="multipleVelocity">multiple</param>
        public void Scale(double multiplePosition, double multipleVelocity)
        {
            PositionECI[0] = PositionECI.x * (float)multiplePosition;
            PositionECI[1] = PositionECI.y * (float)multiplePosition;
            PositionECI[2] = PositionECI.z * (float)multiplePosition;

            VelocityECI[0] = VelocityECI.x * (float)multipleVelocity;
            VelocityECI[1] = VelocityECI.y * (float)multipleVelocity;
            VelocityECI[2] = VelocityECI.z * (float)multipleVelocity;
        }
    }
}




