using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class carControl : MonoBehaviour {

    public WheelCollider wheel_FL;
    public WheelCollider wheel_FR;
    public WheelCollider wheel_RL;
    public WheelCollider wheel_RR;

    public float speed   = 25;
    public float brake   = 30;
    public float turning = 15;

    public Text output_text;

    private float velocity;

	// Use this for initialization
	void Start () 
    {
        Vector3 moveForward = new Vector3(0f, 0f, 1.0f);
        rigidbody.centerOfMass += moveForward;
    }
	
	// Update is called once per frame
	void FixedUpdate () 
    {
	    // make the car move forward
        wheel_RL.motorTorque = speed * Input.GetAxis("Vertical");
        wheel_RR.motorTorque = speed * Input.GetAxis("Vertical");

        wheel_RL.brakeTorque = 0.0f;
        wheel_RR.brakeTorque = 0.0f;

        // turn the car
        wheel_RL.steerAngle = -Input.GetAxis("Horizontal") * turning;
        wheel_RR.steerAngle = -Input.GetAxis("Horizontal") * turning;

        // Anti-roll
        AntiRoll(ref wheel_RL, ref wheel_RR, 5000.0f);
        AntiRoll(ref wheel_FL, ref wheel_FR, 5000.0f);

        // Joystick breaks
        float break_applied = Mathf.Abs(Input.GetAxisRaw("3rd axis"));
        if (break_applied > .1f)
        {
            wheel_RL.brakeTorque = brake * break_applied;
            wheel_RR.brakeTorque = brake * break_applied;
        }

        // keyboard brakes
        if (Input.GetKeyDown(KeyCode.Space))
        {
            wheel_RL.brakeTorque = brake;
            wheel_RR.brakeTorque = brake;
        }

        //velocity = wheel_RR.rigidbody.velocity.magnitude;
        velocity = rigidbody.velocity.magnitude;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.F)) transform.Rotate(0f, 0f, 180f);
        
        float milesperhour = velocity * 0.000621371f * 60.0f * 60.0f;
        output_text.text = milesperhour.ToString("0.00") + "m/h";
    }

    void AntiRoll (ref WheelCollider WheelL, ref WheelCollider WheelR, float AntiRoll)
    {
        // Source code taken from the website listened below
        //
        // http://forum.unity3d.com/threads/how-to-make-a-physically-real-stable-car-with-wheelcolliders.50643/
        //
        // http://www.edy.es/dev/vehicle-physics/live-demo
        //

        WheelHit hit;
        float travelL = 1.0f;
        float travelR = 1.0f;
     
        bool groundedL = WheelL.GetGroundHit(out hit);
        if (groundedL)
            travelL = (-WheelL.transform.InverseTransformPoint(hit.point).y - WheelL.radius) / WheelL.suspensionDistance;
     
        bool groundedR = WheelR.GetGroundHit(out hit);
        if (groundedR)
            travelR = (-WheelR.transform.InverseTransformPoint(hit.point).y - WheelR.radius) / WheelR.suspensionDistance;
     
        var antiRollForce = (travelL - travelR) * AntiRoll;

        if (groundedL)
        {
            rigidbody.AddForceAtPosition(WheelL.transform.up * -antiRollForce, WheelL.transform.position);
        }
        if (groundedR)
        {
            rigidbody.AddForceAtPosition(WheelR.transform.up * antiRollForce, WheelR.transform.position);
        }
    }
}
