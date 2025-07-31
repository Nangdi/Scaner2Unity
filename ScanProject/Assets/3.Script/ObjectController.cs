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
    private OffsetTuner offsetTuner;
    [SerializeField]
    private BoxCollider col;
    [SerializeField]
    private Animator animator;
    public Transform namePos;
    private Vector3 targetPosition;
    public int MotionDelay =3;
    private float remainingTime;
    Vector3 CameraDir = new Vector3();
    private void Start()
    {
        offsetTuner.go = gameObject;
        StartCoroutine(BehaviorLoop());
        CameraDir = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);
        col = GameObject.FindGameObjectWithTag("bounds").GetComponent<BoxCollider>();
     
    }
    private void Update()
    {
        if (state != State.Walk) return;
        targetPosition.y = transform.position.y;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5);
        transform.LookAt(targetPosition);
        float distance = Vector3.Distance(transform.position, targetPosition);
        if(distance < 0.1f)
        {
            SetState(State.Idle);
            transform.LookAt(CameraDir);
        }
    }
    
    public void SetState(State state)
    {
        this.state = state;
        switch (state)
        {
            case State.Idle:
                state = State.Idle;
                animator.SetBool("isWalking", false);
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

            if (state == State.Idle)
            {

                remainingTime += Time.deltaTime;
            }


            if (state == State.Idle && remainingTime > MotionDelay)
            {
                Debug.Log("¸ð¼Çµô·¹ÀÌ¼³Á¤");
                MotionDelay = Random.Range(5, 10);
                Debug.Log("·£´ýÀÎµ¦½º");
                int randomIndex = Random.Range(1, 5);
                if (randomIndex <4)
                {
                    //walkÄÚµå
                    targetPosition = SetNewRandomDestination();
                    SetState((State)1);
                }
                else
                {
                    int index = randomIndex & 3;
                    SetState((State)index);
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
