using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathTracer.CrossSectionUtility
{
    [CreateAssetMenu(fileName = "NewCrossSection",menuName = "Path Tracer/Cross Section", order = 100)]
    public class CrossSectionAsset : ScriptableObject
    {
        #region Attributes

        [SerializeField] private CrossSection _crossSection = CrossSection.defaultSection;


        #region Accessors

        public CrossSection crossSection
        {
            get { return _crossSection; }
        }

        #endregion
        #endregion

        #region Constructors
        #endregion

        #region Methods
        #endregion
    }
}