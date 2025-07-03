using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms;
public enum State
{
    Idle = 0,
    Walk,
    //Run,
    Reaction1,
    Reaction2,
    Reaction3
};
public class ObjectController : MonoBehaviour
{

    public State state;
    [SerializeField]
    private BoxCollider col;
    [SerializeField]
    private Animator animator;
    private Vector3 targetPosition;
    public int MotionDelay =3;
    private float remainingTime;
    private void Start()
    {
        StartCoroutine(BehaviorLoop());
    }
    private void Update()
    {
        if (state != State.Walk) return;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5);
        transform.LookAt(targetPosition);
        float distance = Vector3.Distance(transform.position, targetPosition);
        if(distance < 0.1f)
        {
            SetState(State.Idle);
        }
    }
    private void PlayAction(State state)
    {
        switch (state)
        {
            case State.Idle:
              
                break;
            case State.Walk:
                SetState(State.Walk);
                animator.SetBool("isWalking", true);
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5);
                transform.LookAt(targetPosition);
                break;
            case State.Reaction1:
                SetState(State.Reaction1);
                animator.SetTrigger("Reaction1");
                break;
            case State.Reaction2:
                SetState(State.Reaction2);
                animator.SetTrigger("Reaction2");
                break;
            case State.Reaction3:
                SetState(State.Reaction3);
                animator.SetTrigger("Reaction3");

                break;
        }



    }
    public void SetState(State state)
    {
        this.state = state;
        switch (state)
        {
            case State.Idle:
                state = State.Idle;
                break;
            case State.Walk:
                state = State.Walk;
                animator.SetBool("isWalking", true);
              
                break;
            case State.Reaction1:
                state = State.Reaction1;
                animator.SetTrigger("Reaction1");
                break;
            case State.Reaction2:
                state = State.Reaction2;
                animator.SetTrigger("Reaction2");
                break;
            case State.Reaction3:
                state = State.Reaction3;
                animator.SetTrigger("Reaction3");

                break;
        }
    }
    IEnumerator BehaviorLoop()
    {
        while (true)
        {


            remainingTime += Time.deltaTime;


            if (state == State.Idle && remainingTime > MotionDelay)
            {
                Debug.Log("¸ð¼Çµô·¹ÀÌ¼³Á¤");
                MotionDelay = Random.Range(5, 10);
                Debug.Log("·£´ýÀÎµ¦½º");
                int randomIndex = Random.Range(1, 4);
                if (randomIndex == 1)
                {
                    //walkÄÚµå
                    targetPosition = SetNewRandomDestination();
                    SetState((State)randomIndex);
                }
                else
                {
                    SetState((State)randomIndex);
                }
                remainingTime = 0;

            }



            yield return null;
        }
    }
    private Vector3 SetNewRandomDestination()
    {
        Bounds bounds = col.bounds;
        float randX = Random.Range(bounds.min.x, bounds.max.x);
        float randZ = Random.Range(bounds.min.z, bounds.max.z);
        targetPosition = new Vector3(randX, transform.position.y, randZ);

        return targetPosition;
    }
}
