using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//TODO:当前只试应横向的ScrollRect，还需要扩展支持纵向
public class CFixGridRect : MonoBehaviour, IEndDragHandler
{
	public GameObject content;
	public ScrollRect scorllRect;
	public float itemWidth;
	private RectTransform contentRectTf;

	private float formalPosX = 0;
	private float currentPosX = 0;
	private float halfItemLength = 0;

	void Start()
	{
		if (itemWidth <= 0)
			UnityEngine.Debug.LogError("请设置Item的宽度");
		halfItemLength = itemWidth / 2;
		this.contentRectTf = this.content.GetComponent<RectTransform>();
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		this.scorllRect.StopMovement();
		Vector2 afterDragPagePos = this.content.transform.localPosition;
		currentPosX = afterDragPagePos.x; //当前拖动的位置  负
		if (scorllRect.horizontalNormalizedPosition < 0 || scorllRect.horizontalNormalizedPosition > 1)
			return;
		int count = (int)(Mathf.Abs(currentPosX) / itemWidth);
		var targetPos = -(float)(count * itemWidth);

		if (((float)(count * itemWidth + halfItemLength)) < Mathf.Abs(currentPosX)) //移动不超过一半
		{
			targetPos = -(float)((count + 1) * itemWidth);
		}
		formalPosX = targetPos;
		this.contentRectTf.DOLocalMoveX(targetPos, .2f);
	}
}
