using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerRopeSystem : MonoBehaviour
{
    [SerializeField] public LineRenderer ropeRenderer = null;
    [SerializeField] public LayerMask ropeLayerMask;
    [SerializeField] public float climbSpeed = 3f;
    [SerializeField] public GameObject ropeHingeAnchor;
    [SerializeField] public DistanceJoint2D ropeJoint;
    [SerializeField] public Transform crosshair;
    [SerializeField] private float offset = 1.4f;
    [SerializeField] public SpriteRenderer crosshairSprite;
    [SerializeField] public PlayerController playerController;
    [SerializeField] private Vector3 presaEscaladaPosition = Vector3.zero;
    private bool ropeAttached;
    private Vector2 playerPosition;
    private List<Vector2> ropePositions = new List<Vector2>();
    private float ropeMaxCastDistance = 20f;
    private Rigidbody2D ropeHingeAnchorRb;
    private bool distanceSet;
    private bool isColliding;
    private Dictionary<Vector2, int> wrapPointsLookup = new Dictionary<Vector2, int>();
    private SpriteRenderer ropeHingeAnchorSprite;

    void Awake ()
    {
        ropeJoint.enabled = false;
        playerPosition = this.transform.position;
        ropeHingeAnchorRb = ropeHingeAnchor.GetComponent<Rigidbody2D>();
        ropeHingeAnchorSprite = ropeHingeAnchor.GetComponent<SpriteRenderer>();
    }

    private Vector2 GetClosestColliderPointFromRaycastHit(RaycastHit2D hit, PolygonCollider2D polyCollider)
    {
        var distanceDictionary = polyCollider.points.ToDictionary<Vector2, float, Vector2>(
            position => Vector2.Distance(hit.point, polyCollider.transform.TransformPoint(position)), 
            position => polyCollider.transform.TransformPoint(position));

        var orderedDictionary = distanceDictionary.OrderBy(e => e.Key);
        return orderedDictionary.Any() ? orderedDictionary.First().Value : Vector2.zero;
    }

    

    private void Update ()
	{
        playerPosition = this.transform.position;

        if (Input.GetKeyDown(KeyCode.E))
        { 
            print("sss");
            
            Vector3 facingDirection = Vector3.zero;
            if (playerController.isFacingRight == true)
            { 
                
                facingDirection = this.transform.position - presaEscaladaPosition;
                print("R=" + facingDirection);
            } 
            else
            { 
                facingDirection = presaEscaladaPosition - this.transform.position;
                
                print("L =" + facingDirection);
            }



            

            //var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
            //if (aimAngle < 0f)
            //{
            //    aimAngle = Mathf.PI * 2 + aimAngle;
            //}
            

            Vector3 aimDirection = default;

            if (playerController.isFacingRight == true)
            { 
                aimDirection = -new Vector2(this.transform.position.x, this.transform.position.y) + (Vector2)facingDirection.normalized * ropeMaxCastDistance;

                 //aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.left;
                print("R aimdirection=" + aimDirection);
            }
            else
            { 
                aimDirection = new Vector2(this.transform.position.x, this.transform.position.y) + (Vector2)facingDirection.normalized * ropeMaxCastDistance;

                //aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;
               
                print("L aimdirection=" + aimDirection);
            }

            

            
            if (!ropeAttached)
            {
                print("saaaa");
                playerController.isSwinging = false;
            }
            else
            {

                print("123123aa");
                playerController.isSwinging = true;
                playerController.ropeHook = ropePositions.Last();
                crosshairSprite.enabled = false;
                print("ropePositions.Count=" + ropePositions.Count);

                if (ropePositions.Count > 0)
                {
                    var lastRopePoint = ropePositions.Last();
                    var playerToCurrentNextHit = Physics2D.Raycast(playerPosition, (lastRopePoint - playerPosition).normalized, Vector2.Distance(playerPosition, lastRopePoint) - 0.1f, ropeLayerMask);
                    if (playerToCurrentNextHit)
                    {
                        var colliderWithVertices = playerToCurrentNextHit.collider as PolygonCollider2D;
                        if (colliderWithVertices != null)
                        {
                            var closestPointToHit = GetClosestColliderPointFromRaycastHit(playerToCurrentNextHit, colliderWithVertices);
                            if (wrapPointsLookup.ContainsKey(closestPointToHit))
                            {
                                ResetRope();
                                return;
                            }

                            ropePositions.Add(closestPointToHit);
                            wrapPointsLookup.Add(closestPointToHit, 0);
                            distanceSet = false;
                        }
                    }
                }





            }

            UpdateRopePositions();
            HandleRopeLength();
            HandleInput(aimDirection);
            HandleRopeUnwrap();


        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResetRope();
        }


     //   var worldMousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f));
     //   var facingDirection = worldMousePosition - this.transform.position;
     //   var aimAngle = Mathf.Atan2(facingDirection.y, facingDirection.x);
     //   if (aimAngle < 0f)
     //   {
     //       aimAngle = Mathf.PI * 2 + aimAngle;
     //   }

     //   var aimDirection = Quaternion.Euler(0, 0, aimAngle * Mathf.Rad2Deg) * Vector2.right;
     //   playerPosition = transform.position;

     //   if (!ropeAttached)
     //   {
     //       SetCrosshairPosition(aimAngle);
     //       playerController.isSwinging = false;
	    //}
	    //else
     //   {
     //       playerController.isSwinging = true;
     //       playerController.ropeHook = ropePositions.Last();
     //       crosshairSprite.enabled = false;

	    //    if (ropePositions.Count > 0)
	    //    {
	    //        var lastRopePoint = ropePositions.Last();
     //           var playerToCurrentNextHit = Physics2D.Raycast(playerPosition, (lastRopePoint - playerPosition).normalized, Vector2.Distance(playerPosition, lastRopePoint) - 0.1f, ropeLayerMask);
     //           if (playerToCurrentNextHit)
     //           {
     //               var colliderWithVertices = playerToCurrentNextHit.collider as PolygonCollider2D;
     //               if (colliderWithVertices != null)
     //               {
     //                   var closestPointToHit = GetClosestColliderPointFromRaycastHit(playerToCurrentNextHit, colliderWithVertices);
     //                   if (wrapPointsLookup.ContainsKey(closestPointToHit))
     //                   {
     //                       ResetRope();
     //                       return;
     //                   }

     //                   ropePositions.Add(closestPointToHit);
     //                   wrapPointsLookup.Add(closestPointToHit, 0);
     //                   distanceSet = false;
     //               }
     //           }
     //       }
     //   }

	    //UpdateRopePositions();
     //   HandleRopeLength();
     //   HandleInput(aimDirection);
     //   HandleRopeUnwrap();
    }

    private void HandleInput(Vector2 aimDirection)
    {

        print("ropeattaced=" + ropeAttached + " direction=" + aimDirection + " playerPosition=" + playerPosition + " pos=" + this.transform.position);
        //if (Input.GetKeyDown(KeyCode.P))
        {

            if (ropeAttached == true) return;
            ropeRenderer.enabled = true;

            var hit = Physics2D.Raycast(this.transform.position, aimDirection, ropeMaxCastDistance, ropeLayerMask);
            Debug.DrawLine(this.transform.position, aimDirection, Color.red, 100000);
            if (hit.collider != null)
            {
                ropeAttached = true;
                if (!ropePositions.Contains(hit.point))
                {
                    transform.GetComponent<Rigidbody2D>().AddForce(new Vector2(0f, 2f), ForceMode2D.Impulse);
                    ropePositions.Add(hit.point);
                    wrapPointsLookup.Add(hit.point, 0);
                    ropeJoint.distance = Vector2.Distance(playerPosition, hit.point);
                    ropeJoint.enabled = true;
                    ropeHingeAnchorSprite.enabled = true;
                }
            }
            else
            {
                ropeRenderer.enabled = false;
                ropeAttached = false;
                ropeJoint.enabled = false;
            }
        }

        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    ResetRope();
        //}
    }

    private void ResetRope()
    {
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

    private void SetCrosshairPosition(float aimAngle)
    {
        if (!crosshairSprite.enabled)
        {
            crosshairSprite.enabled = true;
        }

        var x = transform.position.x + offset * Mathf.Cos(aimAngle);
        var y = transform.position.y + offset * Mathf.Sin(aimAngle);

        var crossHairPosition = new Vector3(x, y, 0);
        crosshair.transform.position = crossHairPosition;
    }

    private void HandleRopeLength()
    {
        if (Input.GetAxis("Vertical") >= 1f && ropeAttached && !isColliding)
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

        print("ropeattaced=" + ropeAttached);
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
        var playerDir = playerPosition - anchorPosition;
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

        print("colliderstay" + collision.name + " tag=" + collision.tag);
        if (collision.CompareTag("PresaEscalada"))
        { 
        
            presaEscaladaPosition = collision.gameObject.transform.position;
        
        }

        isColliding = true;
    }

    private void OnTriggerExit2D(Collider2D colliderOnExit)
    {
        isColliding = false;
        presaEscaladaPosition = default;
    }
}