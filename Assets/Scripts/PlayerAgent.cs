using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    [SerializeField] private Transform _goal;
    private int _currentEpisode = 0;
    private float _cumulativeReward = 0f;

    public override void Initialize(){
        
        _currentEpisode = 0;
        _cumulativeReward = 0f;
    }
    public override void OnEpisodeBegin(){
        
        _currentEpisode++;
        _cumulativeReward = 0f;
        SpawnObjects();
    }
    private void SpawnObjects(){
        // transform.localPosition = Quaternion.identity;
        // transform.localRotation = new Vector3(0f,0f,0f);

        // float randomAngle = Random.Range(0f,360f);
        // Vector3 randomDirection = Quaternion.Euler(0f,randomAngle,0f)*Vector3.forward;
        // float randomDistance = Random.Range(1f,2f);
        // Vector3 goalposition = transform.localPosition + randomDirection * randomDistance;
        // _goal.localPosition = new Vector3(goalposition.x,0.25f,goalposition.y);
        transform.localPosition = new Vector3(0f,1.25f,-23f);
    }
    public override void CollectObservations(VectorSensor sensor){
        float goalPosX = 0f; 
        float goalPosZ = (_goal.localPosition.z + 25) /50f;
        float curPosX = (float)(transform.localPosition.x + 2.5) /5f; 
        float curPosZ = (transform.localPosition.z + 25) /50f;
        sensor.AddObservation(curPosX);
        sensor.AddObservation(curPosZ);
        sensor.AddObservation(goalPosX);
        sensor.AddObservation(goalPosZ);
        
    }
    public override void OnActionReceived(ActionBuffers actions){
        MoveAgent(actions.DiscreteActions);
        AddReward(-2f/MaxStep);
        _cumulativeReward = GetCumulativeReward();
    }
    private void MoveAgent(ActionSegment<int> act){
        var action = act[0];
        // switch(action){
        //     case 1:
                
        // }
    }

}
