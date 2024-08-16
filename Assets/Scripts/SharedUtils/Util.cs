using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace SharedUtils
{
	public static class Util
	{
		public static int IndexOf<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate) {

			var index = 0;
			foreach (var item in source) {
				if (predicate.Invoke(item)) {
					return index;
				}
				index++;
			}

			return -1;
		}
		
		private static ColliderByDistanceSort _colliderBy;
		public static ColliderByDistanceSort ColliderByDistanceSort => _colliderBy ??= new();

		public static IList<T> Shuffle<T>(this IList<T> ts) 
		{
			var count = ts.Count;
			var last = count - 1;
			for (var i = 0; i < last; ++i) {
				var r = Random.Range(i, count);
				(ts[i], ts[r]) = (ts[r], ts[i]);
			}
			
			return ts;
		}
		
		public static T GetRandom<T>(this IList<T> coll)
		{
			if (coll.Count == 0)
				throw new ArgumentException($"Collection {coll} Empty");
			return coll[Random.Range(0, coll.Count)];
		}
		
		public static T GetRandom<T>(this IEnumerable<T> coll)
		{
			var enumerable = coll as T[] ?? coll.ToArray();
			return GetRandom(enumerable);
		}
		
		public static T GetRandomOrDefault<T>(this IEnumerable<T> coll)
		{
			var enumerable = coll as T[] ?? coll.ToArray();
			return GetRandomOrDefault(enumerable);
		}
		
		public static T GetRandomOrDefault<T>(this IList<T> coll)
		{
			return coll.Count == 0 ? default(T) : coll[Random.Range(0, coll.Count)];
		}
		
		public static TimeSpan ToSec(this float value)
		{
			return TimeSpan.FromSeconds(value);
		}

		public static void DrawBounds(this Bounds bounds, float time = 0.3f)
		{
#if !UNITY_EDITOR
			return;
#endif
			Vector3 v3Center = bounds.center;
			Vector3 v3Extents = bounds.extents;

			var v3FrontTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z); // Front top left corner
			var v3FrontTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z); // Front top right corner
			var v3FrontBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z); // Front bottom left corner
			var v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z); // Front bottom right corner
			var v3BackTopLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z); // Back top left corner
			var v3BackTopRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z); // Back top right corner
			var v3BackBottomLeft = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z); // Back bottom left corner
			var v3BackBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z); // Back bottom right corner


			var color = Color.magenta;
			Debug.DrawLine(v3FrontTopLeft, v3FrontTopRight, color,time);
			Debug.DrawLine(v3FrontTopRight, v3FrontBottomRight, color,time);
			Debug.DrawLine(v3FrontBottomRight, v3FrontBottomLeft, color,time);
			Debug.DrawLine(v3FrontBottomLeft, v3FrontTopLeft, color,time);

			Debug.DrawLine(v3BackTopLeft, v3BackTopRight, color,time);
			Debug.DrawLine(v3BackTopRight, v3BackBottomRight, color,time);
			Debug.DrawLine(v3BackBottomRight, v3BackBottomLeft, color,time);
			Debug.DrawLine(v3BackBottomLeft, v3BackTopLeft, color,time);

			Debug.DrawLine(v3FrontTopLeft, v3BackTopLeft, color,time);
			Debug.DrawLine(v3FrontTopRight, v3BackTopRight, color,time);
			Debug.DrawLine(v3FrontBottomRight, v3BackBottomRight, color,time);
			Debug.DrawLine(v3FrontBottomLeft, v3BackBottomLeft, color,time);
		}
		
		public static void SetLayerRecursively(GameObject obj, int newLayer)
		{
			if (null == obj)
			{
				return;
			}
       
			obj.layer = newLayer;
       
			foreach (Transform child in obj.transform)
			{
				if (null == child)
				{
					continue;
				}
				SetLayerRecursively(child.gameObject, newLayer);
			}
		}
		
		public static void DrawSphere(Vector3 position, Quaternion orientation, float radius, Color color, float duration = 0.25f, int segments = 4)
		{
#if !UNITY_EDITOR
			return;
#endif
			if(segments < 2)
			{
				segments = 2;
			}
 
			int doubleSegments = segments * 2;
         
			float meridianStep = 180.0f / segments;
 
			for (int i = 0; i < segments; i++)
			{
				DrawCircle(position, orientation * Quaternion.Euler(0, meridianStep * i, 0), radius, doubleSegments, color, duration);
			}
 
			Vector3 verticalOffset = Vector3.zero;
			float parallelAngleStep = Mathf.PI / segments;
			float stepRadius = 0.0f;
			float stepAngle = 0.0f;
 
			for (int i = 1; i < segments; i++)
			{
				stepAngle = parallelAngleStep * i;
				verticalOffset = orientation * Vector3.up * (Mathf.Cos(stepAngle) * radius);
				stepRadius = Mathf.Sin(stepAngle) * radius;
 
				DrawCircle(position + verticalOffset, orientation * Quaternion.Euler(90.0f, 0, 0), stepRadius, doubleSegments,  color, duration);
			}
		}
		
		public static void DrawCircle(Vector3 position, Quaternion rotation, float radius, int segments, Color color, float duration = 0.25f)
		{
			if (radius <= 0.0f || segments <= 0)
			{
				return;
			}
 
			float angleStep = (360.0f / segments);
			angleStep *= Mathf.Deg2Rad;
 
			Vector3 lineStart = Vector3.zero;
			Vector3 lineEnd = Vector3.zero;
 
			for (int i = 0; i < segments; i++)
			{
				lineStart.x = Mathf.Cos(angleStep * i);
				lineStart.y = Mathf.Sin(angleStep * i);
				lineStart.z = 0.0f;
 
				lineEnd.x = Mathf.Cos(angleStep * (i + 1));
				lineEnd.y = Mathf.Sin(angleStep * (i + 1));
				lineEnd.z = 0.0f;
 
				lineStart *= radius;
				lineEnd *= radius;
 
				lineStart = rotation * lineStart;
				lineEnd = rotation * lineEnd;
 
				lineStart += position;
				lineEnd += position;
 
				Debug.DrawLine(lineStart, lineEnd, color, duration);
			}
		}
		
		public static Vector2 ActualSize(this RectTransform trans, Canvas canvas)
		{
			var v = new Vector3[4];
			trans.GetWorldCorners(v);
			//method one
			//return new Vector2(v[3].x - v[0].x, v[1].y - v[0].y);

			//method two
			return RectTransformUtility.PixelAdjustRect(trans,canvas).size;
		}
		
		public static void ForGizmo(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
		{
#if !UNITY_EDITOR
			return;
#endif
			Gizmos.DrawRay(pos, direction);

			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		public static void ForGizmo(Vector3 pos, Vector3 direction, Color color, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f)
		{
#if !UNITY_EDITOR
			return;
#endif			
			Gizmos.color = color;
			Gizmos.DrawRay(pos, direction);

			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Gizmos.DrawRay(pos + direction, right * arrowHeadLength);
			Gizmos.DrawRay(pos + direction, left * arrowHeadLength);
		}

		public static void ForDebug(Vector3 pos, Vector3 direction, float arrowHeadLength = 0.25f, float arrowHeadAngle = 20.0f, float duration = 0.1f)
		{
#if !UNITY_EDITOR
			return;
#endif
			Debug.DrawRay(pos, direction, Color.white, duration);

			Vector3 right = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 + arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Vector3 left = Quaternion.LookRotation(direction) * Quaternion.Euler(0, 180 - arrowHeadAngle, 0) * new Vector3(0, 0, 1);
			Debug.DrawRay(pos + direction, right * arrowHeadLength, Color.magenta, duration);
			Debug.DrawRay(pos + direction, left * arrowHeadLength, Color.magenta, duration);
		}

		public static string ToShortString(this int num)
		{
			if (num > 10_000_000)
			{
				return Mathf.CeilToInt(num / 1_000_000f) + "M";
			}
			if (num > 1_000_000)
			{
				return Mathf.CeilToInt(num / 1_000_00f)/10f+"M";
			}
			if (num > 100_000)
			{
				return Mathf.CeilToInt(num / 1_000f)+"K";
			}
			if (num > 1_000)
			{
				return Mathf.CeilToInt(num / 1_00f)/10f+"K";
			}
			return num.ToString();
		}
		
		public static float ClampAngle(float lfAngle, float lfMin, float lfMax)
		{
			if (lfAngle < -360f) lfAngle += 360f;
			if (lfAngle > 360f) lfAngle -= 360f;
			return Mathf.Clamp(lfAngle, lfMin, lfMax);
		}

		public static IList<Vector3> GetBezierCurvePoint(IList<Vector3> points,int len, float step)
		{
			for (var i = 0; i < len-1; i++)
			{
				points[i] = Vector3.Lerp(points[i], points[i + 1], step);
			}
			if (len <= 1)
			{
				return points;
			}
			return GetBezierCurvePoint(points, len - 1, step);
		}
		
		/// <summary>
		/// Closest point on a line segment from a given point in 3D.
		/// </summary>
		public static Vector3 ClosestPointOnLineSegment( float px, float py, float pz, float ax, float ay, float az, float bx, float by, float bz )
		{
			var apx = px - ax;
			var apy = py - ay;
			var apz = pz - az;
			var abx = bx - ax;
			var aby = by - ay;
			var abz = bz - az;
			var abMag = abx * abx + aby * aby + abz * abz; // Sqr magnitude.
			if( abMag < Mathf.Epsilon ) return new Vector3( ax, ay, az );
			// Normalize.
			abMag = Mathf.Sqrt( abMag );
			abx /= abMag;
			aby /= abMag;
			abz /= abMag;
			var mu = abx * apx + aby * apy + abz * apz; // Dot.
			if( mu < 0 ) return new Vector3( ax, ay, az );
			if( mu > abMag ) return new Vector3( bx, by, bz );
			return new Vector3( ax + abx * mu, ay + aby * mu, az + abz * mu );
		}
		
		/// <summary>
		/// Closest point on a line segment from a given point in 3D.
		/// </summary>
		public static Vector3 ClosestPointOnLineSegment( Vector3 p, Vector3 a, Vector3 b )
		{
			return ClosestPointOnLineSegment( p.x, p.y, p.z, a.x, a.y, a.z, b.x, b.y, b.z );
		}
		
		public static Transform GetBoneChildDeepth(this Animator animator, HumanBodyBones bone, int deepth)
		{
			var refBone = animator.GetBoneTransform(bone);
			for (int i = 0; i < deepth; i++)
			{
				refBone = refBone.GetChild(0);
			}
			return refBone;
		}
	}
}