using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

namespace ShapeTracer.Shapes
{
    [System.Serializable]
    public struct Shape
    {
        #region Attributes

        [SerializeField] private Vector2[] _points;
        [SerializeField] private bool _closeShape;

        #region Accessors


        public bool closeShape
        {
            get
            {
                if(_points.Length < 2)
                {
                    return false;
                }
                else
                {
                    return _closeShape;
                }
            }
            set
            {
                //Change state
                if(value == true && _closeShape == false)
                {
                    AddPoint(_points[0]);
                }
                else if(value == false && _closeShape == true)
                {
                    RemovePoint(pointCount - 1);
                }

                _closeShape = value;
            }
        }

        public static Shape defaultShape
        {
            get
            {
                Shape shape = new Shape(new Vector2[2] { new Vector2(-1, 0), new Vector2(1, 0) });
                return shape;
            }
        }

        public int pointCount
        {
            get { return _points.Length; }
        }

        #endregion

        #endregion

        #region Contructors

        public Shape(Vector2[] points)
        {
            _points = points;
            _closeShape = false;
        }


        #endregion



        #region Points

        public void SetPointPosition(int index, Vector2 pos)
        {
            if (_closeShape && (index == 0 || index == pointCount - 1))
            {
                _points[0] = pos;
                _points[pointCount - 1] = pos;
            }
            else
            {
                _points[index] = pos;
            }
        }

        public Vector2 GetPointPosition(int index)
        {
            return _points[index];
        }

        public void AddPoint(Vector2 position)
        {
            Array.Resize(ref _points, _points.Length + 1);
            _points[pointCount - 1] = position;
        }

        public void RemovePoint(int index)
        {
            List<Vector2> pts = _points.ToList();
            pts.RemoveAt(index);
            _points = pts.ToArray();
        }

        #endregion
    }
}

