using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Drawing;
using System.Linq;

public class Colors : MonoBehaviour
{
    public static Colors Instance { get; private set; }

    [SerializeField]
    List<Material> colors;

    [SerializeField]
    List<GameObject> balls_preview;

    List<Material> random_colors = new List<Material>();

    int preview_index;
    int spawn_index;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        preview_index = 0;
        spawn_index = 0;

        CreateRandomColors();
    }

    private void Update()
    {
        int i = 0;
        foreach (var ball in balls_preview)
        {
            ball.GetComponent<Renderer>().material = GetPreviewNextColor(i);
            i++;
        }
    }

    public Material GetColorForSimulation()
    {
        int wrappedIndex = ((spawn_index % random_colors.Count) + random_colors.Count) % random_colors.Count;
        Material result = random_colors[wrappedIndex];

        spawn_index++;
        return result;
    }

    public Material GetRandomColor()
    {
        int wrappedIndex = ((preview_index % random_colors.Count) + random_colors.Count) % random_colors.Count;
        Material result = random_colors[wrappedIndex];

        preview_index++;
        spawn_index++;

        return result;
    }

    public Material GetPreviewNextColor(int step)
    {
        int wrappedIndex = (((preview_index + step) % random_colors.Count) + random_colors.Count) % random_colors.Count;
        return random_colors[wrappedIndex];
    }

    void CreateRandomColors()
    {
        random_colors.Clear();

        for (int i = 0; i < 999; i++)
        {
            Material newColor;
            int attempts = 0;
            do
            {
                int random_int = UnityEngine.Random.Range(0, colors.Count);
                newColor = colors[random_int];
                attempts++;

                if (attempts > 10) break;

            } while (IsSameAsLastThree(newColor));

            random_colors.Add(newColor);
        }
    }

    bool IsSameAsLastThree(Material newColor)
    {
        if (random_colors.Count < 3) return false;

        return
            random_colors[random_colors.Count - 1] == newColor &&
            random_colors[random_colors.Count - 2] == newColor &&
            random_colors[random_colors.Count - 3] == newColor;
    }
}