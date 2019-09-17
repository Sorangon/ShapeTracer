using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathTracer.CrossSectionUtility
{
    [CreateAssetMenu(fileName = "NewCrossSection",menuName = "Path Tracer/Cross Section", order = 100)]
    public class CrossSectionAsset : ScriptableObject
    {
        #region Attributes

        [SerializeField] public CrossSection crossSection = CrossSection.defaultSection;


        #region Accessors

        /*public CrossSection crossSection
        {
            get { return _crossSection; }
            set { _crossSection = value; }
        }*/

        #endregion
        #endregion

        #region Constructors
        #endregion

        #region Methods
        #endregion
    }
}