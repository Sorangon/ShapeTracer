using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoadGenerator
{
    [System.Serializable]
    public class PathData
    {
        #region Attributes

        [SerializeField] private List<PathPoint> _path = new List<PathPoint>();

        #region Accessors

        public PathPoint this[int i]
        {
            get { return _path[i]; }
        }

        public int Length
        {
            get { return _path.Count; }
        }

        #endregion
        #endregion

        #region Constructors

        public PathData()
        {
            _path = new List<PathPoint>()
            {
                new PathPoint(Vector3.back * 2, Vector3.up),
                new PathPoint(Vector3.forward * 2, Vector3.up)
            };
        }

        #endregion

        #region Methods
        #region Public

        /// <summary>
        /// Adds a point in the path
        /// </summary>
        /// <param name="offset">World game object position</param>
        public void AddPoint(Vector3 offset)
        {
            Vector3 newPointPos = offset;

            if(Length > 1)
            {
                newPointPos += (_path[_path.Count - 1].position - _path[_path.Count - 2].position).normalized * 2;
            }
            else
            {
                newPointPos += _path[_path.Count - 1].position  + Vector3.forward * 2;
            }

            _path.Add(new PathPoint(newPointPos, _path[_path.Count - 1].normal));
        }

        /// <summary>
        /// Removes a point on the path at selected index
        /// </summary>
        /// <param name="pointId">Point index</param>
        public void RemovePoint(int pointId)
        {
            if(pointId > 0)
            {
                _path.RemoveAt(pointId);
            }
            else
            {
                Debug.LogWarning("This point doesn't exist");
            }
            Debug.Log("Remove Point");
        }

        public Vector3 GetBezierPosition(int fromIndex, float t)
        {
            return GetBezierPosition
                (_path[fromIndex].position,
                _path[fromIndex].GetWorldSpaceTangent(PathTangentType.Out),
                _path[fromIndex + 1].GetWorldSpaceTangent(PathTangentType.In), 
                _path[fromIndex + 1].position, t);
        }

        public Vector3 GetBezierPosition(Vector3 p0, Vector3 t0Out, Vector3 t1In, Vector3 p1, float t)
        {
            float oneMinusT = 1 - t;
            return p0 * oneMinusT * oneMinusT * oneMinusT
                + 3 * t0Out * t * oneMinusT * oneMinusT
                + 3 * t1In * t * t * oneMinusT
                + p1 * t * t * t;
        }

        #endregion
        #endregion
    }
}