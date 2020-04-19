using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UniRx.Async;
using System;

public class PlayerRopeSystem : MonoBehaviour
{
    [SerializeField] public LineRenderer ropeRenderer = null;
    [SerializeField] public LayerMask ropeLayerMask;
    [SerializeField] public float climbSpeed = 3f;
    [SerializeField] public GameObject ropeHingeAnchor;
    [SerializeField] public DistanceJoint2D ropeJoint;
    [SerializeField] private float offset = 1.4f;
    [SerializeField] public PlayerController playerController;
    [SerializeField] private Vector3 presaEscaladaPosition = Vector3.zero;
    [SerializeField] private bool isPresaSelected = false;
    [SerializeField] private Rigidbody2D rigid = null;
    [SerializeField] private BoxCollider2D playerCollider = null;
    [SerializeField] private float ropeMaxCastDistance = 20f;
    [SerializeField] private Rigidbody2D ropeHingeAnchorRb;
    [SerializeField] private SpriteRenderer ropeHingeAnchorSprite;

    private bool ropeAttached;
    private List<Vector2> ropePositions = new List<Vector2>();
    
    private bool distanceSet;
    private bool isColliding;
    
    private Dictionary<Vector2, int> wrapPointsLookup = new Dictionary<Vector2, int>();
    private PresaEscaladaManager presaEscalada = null;

    void Awake ()
    {
        ropeJoint.enabled = false;
        //playerPosition = this.transform.position;
        //ropeHingeAnchorRb = ropeHingeAnchor.GetComponent<Rigidbody2D>();
        //ropeHingeAnchorSprite = ropeHingeAnchor.GetComponent<SpriteRenderer>();
    }

    private void Update ()
	{
        
        
        if (isPresaSelected == false) return;

        if (Input.GetKeyDown(KeyCode.E))
        { 

            if (ropeAttached == true) return;

            print("pulsado E");
            
            Vector3 facingDirection = Vector3.zero;
            facingDirection = presaEscaladaPosition - this.transform.position;

            Vector3 aimDirection = default;
            
            aimDirection = new Vector2(this.transform.position.x, this.transform.position.y) + (Vector2)facingDirection.normalized * ropeMaxCastDistance;

            HandleInput(aimDirection);
            


        }

        UpdateRopePositions();
        HandleRopeLength();
        HandleRopeUnwrap();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            presaEscalada.DesActivarSelectedRender();

            isColliding = false;
            presaEscaladaPosition = Vector3.zero;
            isPresaSelected = false;
            ResetRope();

        }


    }

    private async void HandleInput(Vector2 aimDirection)
    {

        //print("ropeattaced=" + ropeAttached + " direction=" + aimDirection + " playerPosition=" + playerPosition + " pos=" + this.transform.position);

        ropeRenderer.enabled = true;
        bool encontrado = false;

        var hits = Physics2D.RaycastAll(this.transform.position, aimDirection, ropeMaxCastDistance, ropeLayerMask);

        for (ushort i = 0; i < hits.Length; i++)
        { 
            //print("nombrehit=" + hits[i].transform.gameObject.name + " tag=" + hits[i].transform.tag );
            if (hits[i].transform.CompareTag("PresaEscalada") == true)
            { 
                if (hits[i].collider != null)
                {
                    //subimos un poco al player

                    rigid.AddRelativeForce(new Vector2(50f, 20f), ForceMode2D.Impulse);
                    await UniTask.Delay(TimeSpan.FromMilliseconds(200));

                    isColliding = false;
                    ropeAttached = true;
                    encontrado = true;
                    ropeRenderer.SetPosition(0, this.transform.position);
                    ropeRenderer.SetPosition(1, hits[i].transform.position);

                    ropeHingeAnchorRb.transform.position = hits[i].transform.position;
                    ropePositions.Add(hits[i].transform.position);
                    playerController.ropeHook = ropePositions.Last();
                    wrapPointsLookup.Add(hits[i].transform.position, 0);

                    ropeJoint.distance = Vector2.Distance(this.transform.position, hits[i].transform.position);
                    ropeJoint.enabled = true;
                    ropeHingeAnchorSprite.enabled = true;
                    await UniTask.Delay(1000);
                    rigid.velocity = Vector2.zero;
                    rigid.drag = 0.3f;
                    playerController.isSwinging = true;
                }
                
                
            }
            
        }

        if (encontrado == false)
        { 
            
            ropeRenderer.enabled = false;
            ropeAttached = false;
            ropeJoint.enabled = false;

        }

        
    }

    private void ResetRope()
    {

        playerController.isSwinging = false;
        ropeJoint.enabled = false;
        ropeAttached = false;
        playerController.isSwinging = false;
        ropeRenderer.positionCount = 2;
        ropeRenderer.SetPosition(0, transform.position);
        ropeRenderer.SetPosition(1, transform.position);
        ropePositions.Clear();
        wrapPointsLookup.Clear();
        ropeHingeAnchorSprite.enabled = false;
    }



    private void HandleRopeLength()
    {
        if (Input.GetAxis("Vertical") >= 0.1f && ropeAttached && isColliding == false)
        {
            ropeJoint.distance -= Time.deltaTime * climbSpeed;
        }
        else if (Input.GetAxis("Vertical") < 0f && ropeAttached)
        {
            ropeJoint.distance += Time.deltaTime * climbSpeed;
        }
    }

    private void UpdateRopePositions()
    {

        if (ropeAttached)
        {
            ropeRenderer.positionCount = ropePositions.Count + 1;

            for (var i = ropeRenderer.positionCount - 1; i >= 0; i--)
            {
                if (i != ropeRenderer.positionCount - 1)
                {
                    ropeRenderer.SetPosition(i, ropePositions[i]);
                    
                    if (i == ropePositions.Count - 1 || ropePositions.Count == 1)
                    {
                        if (ropePositions.Count == 1)
                        {
                            var ropePosition = ropePositions[ropePositions.Count - 1];
                            ropeHingeAnchorRb.transform.position = ropePosition;
                            if (!distanceSet)
                            {
                                ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                                distanceSet = true;
                            }
                        }
                        else
                        {
                            var ropePosition = ropePositions[ropePositions.Count - 1];
                            ropeHingeAnchorRb.transform.position = ropePosition;
                            if (!distanceSet)
                            {
                                ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                                distanceSet = true;
                            }
                        }
                    }
                    else if (i - 1 == ropePositions.IndexOf(ropePositions.Last()))
                    {
                        var ropePosition = ropePositions.Last();
                        ropeHingeAnchorRb.transform.position = ropePosition;
                        if (!distanceSet)
                        {
                            ropeJoint.distance = Vector2.Distance(transform.position, ropePosition);
                            distanceSet = true;
                        }
                    }
                }
                else
                {
                    ropeRenderer.SetPosition(i, transform.position);
                }
            }
        }
    }

    private void HandleRopeUnwrap()
    {
        if (ropePositions.Count <= 1)
        {
            return;
        }


        // 1
        var anchorIndex = ropePositions.Count - 2;
        // 2
        var hingeIndex = ropePositions.Count - 1;
        // 3
        var anchorPosition = ropePositions[anchorIndex];
        // 4
        var hingePosition = ropePositions[hingeIndex];
        // 5
        var hingeDir = hingePosition - anchorPosition;
        // 6
        var hingeAngle = Vector2.Angle(anchorPosition, hingeDir);
        // 7
        var playerDir = (Vector2)this.transform.position - anchorPosition;
        // 8
        var playerAngle = Vector2.Angle(anchorPosition, playerDir);

        if (!wrapPointsLookup.ContainsKey(hingePosition))
        {
            Debug.LogError("no tiene posicion =" + hingePosition);
            return;
        }

        if (playerAngle < hingeAngle)
        {
            // 1
            if (wrapPointsLookup[hingePosition] == 1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            // 2
            wrapPointsLookup[hingePosition] = -1;
        }
        else
        {
            // 3
            if (wrapPointsLookup[hingePosition] == -1)
            {
                UnwrapRopePosition(anchorIndex, hingeIndex);
                return;
            }

            // 4
            wrapPointsLookup[hingePosition] = 1;
        }
    }

    private void UnwrapRopePosition(int anchorIndex, int hingeIndex)
    {
        // 1
        var newAnchorPosition = ropePositions[anchorIndex];
        wrapPointsLookup.Remove(ropePositions[hingeIndex]);
        ropePositions.RemoveAt(hingeIndex);

        // 2
        ropeHingeAnchorRb.transform.position = newAnchorPosition;
        distanceSet = false;

        // Set new rope distance joint distance for anchor position if not yet set.
        if (distanceSet)
        {
            return;
        }
        ropeJoint.distance = Vector2.Distance(transform.position, newAnchorPosition);
        distanceSet = true;
    }

   

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("PresaEscalada"))
        { 
            presaEscalada = collision.GetComponent<PresaEscaladaManager>();
            presaEscalada.ActivarSelectedRender();
            presaEscaladaPosition = collision.gameObject.transform.position;
            //isColliding = true;
            isPresaSelected = true;
        }

        
    }

    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.CompareTag("PresaEscalada"))
        { 
        
            if (ropeAttached == true ) return;

            collision.GetComponent<PresaEscaladaManager>().DesActivarSelectedRender();
            presaEscalada = null;
            isColliding = false;
            presaEscaladaPosition = Vector3.zero;
            isPresaSelected = false;
            ResetRope();
        }


        
    }
}