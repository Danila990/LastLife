using UnityEngine;
using Utils;

namespace Core.Factory.DataObjects
{
    [CreateAssetMenu(menuName = SoNames.SETTINGS+nameof(LineDrawData),fileName = nameof(LineDrawData))]
    public class LineDrawData : ScriptableObject, IDrawLineData
    {
        [SerializeField] private LineRenderer _lineRenderer;
        public LineRenderer LineRenderer => _lineRenderer;
    }
}