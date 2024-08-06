
using System;
using System.Collections;
using TMPro;
using UnityEngine;



public class PlayerHit : MonoBehaviour
{
    private int health = 100;
    private int resistance = 1;
    public bool pan = false;
    public bool buff = false;
    public TextMeshProUGUI healthTxt;
    private void Update()
    {
        healthTxt.text = "Health: " + health.ToString();
        if (health < 0)
        {
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collided with: " + other.gameObject.tag);
        switch (other.gameObject.tag)
        {
            case "Potion":
                Potion();
                break;
            case "Garlic":
                Garlic();
                break;
            case "Pan":
                Pan();
                break;
            case "Donut":
                Donut();
                break;
            case "Coffee":
                Coffee();
                break;
            case "Cake":
                Cake();
                break;
            case "EnemyBullet":
                EnemyBullet(other);
                break;
            default:

                break;
        }
    }
    private void OnCollisionEnter(Collision collision)
    {

        switch (collision.gameObject.tag)
        {
            case "Enemy1":
                Enemy1();
                break;
            case "Enemy2":
                Enemy2();
                break;
            default:
                break;
        }
    }
    void Potion()
    {
        Transform childTransform = transform.Find("Resistance");
        if (childTransform != null)
        {
            childTransform.gameObject.SetActive(true);
            resistance = 2;
            Action resetResistance = () =>
            {
                childTransform.gameObject.SetActive(false);
                resistance = 1;
            };
            StartCoroutine(ExecuteLater(10, resetResistance));
        }

    }
    void Garlic()
    {
        GameObject enemy = GameObject.FindGameObjectWithTag("Enemy1");

        if (enemy != null)
        {
            Enemy enemyScript = enemy.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                // Set the state of the enemy to "Flee"
                enemyScript.currentState = Enemy.NPCStates.Patrol;
            }
            else
            {
                Debug.LogError("Enemy script not found on the GameObject");
            }
        }
        else
        {
            Debug.LogError("No GameObject found with tag 'Enemy1'");
        }
    }
    void Pan()
    {


        pan = true;
        Action resetResistance = () =>
        {
            pan = false;
        };
        StartCoroutine(ExecuteLater(10, resetResistance));


    }
    void Donut()
    {
        health = health >= 50 ? 100 : health + 50;
    }
    void Cake() {

        buff = true;
            Action resetResistance = () =>
            {
                buff = false;
            };
            StartCoroutine(ExecuteLater(10, resetResistance));
        

    }
    void Coffee()
    {
        PlayerController playerController = GetComponent<PlayerController>();

        if (playerController != null)
        {
            playerController.speed = 15;

            Action resetSpeedAction = () =>

            {
                playerController.speed = 10;

            };
            StartCoroutine(ExecuteLater(10, resetSpeedAction));
        }
    }
    void EnemyBullet(Collider bulletCollider)
    {
        EnemyBulletController bullet = bulletCollider.GetComponent<EnemyBulletController>();
        if (bullet != null)
        {
            health -= bullet.damage / resistance;
        }
    }
    void Enemy1()
    {

        health -= 10 / resistance;
    }
    void Enemy2()
    {
        health -= 50 / resistance;
    }
    IEnumerator ExecuteLater(float delay, Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }



}
