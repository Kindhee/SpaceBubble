using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Canon : MonoBehaviour
{

    [SerializeField]
    GameObject ball_prefab;

    [SerializeField]
    GameObject bomb_prefab;

    void Start()
    {
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.forward, transform.position);
        float distance;

        if (plane.Raycast(ray, out distance))
        {
            Vector3 hit_point = ray.GetPoint(distance);
            Quaternion hit_rotation = Quaternion.identity;
            Vector3 direction = hit_point - transform.position;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                GameObject ball_temp = Instantiate(ball_prefab, transform.position, hit_rotation);

                if (ball_temp != null)
                {
                    Ball ball_ref = ball_temp.GetComponentInChildren<Ball>();

                    ball_ref.is_shot = true;
                    ball_ref.LaunchAtDirection(direction);
                }
            }

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                if (Game.Instance.IsBombActive())
                {
                    GameObject bomb_temp = Instantiate(bomb_prefab, transform.position, hit_rotation);                  

                    if (bomb_temp != null)
                    {
                        Ball bomb_ref = bomb_temp.GetComponentInChildren<Ball>();

                        bomb_ref.SetIsBomb(true);
                        bomb_ref.LaunchAtDirection(direction);
                    }

                    Game.Instance.SetBomb(false);
                }
            }
        }
    }
}
