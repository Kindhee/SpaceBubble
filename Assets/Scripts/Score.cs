using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class Score : MonoBehaviour
{
    public static Score Instance { get; private set; }

    [SerializeField]
    TextMeshProUGUI score_text;

    [SerializeField]
    GameObject canvas_placeholder;

    [SerializeField]
    GameObject prefab_popup;

    GameObject last_popup;
    CanvasGroup group_popup;

    int malus = 1;

    float visibleDuration = 2.0f;
    float fadeDuration = 1.0f;

    int score_amount;
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        score_amount = 0;
    }
    void Update()
    {
        score_text.text = score_amount.ToString();
    }

    public void AddScore(int amount, int exponent)
    {
        if (last_popup != null)
        {
            Destroy(last_popup);
        }

        int to_add = (int)(amount * Mathf.Pow(2, exponent));

        score_amount += to_add;

        last_popup = Instantiate(prefab_popup, canvas_placeholder.transform);

        last_popup.GetComponentInChildren<TextMeshProUGUI>().text = "+ " + to_add.ToString();

        group_popup = last_popup.GetComponentInChildren<CanvasGroup>();

        StartCoroutine(Fade());
    }

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(visibleDuration);

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            if (group_popup == null || group_popup.gameObject == null)
                yield break; 

            elapsed += Time.deltaTime;
            group_popup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        if (group_popup != null && group_popup.gameObject != null)
        {
            group_popup.alpha = 0f;
            Destroy(group_popup.gameObject);
        }
    }


    public void SubstractScore(int amount)
    {
        if (last_popup != null)
        {
            Destroy(last_popup);
        }

        int to_substract = amount * malus;

        score_amount = Math.Max(0, score_amount - to_substract);

        malus++;

        if (score_amount > 0)
        {
            last_popup = Instantiate(prefab_popup, canvas_placeholder.transform);

            last_popup.GetComponentInChildren<TextMeshProUGUI>().text = "- " + to_substract.ToString();

            group_popup = last_popup.GetComponentInChildren<CanvasGroup>();

            StartCoroutine(Fade());
        }
    }
}

