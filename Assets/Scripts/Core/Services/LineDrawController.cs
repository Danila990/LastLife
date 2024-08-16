using Core.Factory.DataObjects;
using SharedUtils;
using UnityEngine;

namespace Core.Services
{
    public class LineDrawController
    {
        private const int LINE_SMOOTHNESS = 20;
        private readonly IDrawLineData _data;
        private readonly Vector3[] _cachePoints;
        private LineRenderer _lineRenderer;
        private Vector3 _start;
        private Vector3 _mid;
        private Vector3 _end;

        public LineDrawController(
            IDrawLineData data
        )
        {
            _data = data;
            _cachePoints = new Vector3[3];
        }
        
        public void Init()
        {
            _lineRenderer = Object.Instantiate(_data.LineRenderer);
            _lineRenderer.positionCount = LINE_SMOOTHNESS;
            DisableLine();
        }

        public void OnUpdate()
        {
            DrawLineInternal(_start,_mid,_end);
        }

        public void DisableLine()
        {
            _lineRenderer.gameObject.SetActive(false);
        }

        public void DrawLine(Vector3 start, Vector3 mid, Vector3 end)
        {
            _lineRenderer.gameObject.SetActive(true);
            _start = start;
            _mid = mid;
            _end = end;
            DrawLineInternal(_start,_mid,_end);
        }
        
        private void DrawLineInternal(Vector3 start, Vector3 mid, Vector3 end)
        {
            for (var i = 0; i < LINE_SMOOTHNESS; i++)
            {
                _cachePoints[0] = start;
                _cachePoints[1] = mid;
                _cachePoints[2] = end;
                var step = i / (LINE_SMOOTHNESS * 1f);
                Util.GetBezierCurvePoint(_cachePoints, 3, step);
                _lineRenderer.SetPosition(i, _cachePoints[0]);
            }
        }
    }
}