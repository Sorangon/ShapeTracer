using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ShapeTracer.Shapes
{
    [CreateAssetMenu(fileName = "NewShape",menuName = "Shape Tracer/Shape", order = 800)]
    public class ShapeAsset : ScriptableObject
    {
        [SerializeField] public Shape shape = Shape.defaultShape;
    }
}