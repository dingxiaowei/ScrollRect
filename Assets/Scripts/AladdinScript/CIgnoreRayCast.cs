using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Mga
{
	public class CIgnoreRayCast : MonoBehaviour, ICanvasRaycastFilter
	{
		public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
		{
			return false;
		}
	}
}
