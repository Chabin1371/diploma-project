using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    [Header("设置")]
    public Image backgroundPrefab;          // 背景预制体（需锚点居中）
    public int copyCount = 3;                // 副本数量（3张即可）
    public float speed = 200f;                // 滚动速度（像素/秒）

    private List<RectTransform> images = new List<RectTransform>();
    private float imageWidth;                  // 图片本地宽度
    private RectTransform canvasRect;           // 父Canvas的RectTransform（用于获取屏幕尺寸）

    void Start()
    {
        // 获取父Canvas的RectTransform
        canvasRect = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        if (canvasRect == null)
        {
            Debug.LogError("未找到父Canvas的RectTransform！");
            return;
        }

        // 实例化图片
        for (int i = 0; i < copyCount; i++)
        {
            Image img = Instantiate(backgroundPrefab, transform);
            images.Add(img.rectTransform);
        }

        // 获取图片宽度（所有图片相同）
        imageWidth = images[0].rect.width;

        // 初始化位置：使图片从左到右无缝覆盖屏幕
        float screenWidth = canvasRect.rect.width;
        // 最左边图片的中心坐标（以父物体中心为原点）
        float startX = -screenWidth * 0.5f + imageWidth * 0.5f;

        for (int i = 0; i < copyCount; i++)
        {
            float x = startX + i * imageWidth;
            images[i].anchoredPosition = new Vector2(x, 0);
        }
    }

    void Update()
    {
        // 所有图片向左移动
        float deltaX = -speed * Time.deltaTime;
        foreach (RectTransform img in images)
        {
            Vector2 pos = img.anchoredPosition;
            pos.x += deltaX;
            img.anchoredPosition = pos;
        }

        // 计算屏幕左右边界（本地坐标）
        float screenLeft = -canvasRect.rect.width * 0.5f;
        float screenRight = canvasRect.rect.width * 0.5f;

        // 找出当前所有图片的右边缘最大坐标（即最右边图片的右边缘）
        float maxRight = float.MinValue;
        foreach (RectTransform img in images)
        {
            float rightEdge = img.anchoredPosition.x + imageWidth * 0.5f;
            if (rightEdge > maxRight)
                maxRight = rightEdge;
        }

        // 遍历图片，将完全移出屏幕左侧的图片重新放置到最右边
        foreach (RectTransform img in images)
        {
            float rightEdge = img.anchoredPosition.x + imageWidth * 0.5f;
            // 如果图片的右边缘已经完全移出屏幕左侧
            if (rightEdge < screenLeft)
            {
                // 将它放到当前最右边图片的右侧
                float newX = maxRight + imageWidth * 0.5f; // 新图片左边缘紧挨最右图片右边缘
                img.anchoredPosition = new Vector2(newX, 0);
                // 更新maxRight为新的最右边缘
                maxRight = newX + imageWidth * 0.5f;
            }
        }
    }
}
