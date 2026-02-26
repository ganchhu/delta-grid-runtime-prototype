using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Card : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Image image;

    public CardData Data { get; private set; }

    public bool IsFaceUp { get; private set; }
    public bool IsMatched { get; private set; }
    [SerializeField] private Button button;

    private Action<Card> onSelected;
    private bool isFlipping;

    private Sprite frontSprite;
    private Sprite backSprite;

    private const float flipDuration = 0.2f;

    private void Awake()
    {
        button=GetComponent<Button>();
    }
    public void Init(CardData data, Sprite back, Action<Card> callback)
    {
        Data = data;
        frontSprite = data.frontSprite;
        backSprite = back;

        onSelected = callback;

        IsFaceUp = false;
        IsMatched = false;
        isFlipping = false;

        image.sprite = backSprite;
        transform.localScale = Vector3.one;
    }

    
    public void OnClick()
    {
        if (IsMatched || IsFaceUp || isFlipping) return;

        onSelected?.Invoke(this);
    }


    public void FlipUp()
    {
        if (IsFaceUp) return;
        AudioManager.Instance.Play(AudioManager.SFX.Flip);
        StartCoroutine(Flip(frontSprite));
    }

    public void FlipDown()
    {
        if (!IsFaceUp) return;
        AudioManager.Instance.Play(AudioManager.SFX.Flip);
        StartCoroutine(Flip(backSprite));
    }

 

    private IEnumerator Flip(Sprite newSprite)
    {
        isFlipping = true;

        float timer = 0f;

        // Shrink
        while (timer < flipDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(1f, 0f, timer / flipDuration);
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        image.sprite = newSprite;

        IsFaceUp = !IsFaceUp;

        timer = 0f;


        while (timer < flipDuration)
        {
            timer += Time.deltaTime;
            float scale = Mathf.Lerp(0f, 1f, timer / flipDuration);
            transform.localScale = new Vector3(scale, 1f, 1f);
            yield return null;
        }

        transform.localScale = Vector3.one;
        isFlipping = false;
    }

   

    public void ResetCard()
    {
        StopAllCoroutines();
        transform.localScale = Vector3.one;
        image.sprite = backSprite;

        IsFaceUp = false;
        IsMatched = false;
        isFlipping = false;
    }

    public void SetMatched()
    {
        if (IsMatched) return;

        IsMatched = true;
        StartCoroutine(MatchSequence());
    }
    private IEnumerator MatchSequence()
    {
        yield return new WaitForSeconds(0.2f);
        AudioManager.Instance?.Play(AudioManager.SFX.Match);
        yield return PopEffect();
        yield return Disappear();
    }
    private IEnumerator PopEffect()
    {
        float duration = 0.15f;
        float timer = 0f;

        Vector3 start = transform.localScale;
        Vector3 target = start * 1.2f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(start, target, timer / duration);
            yield return null;
        }

        timer = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            transform.localScale = Vector3.Lerp(target, start, timer / duration);
            yield return null;
        }
    }
    private IEnumerator Disappear()
    {
        float duration = 0.25f;
        float timer = 0f;

        Vector3 startScale = transform.localScale;
        Color startColor = image.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            image.color = Color.Lerp(startColor, Color.clear, t);

            yield return null;
        }

        gameObject.SetActive(false);
        button.interactable = false;
    }
     

    public void SetInteractable(bool value)
    {
        button.interactable = value;
    }

    
}



[System.Serializable]
public class CardData
{
    public int id;
    public Sprite frontSprite;
}
