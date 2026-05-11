using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipPanel : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private Text toolNameText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("显示设置")]
    [SerializeField] private float displayDuration = 1.5f;
    [SerializeField] private float fadeDuration = 0.2f;

    private Coroutine autoHideCoroutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 显示工具名称弹窗
    /// </summary>
    public void Show(string itemName)
    {
        toolNameText.text = itemName;
        gameObject.SetActive(true);

        // 重置透明度
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;

        // 重启自动隐藏协程
        if (autoHideCoroutine != null)
            StopCoroutine(autoHideCoroutine);
        autoHideCoroutine = StartCoroutine(AutoHideWithFade());
    }

    private IEnumerator AutoHideWithFade()
    {
        // 等待显示时长
        yield return new WaitForSeconds(displayDuration);

        // 渐隐效果
        if (canvasGroup != null && fadeDuration > 0)
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// 立即隐藏弹窗
    /// </summary>
    public void Hide()
    {
        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }
        gameObject.SetActive(false);
    }
}
