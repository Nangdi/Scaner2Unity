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
public enum ObjectType
{
    Fariy,
    Mouse
}
public class ObjectController : MonoBehaviour
{
    private ParticleController particleController;
    public State state;
    public ObjectType type;
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
    Vector3 LookAtDir = new Vector3();

    public int percentChance = 50;
    private void Start()
    {
        TryGetComponent(out particleController);
        col = GameObject.FindGameObjectWithTag("bounds").GetComponent<BoxCollider>();
        offsetTuner.go = gameObject;
        StartCoroutine(BehaviorLoop());
        CameraDir = new Vector3(Camera.main.transform.position.x, transform.position.y, Camera.main.transform.position.z);
     
    }
    private void Update()
    {
        if (state != State.Walk) return;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * 5);
        LookAtDir = targetPosition;
        LookAtDir.y = transform.position.y;
        transform.LookAt(LookAtDir);
        float distance = Vector3.Distance(transform.position, targetPosition);
        if(distance < 0.1f)
        {
            SetState(State.Idle);
            //transform.LookAt(CameraDir);
            
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
                if (type == ObjectType.Fariy)
                {
                    particleController.StopParticle(2);
                }
                break;
            case State.Walk:
                state = State.Walk;
                animator.SetBool("isWalking", true);
                if (type == ObjectType.Fariy)
                {
                    particleController.playParticle(2);
                }
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
                MotionDelay = Random.Range(2, 5);
                Debug.Log("·£´ýÀÎµ¦½º");
                int randomIndex = Random.Range(1, 101);
                if (randomIndex <= percentChance)
                {
                    //walkÄÚµå
                    targetPosition = SetNewRandomDestination();
                    SetState((State)1);
                }
                else
                {
                    int random = Random.Range(2, 4);
                    //4 5
                    //int index = randomIndex;
                    if(random == 2 && type == ObjectType.Mouse)
                    {
                        transform.LookAt(CameraDir);

                    }
                    SetState((State)random);
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
        float randY = transform.position.y;
        if(type == ObjectType.Fariy)
        {
            randY = Random.Range(bounds.min.y, bounds.max.y);
        }
        targetPosition = new Vector3(randX, randY, randZ);

        return targetPosition;
    }
}
