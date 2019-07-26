using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathTracer
{
    [System.Serializable]
    public class PathData
    {
        #region Attributes

        [SerializeField] private List<PathPoint> _path = new List<PathPoint>();

        #region Accessors

        public PathPoint this[int i]
        {
            get
            {
                return _path[i % Length];
            }
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
            Vector3 newPointPos = _path[Length - 1].GetObjectSpaceTangent(PathTangentType.Out);

            Vector3 newInTangent = _path[Length - 1].GetPointSpaceTangent(PathTangentType.In);
            Vector3 newOutTangent = _path[Length - 1].GetPointSpaceTangent(PathTangentType.Out);

            _path.Add(new PathPoint(newPointPos, _path[_path.Count - 1].normal, newInTangent, newOutTangent));
        }

        /// <summary>
        /// Removes a point on the path at selected index
        /// </summary>
        /// <param name="pointId">Point index</param>
        public void RemovePoint(int pointId)
        {
            if(_path.Count > 2)
            {
                if (pointId > 0)
                {
                    _path.RemoveAt(pointId);
                }
                else
                {
                    Debug.LogWarning("This point doesn't exist");
                }
            }
            else
            {
                Debug.LogWarning("A path cannot have less than 2 point at least");
            }
        }

        public Vector3 GetBezierPosition(int fromPoint, float t)
        {
            int toPoint = (fromPoint + 1) % (Length);

            return GetBezierPosition
                (_path[fromPoint].position,
                _path[fromPoint].GetObjectSpaceTangent(PathTangentType.Out),
                _path[toPoint].GetObjectSpaceTangent(PathTangentType.In), 
                _path[toPoint].position, t);
        }

        public Vector3 GetBezierPosition(Vector3 p0, Vector3 t0Out, Vector3 t1In, Vector3 p1, float t)
        {
            float oneMinusT = 1 - t;
            return p0 * oneMinusT * oneMinusT * oneMinusT
                + 3 * t0Out * t * oneMinusT * oneMinusT
                + 3 * t1In * t * t * oneMinusT
                + p1 * t * t * t;
        }


        public Vector3 GetDerivativeDirection(int fromPoint, float t)
        {
            int toPoint = (fromPoint + 1) % (Length);

            return GetDerivativeDirection
                (_path[fromPoint].position,
                _path[fromPoint].GetObjectSpaceTangent(PathTangentType.Out),
                _path[toPoint].GetObjectSpaceTangent(PathTangentType.In),
                _path[toPoint].position, t);
        }


        public Vector3 GetDerivativeDirection(Vector3 p0, Vector3 t0Out, Vector3 t1In, Vector3 p1, float t)
        {
            float oneMinus = 1 - t;
            return 3 * oneMinus * oneMinus * (t0Out - p0)
                + 6 * oneMinus * t * (t1In - t0Out)
                + 3 * t * t * (p1 - t1In);
        }

        #endregion
        #endregion
    }
}