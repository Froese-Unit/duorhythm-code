using DG.Tweening;
using UnityEngine;

public class DOTweenSimpleTest : MonoBehaviour
{
    public RectTransform targetUIElement; // Assign a UI element from your scene

    void Start()
    {
        if (targetUIElement != null)
        {
            targetUIElement.DOScale(1.5f, 1.0f).SetEase(Ease.InOutBounce).SetLoops(-1, LoopType.Yoyo);
            Debug.Log("Simple DOTween animation started.");
        }
        else
        {
            Debug.LogError("Target UI element is not assigned.");
        }
    }
}
