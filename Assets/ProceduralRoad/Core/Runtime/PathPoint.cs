using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoadGenerator
{
    [System.Serializable]
    public class PathPoint
    {
        #region Attributes

        [SerializeField] private Vector3 _position = Vector3.zero;
        [SerializeField] private Vector3 _normal = Vector3.up;
        [SerializeField] private Vector3 _binormal = Vector3.right;
        [SerializeField] private Vector3 _inTangent = Vector3.forward;
        [SerializeField] private Vector3 _outTangent = Vector3.back;

        /// <summary> The position of the point in world space </summary>
        public Vector3 position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <summary> The normal of the point in world space </summary>
        public Vector3 normal
        {
            get { return _normal; }
            set { _normal = value; }
        }

        public Vector3 binormal
        {
            get { return _binormal; }
            set { _binormal = value; }
        }

        #endregion

        #region Constructor

        public PathPoint(Vector3 position, Vector3 normal)
        {
            _position = position;
            _normal = normal;

            Vector3 tangentOffset = new Vector3(1.0f, 0, 0.5f);

            _inTangent = -tangentOffset;
            _outTangent = tangentOffset;
        }

        #endregion

        #region Methods

        public Vector3 GetLocalSpaceTangent(PathTangentType tangent)
        {
            if(tangent == PathTangentType.In)
            {
                return _inTangent;
            }
            else if(tangent == PathTangentType.Out)
            {
                return _outTangent;
            }
            else
            {
                return Vector3.zero;
            }
        }

        public void SetLocalSpaceTangent(PathTangentType tangent,Vector3 newPos)
        {
            if (tangent == PathTangentType.In)
            {
                _inTangent = newPos;
            }
            else if(tangent == PathTangentType.Out)
            {
                _outTangent = newPos;
            }
        }

        public Vector3 GetWorldSpaceTangent(PathTangentType tangent)
        {
            return GetLocalSpaceTangent(tangent) + _position;
        }

        public void SetWorldSpaceTangent(PathTangentType tangent, Vector3 newPos)
        {
            SetLocalSpaceTangent(tangent, newPos - _position);
        }

        #endregion
    }
}