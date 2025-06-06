using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class Ball : MonoBehaviour
{
    [SerializeField]
    float speed;

    [SerializeField]
    int reward_points;

    [SerializeField]
    GameObject particle_prefab;

    [SerializeField]
    int collided = 0;

    [SerializeField]
    public bool is_bomb { get; set; } = false;

    int x, y;

    //Vector3 dir;

    private bool has_hit_wall { get; set; } = false;

    public bool is_placed { get; set; } = false;

    public bool is_shot { get; set; } = false;

    string color_name;

    void Start()
    {
        if (!is_bomb)
        {
            ApplyRandomColor();
        }
    }

    void Update()
    {

    }

    public void SetIsBomb(bool statut)
    {
        is_bomb = statut;
    }

    void ApplyRandomColor()
    {
        Renderer renderer = GetComponent<Renderer>();

        Material new_material;

        if (is_shot)
        {
            new_material = Colors.Instance.GetRandomColor();
        }
        else
        {
            new_material = Colors.Instance.GetColorForSimulation();
        }

        if (renderer != null && new_material != null)
        {
            renderer.material = new_material;
            color_name = new_material.name;
        }
    }

    public void LaunchAtDirection(Vector3 direction)
    {
        //dir = direction;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            direction = direction.normalized;
            rb.velocity = direction * speed;
        }
    }

    public void SetIndex(Vector2 pos)
    {
        x = (int)pos.x;
        y = (int)pos.y;
    }
    public Vector2 GetIndex()
    {
        return new Vector2(x, y);
    }

    public string GetColor()
    {
        return color_name;
    }

    public int GetPoints()
    {
        return reward_points;
    }

    public void Kill()
    {
        GameObject particle_temp = Instantiate(particle_prefab, transform.position, Quaternion.identity);
        ParticleSystem particle_ref = particle_temp.GetComponentInChildren<ParticleSystem>();

        var main = particle_ref.main;

        float duration = main.duration;

        Destroy(particle_temp, duration);

        if (!is_bomb)
        {
            string material_name = GetComponent<Renderer>().material.name;

            if (material_name.Contains(" (Instance)"))
                material_name = material_name.Replace(" (Instance)", "");

            Color particle_color = Color.white;

            switch (material_name)
            {
                case "Red":
                    particle_color = Color.red;
                    break;
                case "Green":
                    particle_color = Color.green;
                    break;
                case "Blue":
                    particle_color = Color.blue;
                    break;
                case "Cyan":
                    particle_color = Color.cyan;
                    break;
                case "Yellow":
                    particle_color = Color.yellow;
                    break;
                case "Pink":
                    particle_color = Color.magenta;
                    break;
                default:
                    break;

            }

            main.startColor = particle_color;

        }

        Destroy(transform.parent.gameObject);
    }


    public void Collide(Collider other, int x, int y)
    {
        if (!is_placed)
        {
            Ball other_ball = other.GetComponentInChildren<Ball>();

            if (other_ball == null)
            {
                Debug.Log(other.name);
            }
            Vector2 other_index = other_ball.GetIndex();

            if (!is_bomb)
            {

                if (other_ball != null)
                {

                    Vector2 target_index = new Vector2(other_index.x + x, other_index.y + y);

                    bool valid = Game.Instance.PlaceBallAtIndex(gameObject, target_index, true);

                    collided++;

                    if (!valid && collided >= 3)
                    {
                        Kill();
                    } 
                }
            }
            else {
                if (other_ball.is_placed)
                {
                    Game.Instance.Explode(other_index);
                    Kill();
                }
            }
        }
    }

    public void Wall(Collider other)
    {
        if (has_hit_wall) return;
        has_hit_wall = true;

        foreach (var collider in GetComponentsInChildren<SphereCollider>())
        {
            collider.enabled = false;
        }

        Score.Instance.SubstractScore(reward_points);
        Kill();
    }
}
    
