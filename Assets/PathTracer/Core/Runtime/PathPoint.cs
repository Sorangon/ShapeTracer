using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathTracer
{
    [System.Serializable]
    public class PathPoint
    {
        #region Attributes

        [SerializeField] private Vector3 _position = Vector3.zero;
        [SerializeField] private Vector3 _normal = Vector3.up;
        //[SerializeField] private Vector3 _binormal = Vector3.right;
        [SerializeField] private Vector3 _inTangent = Vector3.forward;
        [SerializeField] private Vector3 _outTangent = Vector3.back;
        [SerializeField] private float _normalAngle = 0.0f;

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

        /*public Vector3 binormal
        {
            get { return _binormal; }
            set { _binormal = value; }
        }*/

        public float normalAngle
        {
            get { return _normalAngle; }
            set { _normalAngle = value; }
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

        public PathPoint(Vector3 position, Vector3 normal, Vector3 inTangent, Vector3 outTangent)
        {
            _position = position;
            _normal = normal;
            _inTangent = inTangent;
            _outTangent = outTangent;
        }

        #endregion

        #region Methods

        public Vector3 GetPointSpaceTangent(PathTangentType tangent)
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

        public void SetPointSpaceTangent(PathTangentType tangent,Vector3 newPos)
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

        public Vector3 GetObjectSpaceTangent(PathTangentType tangent)
        {
            return GetPointSpaceTangent(tangent) + _position;
        }

        public void SetObjectSpaceTangent(PathTangentType tangent, Vector3 newPos)
        {
            SetPointSpaceTangent(tangent, newPos - _position);
        }

        #endregion
    }
}