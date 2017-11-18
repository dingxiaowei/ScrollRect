using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
namespace Mga
{
	public class CPlayedCardOnDrop : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
	{
		public Image containerImage;
		public Image receivingImage;
		private Color normalColor;
		public Color highlightColor = Color.yellow;
		private int drapAreaIndex = 0;

		void Start()
		{
			drapAreaIndex = System.Convert.ToInt32(this.transform.parent.name);
		}

		public void OnEnable()
		{
			if (containerImage != null)
				normalColor = containerImage.color;
		}

		public void OnDrop(PointerEventData data)
		{
			containerImage.color = normalColor;

			if (receivingImage == null)
				return;
			Sprite dropSprite = GetDropSprite(data);
			if (dropSprite != null)
				receivingImage.overrideSprite = dropSprite;
		}

		public void OnPointerEnter(PointerEventData data)
		{
			if (containerImage == null)
				return;
			Sprite dropSprite = GetDropSprite(data);
			if (dropSprite != null)
				containerImage.color = highlightColor;
		}

		public void OnPointerExit(PointerEventData data)
		{
			if (containerImage == null)
				return;
			containerImage.color = normalColor;
		}

		private Sprite GetDropSprite(PointerEventData data)
		{
			var originalObj = data.pointerDrag;
			if (originalObj == null)
				return null;

			var srcImage = originalObj.GetComponent<Image>();
			if (srcImage == null)
				return null;

			return srcImage.sprite;
		}
	}
}