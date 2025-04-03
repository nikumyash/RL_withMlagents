using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PlayerAgent : Agent
{
    public float moveSpeed = 2f;
    public float rotationSpeed = 2f;
    public float jumpForce = 5f;
    public float raycastDistance = 10f; // Max distance for Raycasts

    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 targetDirection = Vector3.forward;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0.5f, 0.75f, -23f);
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        targetDirection = Vector3.forward;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(isGrounded ? 1.0f : 0.0f);
        sensor.AddObservation(transform.forward); // Facing direction

        // 🔥 Raycasts for vision (8 rays in a 120° field of view)
        float[] rayAngles = { -60f, -45f, -30f, -15f, 0f, 15f, 30f, 45f, 60f  };
        foreach (float angle in rayAngles)
        {
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up * 2f, rayDirection, out hit, raycastDistance))
            {
                float normalizedDistance = hit.distance / raycastDistance; // Normalize 0 to 1
                Debug.DrawRay(transform.position, rayDirection * raycastDistance, Color.green);
                Debug.Log($"Hit {hit.collider.gameObject.name} at distance {hit.distance}");
                sensor.AddObservation(normalizedDistance);

                // Encode object type: 1 = Goal, 2 = Wall, 3 = Rotator, 4 = Ground, 0 = None
                int objectType = 0;
                if (hit.collider.CompareTag("Goal")) objectType = 1;
                else if (hit.collider.CompareTag("Wall")) objectType = 2;
                else if (hit.collider.CompareTag("Rotator")) objectType = 3;
                else if (hit.collider.CompareTag("Ground")) objectType = 4;

                sensor.AddObservation(objectType);
            }
            else
            {
                Debug.DrawRay(transform.position, rayDirection * raycastDistance, Color.green);
                sensor.AddObservation(1.0f); // Max distance (no hit)
                sensor.AddObservation(0);    // No object detected
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveAction = actions.DiscreteActions[0];
        int turnAction = actions.DiscreteActions[1];
        int jumpAction = actions.DiscreteActions[2];

        if (moveAction == 1) MoveForward();
        if (moveAction == 2) MoveBackward();
        if (turnAction == 1) RotateAgent(-90);
        if (turnAction == 2) RotateAgent(90);
        if (jumpAction == 1) Jump();
    }

    private void MoveForward()
    {
        transform.position += transform.forward * moveSpeed * Time.deltaTime;
    }

    private void MoveBackward()
    {
        transform.position -= transform.forward * moveSpeed * Time.deltaTime;
    }

    private void RotateAgent(float angle)
    {
        targetDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void Jump()
    {
        if (isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = 0; // No movement
        discreteActions[1] = 0; // No turn
        discreteActions[2] = 0; // No jump

        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 1; // Move forward
        if (Input.GetKey(KeyCode.S)) discreteActions[0] = 2; // Move backward
        if (Input.GetKey(KeyCode.A)) discreteActions[1] = 1; // Turn left
        if (Input.GetKey(KeyCode.D)) discreteActions[1] = 2; // Turn right
        if (Input.GetKey(KeyCode.Space)) discreteActions[2] = 1; // Jump
    }

    private void OnDrawGizmos()
    {
        // Draw Raycasts for visualization
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        float[] rayAngles = { -60, -45, -30, -15, 0, 15, 30, 45, 60 };
        foreach (float angle in rayAngles)
        {
            Vector3 rayDirection = Quaternion.Euler(0, angle, 0) * transform.forward;
            Gizmos.DrawRay(transform.position, rayDirection * raycastDistance);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            Debug.Log("Grounded");
        }
        if(collision.gameObject.CompareTag("Wall")){
            AddReward(-1f);
            EndEpisode();
        }
        if(collision.gameObject.CompareTag("Goal")){
            AddReward(2f);
            EndEpisode();
        }
        
    }
}

