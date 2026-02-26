using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class UIButtonFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Scale Settings")]
    [SerializeField] private float hoverScale = 1.08f;
    [SerializeField] private float clickScale = 0.92f;
    [SerializeField] private float animationSpeed = 10f;

    [Header("Sound Settings")]
    [SerializeField] private bool playHoverSound = true;
    [SerializeField] private bool playClickSound = true;

    private Vector3 originalScale;
    private Coroutine scaleRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        AnimateScale(originalScale * hoverScale);
        AudioManager.Instance?.PlayInteraction(6);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        AnimateScale(originalScale);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playClickSound)
            AudioManager.Instance?.PlayInteraction(6); // or create ButtonClick enum

       // StartCoroutine(ClickBounce());
    }

    private void AnimateScale(Vector3 target)
    {
        if (scaleRoutine != null)
            StopCoroutine(scaleRoutine);

        scaleRoutine = StartCoroutine(ScaleTo(target));
    }

    private IEnumerator ScaleTo(Vector3 target)
    {
        while (Vector3.Distance(transform.localScale, target) > 0.01f)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, target, Time.deltaTime * animationSpeed);
            yield return null;
        }

        transform.localScale = target;
    }

    private IEnumerator ClickBounce()
    {
        AnimateScale(originalScale * clickScale);
        yield return new WaitForSeconds(0.08f);
        AnimateScale(originalScale * hoverScale);
    }
}