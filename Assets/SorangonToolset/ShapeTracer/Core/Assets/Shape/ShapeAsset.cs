using UnityEngine;

namespace SorangonToolset.ShapeTracer.Shapes {
	/// <summary>
	/// An asset that contains the datas of a shape
	/// </summary>
	[CreateAssetMenu(fileName = "NewShape", menuName = "Shape Tracer/Shape", order = 800)]
	public class ShapeAsset : ScriptableObject {
		[SerializeField] public Shape shape = Shape.defaultShape;
	}
}