using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum HorseColor
{
    Green,
    White,
    Blue,
    Brown
}


[Serializable]
public struct Player
{
    public HorseColor color;
    public int id;
}

public class HorseController : MonoBehaviour
{
    public Player playerProperties = new Player();

    public RaceManager raceManager;

    public WayPointList waypoints;
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    public int index = 0;

    public int totalLap = 1;

    //4-6
    public float _maxSpeed = 5f;
    public float _minSpeed = 2f;
    //0.1-1 is normal
    public float _acceleration = 2f;
    public float currentSpeed = 0;

    public Transform _target;

    public bool _canMove;
    public bool _running;
    public bool _raceFinished;

    float accelerationInput = 0f;

    private int  RunStraight;
    private int RunDownHash;
    private int RunUpHash;
    private int IdleHash;

/*    private Dictionary<string, int> tagToBoolMap = new Dictionary<string, int>
{
    { "Start", RunStraight },
    { "First", RunDownHash },
    { "Second", RunStraight },
    { "Third", RunUpHash },
    { "Fourth", RunStraight }
};*/

    private void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        RunStraight = Animator.StringToHash("RunStraight");
        RunDownHash = Animator.StringToHash("RunDown");
        RunUpHash = Animator.StringToHash("RunUp");
        IdleHash = Animator.StringToHash("Idle");
        
        //Set to Idle
        animator.SetTrigger(IdleHash);
    }

    public void StartRace()
    {
        StartCoroutine(Move());
        animator.SetBool(RunStraight, true);
    }

    IEnumerator Move()
    {
        while (_canMove && index < waypoints._wayPoint.Length)
        {
            _target = waypoints._wayPoint[index];
            if (Vector3.Distance(transform.position, _target.position) > 0.1f)
            {
                Vector3 direction = _target.position - transform.position;

                // Calculate the angle in radians
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Set the rotation of the object to look at the target
                //transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                // Gradually increase speed using lerp
                currentSpeed = Mathf.Lerp(currentSpeed, _maxSpeed, Time.deltaTime * _acceleration);

                // Move towards the target
                float step = currentSpeed * Time.deltaTime;

                //Debug.Log($"Running {step}");

                transform.position = Vector3.MoveTowards(transform.position, _target.position, step);
            }
            else if (Vector3.Distance(transform.position, _target.position) <= 0.1f)
            {
                //Check for next lap
                index++;

                //Last checkpoint and index gone out of bounds then 
                // 1. Decrease total lap
                // 2. Total lap is non negative then restart by index = 0
                if (index > waypoints._wayPoint.Length - 1)
                {
                    if (totalLap != 0)
                    {
                        totalLap--;
                    }
                    if (totalLap > 0)
                    {
                        index = 0;
                    }
                    else if (totalLap <= 0 && index > waypoints._wayPoint.Length - 1)
                    {
                        animator.SetBool(IdleHash, true);
                        animator.SetBool(RunStraight, false);
                        animator.SetBool(RunDownHash, false);
                        animator.SetBool(RunUpHash, false);
                        _running = false;
                        _raceFinished = true;
                        raceManager.horses.Push(this);
                        raceManager.PrintStack();
                        yield break;
                    }
                    Debug.Log($"Checking for last lap index {index}");
                }
                currentSpeed = _minSpeed; // Reset speed when reaching a new waypoint
            }
            _running = true;
            yield return null;
        }
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*        if (tagToBoolMap.TryGetValue(collision.gameObject.tag, out int boolParameter))
                {

                    animator.SetBool(boolParameter, true);

                    // Additional logic for specific cases
                    if (boolParameter == RunStraight)
                    {
                        if (collision.gameObject.CompareTag("Second"))
                        {
                            spriteRenderer.flipX = true;
                        }
                        else if (collision.gameObject.CompareTag("Fourth"))
                        {
                            spriteRenderer.flipX = false;
                        }
                    }
                }*/

        Debug.Log($"Collision {collision.gameObject.tag}");
        if (collision.gameObject.CompareTag("Start"))
        {
            animator.SetBool(RunStraight, true);
            animator.SetBool(RunDownHash, false);
            animator.SetBool(RunUpHash, false);
            animator.SetBool(IdleHash, false);
        }
        else if (collision.gameObject.CompareTag("First"))
        {
            animator.SetBool(RunDownHash, true);
            animator.SetBool(RunStraight, false);
            animator.SetBool(RunUpHash, false);
            animator.SetBool(IdleHash, false);
        }
        else if (collision.gameObject.CompareTag("Second"))
        {
            animator.SetBool(RunStraight, true);
            animator.SetBool(RunDownHash, false);
            animator.SetBool(RunUpHash, false);
            animator.SetBool(IdleHash, false);
            spriteRenderer.flipX = true;
        }
        else if (collision.gameObject.CompareTag("Third"))
        {
            animator.SetBool(RunUpHash, true);
            animator.SetBool(RunDownHash, false);
            animator.SetBool(RunStraight, false);
            animator.SetBool(IdleHash, false);
        }
        else if (collision.gameObject.CompareTag("Fourth"))
        {
            animator.SetBool(RunStraight, true);
            animator.SetBool(RunUpHash, false);
            animator.SetBool(RunDownHash, false);
            animator.SetBool(IdleHash, false);
            spriteRenderer.flipX = false;
        }
    }
}
