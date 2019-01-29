# UGUI各种优化效果

本文所实现的UGUI效果需求如下：

- **支持不用Mask遮罩无限循环加载**
- **支持ObjectPool动态加载**
- **支持无限不规则子物体动态加载**
- **支持拖动并点击和拖拽**
- **支持拖动并拖拽**
- **支持ScrollRect拖动自动吸附功能(拖动是否超过一半自动进退)**

-------------------


## 前言

> 要实现以上效果，我从网上搜索得到部分解决方案[链接](https://github.com/qiankanglai/LoopScrollRect)，但不是完全满足想要的效果，就自己继续改造优化和添加想要的效果，本文最后会附带上完整Demo下载链接。

### 效果图
- **缩放循环展示卡牌效果**

[](http://dingxiaowei.cn/2017/04/25/2.gif)

- **缩放循环展示卡牌效果**

[](http://dingxiaowei.cn/2017/04/25/2.gif)

- **大量数据无卡顿动态加载，并且支持拖拽、点击和吸附功能**

[](http://dingxiaowei.cn/2017/04/25/3.gif)

- **大量数据循固定Item复用**

[](http://dingxiaowei.cn/2017/04/25/4.gif)

- **无限无遮罩动态加载**

[](http://dingxiaowei.cn/2017/04/25/5.gif)


### 部分核心代码

- **有遮罩无卡顿加载**
思路：并没有使用UGUI的ScrollRect组件，摆放几张卡片，通过移动和缩放来实现

```
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnhancelScrollView : MonoBehaviour
{
    // 缩放曲线
    public AnimationCurve scaleCurve;
    // 位移曲线
    public AnimationCurve positionCurve;
    // 位移系数
    public float posCurveFactor = 500.0f;
    // y轴坐标固定值(所有的item的y坐标一致)
    public float yPositionValue = 46.0f;

    // 添加到EnhanceScrollView的目标对象
    public List<EnhanceItem> scrollViewItems;
    // 目标对象Widget脚本，用于depth排序
    private List<Image> imageTargets;

    // 当前处于中间的item
    private EnhanceItem centerItem;
    private EnhanceItem preCenterItem;

    // 当前出移动中，不能进行点击切换
    private bool canChangeItem = true;

    // 计算差值系数
    public float dFactor = 0.2f;
    
    // 点击目标移动的横向目标值
    private float[] moveHorizontalValues;
    // 对象之间的差值数组(根据差值系数算出)
    private float[] dHorizontalValues;

    // 横向变量值
    public float horizontalValue = 0.0f;
    // 目标值
    public float horizontalTargetValue = 0.1f;

    // 移动动画参数
    private float originHorizontalValue = 0.1f;
    public float duration = 0.2f;
    private float currentDuration = 0.0f;

    private static EnhancelScrollView instance;
    public static EnhancelScrollView GetInstance()
    {
        return instance;
    }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if((scrollViewItems.Count % 2) == 0)    
        {
            Debug.LogError("item count is invaild,please set odd count! just support odd count.");
        }

        if(moveHorizontalValues == null)
            moveHorizontalValues = new float[scrollViewItems.Count];

        if(dHorizontalValues == null)
            dHorizontalValues = new float[scrollViewItems.Count];

        if (imageTargets == null)
            imageTargets = new List<Image>();

        int centerIndex = scrollViewItems.Count / 2;
        for (int i = 0; i < scrollViewItems.Count;i++ )
        {
            scrollViewItems[i].scrollViewItemIndex = i;
			Image tempImage = scrollViewItems[i].gameObject.GetComponent<Image>();
            imageTargets.Add(tempImage);

            dHorizontalValues[i] = dFactor * (centerIndex - i);

            dHorizontalValues[centerIndex] = 0.0f;
            moveHorizontalValues[i] = 0.5f - dHorizontalValues[i];
            scrollViewItems[i].SetSelectColor(false);
        }

        centerItem = scrollViewItems[centerIndex];
        canChangeItem = true;
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
		currentDuration += Time.deltaTime;
		if (currentDuration > duration)
		{
			// 更新完毕设置选中item的对象即可
			currentDuration = duration;
			if (centerItem != null)
				centerItem.SetSelectColor(true);
			if (preCenterItem != null)
				preCenterItem.SetSelectColor(false);
			canChangeItem = true;
		}

		SortDepth();
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
        for (int i = 0; i < scrollViewItems.Count;i++ )
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

        // 更改target数值，平滑移动
        horizontalTargetValue += dvalue;
        currentDuration = 0.0f;
        originHorizontalValue = horizontalValue;
    }

    /// <summary>
    /// 向右选择角色按钮
    /// </summary>
    public void OnBtnRightClick()
    {
        if (!canChangeItem)
            return;
        int targetIndex = centerItem.scrollViewItemIndex + 1;
        if (targetIndex > scrollViewItems.Count - 1)
            targetIndex = 0;
        SetHorizontalTargetItemIndex(targetIndex);
    }

    /// <summary>
    /// 向左选择按钮
    /// </summary>
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

```

```
using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnhanceItem : MonoBehaviour {

    // 在ScrollViewitem中的索引
    // 定位当前的位置和缩放
    public int scrollViewItemIndex = 0;
    public bool inRightArea = false;

    private Vector3 targetPos = Vector3.one;
    private Vector3 targetScale = Vector3.one;

    private Transform mTrs;
    private Image mImage;


    void Awake()
    {
        mTrs = this.transform;
        mImage = this.GetComponent<Image>();
    }

    void Start()
    {
		this.gameObject.GetComponent<Button>().onClick.AddListener(delegate () { OnClickScrollViewItem(); });
	}

    // 当点击Item，将该item移动到中间位置
    private void OnClickScrollViewItem()
    {
        EnhancelScrollView.GetInstance().SetHorizontalTargetItemIndex(scrollViewItemIndex);
    }

    /// <summary>
    /// 更新该Item的缩放和位移
    /// </summary>
    public void UpdateScrollViewItems(float xValue, float yValue, float scaleValue)
    {
        targetPos.x = xValue;
        targetPos.y = yValue;
        targetScale.x = targetScale.y = scaleValue;

        mTrs.localPosition = targetPos;
        mTrs.localScale = targetScale;
    }

    public void SetSelectColor(bool isCenter)
    {
        if (mImage == null)
            mImage = this.GetComponent<Image>();
		
        if (isCenter)
            mImage.color = Color.white;
        else
            mImage.color = Color.gray;
    }
}

```


- **有遮罩无卡顿加载**
思路：协程加载，先加载屏幕显示的数量，然后返回一帧在继续加载，防止出现数量太大卡顿的现象。
```
while (CardsList.Count > roleInfo.Count)
{
	DestroyImmediate(CardsList[0].gameObject);
	CardsList.RemoveAt(0);
}
StartCoroutine(createRoleCards());

private IEnumerator createRoleCards()
{
	List<CLocalCharInfo> charInfos = new List<CLocalCharInfo>();
	charInfos.AddRange(roleInfo);
	int index = 0;
	for (int i = 0; i < charInfos.Count; i++)
	{
		_createRoleCard(charInfos[i], index++);
		if (index % 10 == 0)
			yield return null;
	}
}

private void _createRoleCard(CLocalCharInfo roleInfo, int index)
{
	CUIPlayedCharCardWidget charCardWidget = null;
	if (CardsList.Count > index)
	{
		charCardWidget = CardsList[index];
	}
	else
	{
		var obj = Instantiate(Resources.Load<GameObject>("Prefab/RoleCard")) as GameObject;
		if (obj == null)
		{
			UnityEngine.Debug.LogError("有误");
			return;
		}
		obj.name = roleInfo.Name;
		
		charCardWidget = obj.GetComponent<CUIPlayedCharCardWidget>();

		if (charCardWidget == null)
		{
			UnityEngine.Debug.LogError("有误");
			return;
		}
		obj.transform.parent = Obj_ScrollViewContent.transform;
		obj.transform.localScale = Vector3.one;
		CardsList.Add(charCardWidget);
	}

	CUIPlayedCharCardWidget.CUIContent uiContent = new CUIPlayedCharCardWidget.CUIContent();
	uiContent.RoleInfo = roleInfo;
	uiContent.ScrollRectObj = m_ScrollRect;
	uiContent.FixGridRect = m_FixGrid;
	charCardWidget.InitContent(uiContent);
}
```

- **支持ScrollRect拖拽或点击**
思路：在卡片的Image上添加一个继承了IBeginDragHandler,IGradHandler,IEndDragHandler的脚本，重写接口里面的Drag事件方法。

```
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mga
{
	[RequireComponent(typeof(Image))]
	public class CPlayedCardOnDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		public bool dragOnSurfaces = true;
		public ScrollRect m_ScrollRect = null;
		public CFixGridRect m_FixGridRect = null;
		private GameObject m_DraggingCard;
		private RectTransform m_DraggingPlane;

		public bool isVertical = false;
		private bool isSelf = false;

		private System.Action m_OnBeginDragCallBack = null;
		private System.Action m_OnEndDragCallBack = null;

		private System.Action m_OnBeginScroll = null;
		private System.Action m_OnEndScroll = null;
		public void Init(CLocalCharInfo roleInfo, System.Action beginCallBack = null, System.Action endCallBack = null, System.Action beginScroll = null, System.Action endScroll = null)
		{
			m_OnBeginDragCallBack = beginCallBack;
			m_OnEndDragCallBack = endCallBack;
			m_OnBeginScroll = beginScroll;
			m_OnEndScroll = endScroll;
		}
		public void OnBeginDrag(PointerEventData eventData)
		{
			Vector2 touchDeltaPosition = Vector2.zero;
#if UNITY_EDITOR
			float delta_x = Input.GetAxis("Mouse X");
			float delta_y = Input.GetAxis("Mouse Y");
			touchDeltaPosition = new Vector2(delta_x, delta_y);

#elif UNITY_ANDROID || UNITY_IPHONE
        touchDeltaPosition = Input.GetTouch(0).deltaPosition;  
#endif
			if (isVertical)
			{
				if (Mathf.Abs(touchDeltaPosition.x) > Mathf.Abs(touchDeltaPosition.y))
				{
					isSelf = true;
					var canvas = FindInParents<Canvas>(gameObject);
					if (canvas == null)
						return;
					m_DraggingCard = createCard();
					m_DraggingCard.transform.SetAsLastSibling();

					m_DraggingCard.AddComponent<CIgnoreRayCast>();

					if (dragOnSurfaces)
						m_DraggingPlane = transform as RectTransform;
					else
						m_DraggingPlane = canvas.transform as RectTransform;

					SetDraggedPosition(eventData);
					if (m_OnBeginDragCallBack != null)
					{
						m_OnBeginDragCallBack();
					}
				}
				else
				{
					isSelf = false;
					if (m_ScrollRect != null)
						m_ScrollRect.OnBeginDrag(eventData);
				}
			}
			else
			{
				if (Mathf.Abs(touchDeltaPosition.x) < Mathf.Abs(touchDeltaPosition.y))
				{
					isSelf = true;
					var canvas = FindInParents<Canvas>(gameObject);
					if (canvas == null)
						return;
					m_DraggingCard = createCard();
					m_DraggingCard.transform.SetAsLastSibling();

					m_DraggingCard.AddComponent<CIgnoreRayCast>();

					if (dragOnSurfaces)
						m_DraggingPlane = transform as RectTransform;
					else
						m_DraggingPlane = canvas.transform as RectTransform;

					SetDraggedPosition(eventData);
					if (m_OnBeginDragCallBack != null)
					{
						m_OnBeginDragCallBack();
					}
				}
				else
				{
					isSelf = false;
					if (m_ScrollRect != null)
						m_ScrollRect.OnBeginDrag(eventData);
				}
			}
			if (m_OnBeginScroll != null)
				m_OnBeginScroll();
		}

		public void OnDrag(PointerEventData data)
		{
			if (isSelf)
			{
				if (m_DraggingCard != null)
				{
					SetDraggedPosition(data);
				}
			}
			else
			{
				if (m_ScrollRect != null)
					m_ScrollRect.OnDrag(data);
			}
		}

		private void SetDraggedPosition(PointerEventData data)
		{
			if (dragOnSurfaces && data.pointerEnter != null && data.pointerEnter.transform as RectTransform != null)
				m_DraggingPlane = data.pointerEnter.transform as RectTransform;

			var rt = m_DraggingCard.GetComponent<RectTransform>();
			Vector3 globalMousePos;
			if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlane, data.position, data.pressEventCamera, out globalMousePos))
			{
				rt.position = globalMousePos;
				rt.rotation = m_DraggingPlane.rotation;
			}
		}

		private GameObject createCard()
		{
			CUIPlayedCharCardWidget charCardWidget = null;
			m_DraggingCard = Instantiate(Resources.Load<GameObject>("Prefab/RoleCard") as GameObject);
			if (m_DraggingCard == null)
			{
				return null;
			}
			charCardWidget = m_DraggingCard.GetComponent<CUIPlayedCharCardWidget>();

			if (charCardWidget == null)
			{
				return null;
			}

			CUIPlayedCharCardWidget.CUIContent uiContent = new CUIPlayedCharCardWidget.CUIContent();

			charCardWidget.InitContent(uiContent);
			return m_DraggingCard;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			if (isSelf)
			{
				if (m_DraggingCard != null)
				{
					Destroy(m_DraggingCard);
					if (m_OnEndDragCallBack != null)
					{
						m_OnEndDragCallBack();
					}
				}
			}
			else
			{
				if (m_ScrollRect != null)
					m_ScrollRect.OnEndDrag(eventData);
				if (m_FixGridRect != null)
					m_FixGridRect.OnEndDrag(eventData);
			}

		}

		static public T FindInParents<T>(GameObject go) where T : Component
		{
			if (go == null) return null;
			var comp = go.GetComponent<T>();

			if (comp != null)
				return comp;

			Transform t = go.transform.parent;
			while (t != null && comp == null)
			{
				comp = t.gameObject.GetComponent<T>();
				t = t.parent;
			}
			return comp;
		}
	}
}

```
如果想要实现拖拽到目标位置的检测，还要在目标位置放一个Image并且添加上继承了IDropHandler,IPointerEnterHandler,IPointerExitHanler的组件。

```
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
```
在ScrollRect物体上添加吸附功能组件，工程里面要使用DoTween插件

```
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

		if (((float)(count * itemWidth + halfItemLength)) < Mathf.Abs(currentPosX))
		{
			targetPos = -(float)((count + 1) * itemWidth);
		}
		formalPosX = targetPos;
		this.contentRectTf.DOLocalMoveX(targetPos, .2f);
	}
}

```

### 具体DemoGit下载[链接](https://git.oschina.net/dingxiaowei/scrollrect.git)
<font color=#0099ff size=7 face="黑体">欢迎加入U3D开发交流群:159875734</font>


---------

### 优化支持横竖屏的ScrollRect吸附功能
```
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mga
{
	public enum DragDirection
	{
		Horizontal,
		Vertical,
	}

	public class CFixGridRectBase : MonoBehaviour, IEndDragHandler
	{
		public class CUIContent
		{
			public GameObject ScrollRectContent;
			public ScrollRect m_ScorllRect;
			public float ItemSize;
			public float ItemSpaceLength; //间隙
			public float Margin = 0; //顶部边缘间隙
			public DragDirection m_DragDirection = DragDirection.Vertical;
		}
		private RectTransform contentRectTf;
		private float halfItemLength = 0;
		private CUIContent m_uiContent = null;
		private bool m_bWidgetReady = false;
		void Start()
		{
			m_bWidgetReady = true;
			_initContent();
		}

		public void InitContent(CUIContent uiContent)
		{
			m_uiContent = uiContent;

			if (m_bWidgetReady)
				_initContent();
		}

		private void _initContent()
		{
			if (m_uiContent == null) return;

			if (m_uiContent.ItemSize <= 0)
			{
				UnityEngine.Debug.LogError("请设置Item的宽度");
				return;
			}
			halfItemLength = m_uiContent.ItemSize / 2;
			this.contentRectTf = m_uiContent.ScrollRectContent.GetComponent<RectTransform>();
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			m_uiContent.m_ScorllRect.StopMovement();
			Vector2 afterDragPagePos = m_uiContent.ScrollRectContent.transform.localPosition;
			var itemLength = m_uiContent.ItemSize + m_uiContent.ItemSpaceLength;
			if (m_uiContent.m_DragDirection == DragDirection.Horizontal)
			{
				var currentPosX = afterDragPagePos.x; //当前拖动的位置  负
				currentPosX -= m_uiContent.Margin;
				int count = (int)(Mathf.Abs(currentPosX) / m_uiContent.ItemSize);
				if (m_uiContent.m_ScorllRect.horizontalNormalizedPosition <= 0)
				{
					return;
				}
				else if (m_uiContent.m_ScorllRect.horizontalNormalizedPosition >= 1)  //总数-当前显示的数量
				{
					return;
				}

				var targetPosX = -(float)(count * itemLength);
				if (((float)(targetPosX + halfItemLength)) < Mathf.Abs(currentPosX))
				{
					count++;
					targetPosX = -(float)(count * itemLength);
				}
				this.contentRectTf.DOLocalMoveX(targetPosX, .2f);
			}
			else
			{
				var currentPosY = afterDragPagePos.y; //当前拖动的位置  正
				currentPosY -= m_uiContent.Margin;
				int count = (int)(Mathf.Abs(currentPosY) / itemLength);
				if (m_uiContent.m_ScorllRect.verticalNormalizedPosition <= 0)
				{
					return;
				}
				else if (m_uiContent.m_ScorllRect.verticalNormalizedPosition >= 1)  //总数-当前显示的数量
				{
					return;
				}

				var targetPosY = (float)(count * itemLength);
				if (((float)(targetPosY + halfItemLength)) < Mathf.Abs(currentPosY))
				{
					count++;
					targetPosY = (float)(count * itemLength);
				}
				this.contentRectTf.DOLocalMoveY(targetPosY, .2f);
			}
		}
	}
}

```