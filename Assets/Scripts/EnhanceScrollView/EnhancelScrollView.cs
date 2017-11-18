using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnhancelScrollView : MonoBehaviour
{
	public AnimationCurve scaleCurve;
	public AnimationCurve positionCurve;
	public float posCurveFactor = 500.0f;
	public float yPositionValue = 46.0f;

	public List<EnhanceItem> scrollViewItems = new List<EnhanceItem>();
	private List<Image> imageTargets = new List<Image>();

	private EnhanceItem centerItem;
	private EnhanceItem preCenterItem;

	private bool canChangeItem = true;

	public float dFactor = 0.2f;

	private float[] moveHorizontalValues;
	private float[] dHorizontalValues;

	public float horizontalValue = 0.0f;
	public float horizontalTargetValue = 0.1f;

	private float originHorizontalValue = 0.1f;
	public float duration = 0.2f;
	private float currentDuration = 0.0f;

	private static EnhancelScrollView instance;

	private bool isInit = false;
	public static EnhancelScrollView GetInstance()
	{
		return instance;
	}

	void Awake()
	{
		instance = this;
	}

	public void Init()
	{
		if ((scrollViewItems.Count % 2) == 0)
		{
			Debug.LogError("item count is invaild,please set odd count! just support odd count.");
		}

		if (moveHorizontalValues == null)
			moveHorizontalValues = new float[scrollViewItems.Count];

		if (dHorizontalValues == null)
			dHorizontalValues = new float[scrollViewItems.Count];

		if (imageTargets == null)
			imageTargets = new List<Image>();

		int centerIndex = scrollViewItems.Count / 2;
		for (int i = 0; i < scrollViewItems.Count; i++)
		{
			scrollViewItems[i].scrollViewItemIndex = i;
			Image tempImage = scrollViewItems[i].gameObject.GetComponent<Image>();
			imageTargets.Add(tempImage);

			dHorizontalValues[i] = dFactor * (centerIndex - i);

			dHorizontalValues[centerIndex] = 0.0f;
			moveHorizontalValues[i] = 0.5f - dHorizontalValues[i];
			scrollViewItems[i].SetSelectColor(false);
		}

		//centerItem = scrollViewItems[centerIndex];
		canChangeItem = true;
		isInit = true;
	}

	public void UpdateEnhanceScrollView(float fValue)
	{
		for (int i = 0; i < scrollViewItems.Count; i++)
		{
			EnhanceItem itemScript = scrollViewItems[i];
			float xValue = GetXPosValue(fValue, dHorizontalValues[itemScript.scrollViewItemIndex]);
			float scaleValue = GetScaleValue(fValue, dHorizontalValues[itemScript.scrollViewItemIndex]);
			itemScript.UpdateScrollViewItems(xValue, yPositionValue, scaleValue);
		}
	}

	void Update()
	{
		if (!isInit)
			return;
		currentDuration += Time.deltaTime;
		SortDepth();
		if (currentDuration > duration)
		{
			currentDuration = duration;

			//if (centerItem != null)
			//{
			//	centerItem.SetSelectColor(true);
			//}

			if (centerItem == null)
			{
				var obj = transform.GetChild(transform.childCount - 1);
				if (obj != null)
					centerItem = obj.GetComponent<EnhanceItem>();
				if (centerItem != null)
					centerItem.SetSelectColor(true);
			}
			else
				centerItem.SetSelectColor(true);
			if (preCenterItem != null)
				preCenterItem.SetSelectColor(false);
			canChangeItem = true;
		}

		float percent = currentDuration / duration;
		horizontalValue = Mathf.Lerp(originHorizontalValue, horizontalTargetValue, percent);
		UpdateEnhanceScrollView(horizontalValue);
	}

	/// <summary>
	/// 缩放曲线模拟当前缩放值
	/// </summary>
	private float GetScaleValue(float sliderValue, float added)
	{
		float scaleValue = scaleCurve.Evaluate(sliderValue + added);
		return scaleValue;
	}

	/// <summary>
	/// 位置曲线模拟当前x轴位置
	/// </summary>
	private float GetXPosValue(float sliderValue, float added)
	{
		float evaluateValue = positionCurve.Evaluate(sliderValue + added) * posCurveFactor;
		return evaluateValue;
	}

	public void SortDepth()
	{
		imageTargets.Sort(new CompareDepthMethod());
		for (int i = 0; i < imageTargets.Count; i++)
			imageTargets[i].transform.SetSiblingIndex(i);
	}

	/// <summary>
	/// 用于层级对比接口
	/// </summary>
	public class CompareDepthMethod : IComparer<Image>
	{
		public int Compare(Image left, Image right)
		{
			if (left.transform.localScale.x > right.transform.localScale.x)
				return 1;
			else if (left.transform.localScale.x < right.transform.localScale.x)
				return -1;
			else
				return 0;
		}
	}

	/// <summary>
	/// 获得当前要移动到中心的Item需要移动的factor间隔数
	/// </summary>
	private int GetMoveCurveFactorCount(float targetXPos)
	{
		int centerIndex = scrollViewItems.Count / 2;
		for (int i = 0; i < scrollViewItems.Count; i++)
		{
			float factor = (0.5f - dFactor * (centerIndex - i));

			float tempPosX = positionCurve.Evaluate(factor) * posCurveFactor;
			if (Mathf.Abs(targetXPos - tempPosX) < 0.01f)
				return Mathf.Abs(i - centerIndex);
		}
		return -1;
	}

	/// <summary>
	/// 设置横向轴参数，根据缩放曲线和位移曲线更新缩放和位置
	/// </summary>
	public void SetHorizontalTargetItemIndex(int itemIndex)
	{
		if (!canChangeItem)
			return;

		EnhanceItem item = scrollViewItems[itemIndex];
		if (centerItem == item)
			return;

		canChangeItem = false;
		preCenterItem = centerItem;
		centerItem = item;

		// 判断点击的是左侧还是右侧计算ScrollView中心需要移动的value
		float centerXValue = positionCurve.Evaluate(0.5f) * posCurveFactor;
		bool isRight = false;
		if (item.transform.localPosition.x > centerXValue)
			isRight = true;

		// 差值,计算横向值
		int moveIndexCount = GetMoveCurveFactorCount(item.transform.localPosition.x);
		if (moveIndexCount == -1)
		{
			moveIndexCount = 1;
		}

		float dvalue = 0.0f;
		if (isRight)
			dvalue = -dFactor * moveIndexCount;
		else
			dvalue = dFactor * moveIndexCount;

		horizontalTargetValue += dvalue;
		currentDuration = 0.0f;
		originHorizontalValue = horizontalValue;
	}

	public void OnBtnRightClick()
	{
		if (!canChangeItem)
			return;
		int targetIndex = centerItem.scrollViewItemIndex + 1;
		if (targetIndex > scrollViewItems.Count - 1)
			targetIndex = 0;
		SetHorizontalTargetItemIndex(targetIndex);
	}

	public void OnBtnLeftClick()
	{
		if (!canChangeItem)
			return;
		int targetIndex = centerItem.scrollViewItemIndex - 1;
		if (targetIndex < 0)
			targetIndex = scrollViewItems.Count - 1;
		SetHorizontalTargetItemIndex(targetIndex);
	}
}
