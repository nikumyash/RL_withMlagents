using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PlayerAgent : Agent
{
    public float moveSpeed = 2f;
    public Transform gameEnv; 
    public float rotationSpeed = 2f;
    public float jumpForce = 5f;
    public float raycastDistance = 10f; // Max distance for Raycasts
    public GameObject target;
    private Rigidbody rb;
    private bool isGrounded;
    private Vector3 targetDirection = Vector3.forward;
    public GameObject coinPrefab;

    private void SpawnCoins()
    {
        DestroyCoins();
        Instantiate(coinPrefab, gameEnv.TransformPoint(new Vector3(Random.Range(-1.5f,1.5f), 0.5f, -20f)), Quaternion.identity,gameEnv);
        Instantiate(coinPrefab, gameEnv.TransformPoint(new Vector3(Random.Range(-1.5f,1.5f), 0.5f, -16f)), Quaternion.identity,gameEnv);
        Instantiate(coinPrefab, gameEnv.TransformPoint(new Vector3(Random.Range(-1.5f,1.5f), 0.5f, -12f)), Quaternion.identity,gameEnv);
        Instantiate(coinPrefab, gameEnv.TransformPoint(new Vector3(Random.Range(-1.5f,1.5f), 0.5f, -4f)), Quaternion.identity,gameEnv);
        Instantiate(coinPrefab, gameEnv.TransformPoint(new Vector3(Random.Range(-1.5f,1.5f), 0.5f, 8f)), Quaternion.identity,gameEnv);
        Instantiate(coinPrefab, gameEnv.TransformPoint(new Vector3(Random.Range(-1.5f,1.5f), 0.5f, 12f)), Quaternion.identity,gameEnv);
    }
    private void DestroyCoins(){
        foreach (Transform child in gameEnv)
        {
            if (child.CompareTag("Coin"))
            {
                Destroy(child.gameObject);
            }
        }
    }
    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        SpawnCoins();
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-1.5f,1.5f), 0.75f, -23f);
        transform.rotation = Quaternion.identity;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        targetDirection = Vector3.forward;
        SpawnCoins();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(target.transform.localPosition);
        sensor.AddObservation(isGrounded ? 1.0f : 0.0f);
        sensor.AddObservation(transform.forward); // Facing direction
    }
    void Update()
    {
        if (transform.position.y < -2f)
        {
            AddReward(-1f);
            EndEpisode();
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


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
        if(collision.gameObject.CompareTag("Wall")){
            AddReward(-1f);
            EndEpisode();
        }
        if(collision.gameObject.CompareTag("Goal")){
            AddReward(2f);
            EndEpisode();
        }
        if(collision.gameObject.CompareTag("Rotator")){
            AddReward(-0.2f);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Coin"))
        {
            Debug.Log("Coin");
            Destroy(other.gameObject);
            AddReward(1f);
        }
    }
}

