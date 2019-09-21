using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathTracer.Shapes
{
    [CreateAssetMenu(fileName = "NewShape",menuName = "Path Tracer/Shape", order = 100)]
    public class ShapeAsset : ScriptableObject
    {
        #region Attributes

        [SerializeField] public Shape shape = Shape.defaultShape;


        #region Accessors

        /*public CrossSection crossSection
        {
            get { return _crossSection; }
            set { _crossSection = value; }
        }*/

        #endregion
        #endregion

        #region Points
        #endregion
    }
}