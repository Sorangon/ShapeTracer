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

        #region Accessors

        public Vector2[] points
        {
            get { return _points; }
        }

        public static CrossSection defaultSection
        {
            get
            {
                CrossSection section = new CrossSection(new Vector2[2] { new Vector2(-1, 0), new Vector2(1, 0) });
                return section;
            }
        }

        #endregion
        #endregion

        #region Contructors

        public CrossSection(Vector2[] points)
        {
            _points = points;
        }


        #endregion

        #region Methods
        #endregion
    }
}

