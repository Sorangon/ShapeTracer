using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathTracer.CrossSectionUtility
{
    [System.Serializable]
    public struct CrossSection
    {
        #region Attributes

        [SerializeField] private Vector2[] _points;
        [SerializeField] private bool _closeShape;

        #region Accessors

        public Vector2[] points
        {
            get { return _points; }
        }

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
                _closeShape = value;
            }
        }

        public static CrossSection defaultSection
        {
            get
            {
                CrossSection section = new CrossSection(new Vector2[2] { new Vector2(-1, 0), new Vector2(1, 0) });
                return section;
            }
        }

        public int pointCount
        {
            get { return _points.Length; }
        }

        #endregion
        #endregion

        #region Contructors

        public CrossSection(Vector2[] points)
        {
            _points = points;
            _closeShape = false;
        }


        #endregion

        #region Methods
        #endregion
    }
}

