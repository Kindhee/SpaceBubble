using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game Instance { get; private set; }

    [SerializeField]
    GameObject bomb_preview;

    [SerializeField]
    GameObject ball_prefab;

    [SerializeField]
    Vector2 start_grid;

    [SerializeField]
    int height_number;

    [SerializeField]
    int width_number;

    [SerializeField]
    int width_space;

    [SerializeField]
    int height_space;

    [SerializeField]
    int treshold_decend;

    [SerializeField]
    int treshold_bomb;

    int shot_count;

    int descend_step;

    int kill_count;

    bool bomb_active;

    public List<List<GameObject>> grid_ball;

    static readonly Vector2Int[] directions = new Vector2Int[]
    {
    new Vector2Int(0, 1),
    new Vector2Int(0, -1),
    new Vector2Int(-1, 0),
    new Vector2Int(1, 0),
    new Vector2Int(1, 1),
    new Vector2Int(-1, -1),
    new Vector2Int(1, -1),
    new Vector2Int(-1, 1)
    };

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        shot_count = 0;
        kill_count = 0;
        descend_step = 1;

        bomb_active = false;
        SetBomb(false);

        InitializeGrid();
        FillGrid();
    }

    void Update()
    {
        if(kill_count >= treshold_bomb)
        {
            SetBomb(true);
            kill_count = 0;
        }

        if (CheckForWin())
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    private bool CheckForWin()
    {
        for (int i = 0; i < height_number; i++)
        {
            for (int j = 0; j < width_number; j++)
            {
                if (grid_ball[i][j] != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void SetBomb(bool statut)
    {
        bomb_preview.SetActive(statut);
        bomb_active = statut;
    }

    public bool IsBombActive()
    {
        return bomb_active;
    }

    void InitializeGrid()
    {
        grid_ball = new List<List<GameObject>>();

        for (int i = 0; i < height_number; i++)
        {
            List<GameObject> row = new List<GameObject>();
            for (int j = 0; j < width_number; j++)
            {
                row.Add(null);
            }
            grid_ball.Add(row);
        }
    }

    void FillTopRow()
    {
        for (int j = 0; j < width_number; j++)
        {
            Vector3 pos = new Vector3(start_grid.x + j * width_space, start_grid.y * height_space, 0);
            GameObject ball_temp = Instantiate(ball_prefab, pos, Quaternion.identity);
            PlaceBallAtIndex(ball_temp, new Vector2(j, 0), false);
        }
    }

    void FillGrid()
    {
        int rowsToFill = Mathf.Max(0, height_number - 10); 

        for (int y = 0; y < rowsToFill; y++)
        {
            for (int x = 0; x < width_number; x++)
            {
                Vector3 pos = new Vector3(start_grid.x + x * width_space, start_grid.y - y * height_space, 0);
                GameObject ball_temp = Instantiate(ball_prefab, pos, Quaternion.identity);
                PlaceBallAtIndex(ball_temp, new Vector2(x, y), false);
            }
        }
    }


    public bool PlaceBallAtIndex(GameObject ball, Vector2 index, bool shot)
    {

        int x = (int)index.x;
        int y = (int)index.y;

        if (y < 0 || y >= grid_ball.Count || x < 0 || x >= grid_ball[y].Count)
            return false;

        if (grid_ball[y][x] != null)
            return false;

        Rigidbody rb = ball.GetComponentInChildren<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }


        var ball_script = ball.GetComponentInChildren<Ball>();
        ball_script.SetIndex(index);
        ball_script.is_placed = true;

        Vector3 new_pos = new Vector3(start_grid.x + x * width_space, start_grid.y - y * height_space, 0);
        ball.transform.position = new_pos;

        grid_ball[y][x] = ball;

        if (shot)
        {
            string color_target = grid_ball[y][x].GetComponentInChildren<Ball>().GetColor();

            int destroyed = CheckForNearbyColor(index, color_target);

            if (destroyed > 0)
            {
                int exponent = destroyed - 3;

                Score.Instance.AddScore(ball_script.GetPoints(), exponent);
            }

            AddShot();

            BringUpDisconnectedBalls();

            if (shot_count >= treshold_decend)
            {
                StartCoroutine(DelayedDescend());
            }
        }

        return true;
    }

    IEnumerator DelayedDescend()
    {
        shot_count = 0;

        yield return new WaitForEndOfFrame();

        DescendGrid();
        FillTopRow();

    }

    public void DescendGrid()
    {
        descend_step++;

        for (int y = height_number - 2; y >= 0; y--)
        {
            for (int x = 0; x < width_number; x++)
            {
                GameObject ball = grid_ball[y][x];

                if (ball != null)
                {
                    Vector3 new_pos = new Vector3(
                        start_grid.x + x * width_space,
                        start_grid.y - (y + 1) * height_space,
                        ball.transform.position.z
                    );

                    ball.transform.position = new_pos;

                    grid_ball[y + 1][x] = ball;
                    grid_ball[y][x] = null;

                    Ball ballScript = ball.GetComponentInChildren<Ball>();
                    if (ballScript != null)
                    {
                        ballScript.SetIndex(new Vector2(x, y + 1));
                    }
                }
            }
        }
    }

    public void BringUpDisconnectedBalls()
    {
        bool anyMoved;

        do
        {
            anyMoved = false;

            for (int y = descend_step; y < height_number; y++)
            {
                for (int x = 0; x < width_number; x++)
                {
                    GameObject ball = grid_ball[y][x];

                    if (ball != null)
                    {
                        int targetY = y - 1;

                        if (targetY >= 0 && grid_ball[targetY][x] == null)
                        {
                            Vector3 new_pos = new Vector3(
                                start_grid.x + x * width_space,
                                start_grid.y - targetY * height_space,
                                ball.transform.position.z
                            );

                            ball.transform.position = new_pos;

                            grid_ball[targetY][x] = ball;
                            grid_ball[y][x] = null;

                            Ball ballScript = ball.GetComponentInChildren<Ball>();
                            if (ballScript != null)
                            {
                                ballScript.SetIndex(new Vector2(x, targetY));
                            }

                            anyMoved = true;
                        }
                    }
                }
            }

        } while (anyMoved);
    }



    int CheckForNearbyColor(Vector2 index, string color_target)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        List<Vector2Int> matchingBalls = new List<Vector2Int>();

        FindConnectedBalls((int)index.x, (int)index.y, color_target, visited, matchingBalls);

        if (matchingBalls.Count > 2)
        {
            foreach (var pos in matchingBalls)
            {
                int x = pos.x;
                int y = pos.y;

                Ball b = grid_ball[y][x]?.GetComponentInChildren<Ball>();
                if (b != null)
                {
                    b.Kill();


                    if (!bomb_active)
                    {
                        kill_count++;
                    }

                    grid_ball[y][x] = null;
                }
            }
            return matchingBalls.Count;
        }
        return 0;
    }

    void FindConnectedBalls(int x, int y, string targetColor, HashSet<Vector2Int> visited, List<Vector2Int> result)
    {
        Vector2Int current = new Vector2Int(x, y);

        if (visited.Contains(current))
            return;

        if (y < 0 || y >= grid_ball.Count || x < 0 || x >= grid_ball[y].Count)
            return;

        GameObject obj = grid_ball[y][x];
        if (obj == null)
            return;

        Ball ball = obj.GetComponentInChildren<Ball>();

        if (targetColor != null)
        {
            if (ball == null || ball.GetColor() != targetColor)
                return;
        }

        visited.Add(current);
        result.Add(current);

        foreach (var dir in directions)
        {
            FindConnectedBalls(x + dir.x, y + dir.y, targetColor, visited, result);
        }
    }

    void AddShot()
    {
        shot_count++;
    }

    public void Explode(Vector2 target)
    {
        int x = (int)target.x;
        int y = (int)target.y;

        TryKillBallAt(x, y);

        foreach (var dir in directions)
        {
            for (int i = 1; i <= 2; i++)
            {
                int newX = x + dir.x * i;
                int newY = y + dir.y * i;

                TryKillBallAt(newX, newY);
            }
        }

        BringUpDisconnectedBalls();

        StartCoroutine(DelayedDescend());
    }


    private void TryKillBallAt(int x, int y)
    {
        if (y >= 0 && y < grid_ball.Count &&
            x >= 0 && x < grid_ball[y].Count &&
            grid_ball[y][x] != null)
        {
            Ball ball = grid_ball[y][x].GetComponentInChildren<Ball>();
            if (ball != null)
            {
                ball.Kill();

            }
            grid_ball[y][x] = null;
        }
    }

}
