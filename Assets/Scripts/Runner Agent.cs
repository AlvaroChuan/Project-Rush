using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RunnerAgent : Agent
{
    [SerializeField] private AIMovement controller;
    public int playerNumber;

    public override void OnEpisodeBegin()
    {
        controller.SetSpawnPoint();
        Debug.Log("Episode Begin");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(controller.transform.position);
        sensor.AddObservation(controller.distanceToLeftWall);
        sensor.AddObservation(controller.distanceToRightWall);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float horizontalInput = actions.ContinuousActions[0];
        float verticalInput = actions.ContinuousActions[1];
        int jumpInput = actions.DiscreteActions[0];
        int slideInput = actions.DiscreteActions[1];
        controller.horizontalInput = horizontalInput;
        controller.verticalInput = verticalInput;
        controller.jumpInput = jumpInput;
        controller.slideInput = slideInput;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        discreteActions[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        discreteActions[1] = Input.GetKey(KeyCode.LeftControl) ? 1 : 0;
    }

    public void GiveReward()
    {
        AddReward(1f);
    }
}
