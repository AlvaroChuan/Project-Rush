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
        controller.transform.position = controller.spawnPointPosition;
        controller.transform.rotation = controller.spawnPointRotation;
        controller.rb.velocity = Vector3.zero;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(controller.transform.position);
        foreach (var distance in controller.distances)
        {
            sensor.AddObservation(distance);
        }
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
        AddReward(-0.01f);
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

    public void GiveReward(int type)
    {
        if(type == 0) AddReward(1f);
        else AddReward(100f);
    }

    public void GivePenalty()
    {
        AddReward(-100f);
        EndEpisode();
    }

    public void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            //AddReward(-5f);
            //EndEpisode();
        }
    }
}
