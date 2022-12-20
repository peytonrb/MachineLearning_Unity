using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PenguinAgent : Agent
{
    public float moveSpeed = 5.0f;
    public float turnSpeed = 180.0f;
    public GameObject heartPrefab;
    public GameObject regurgitatedFishPrefab;
    private PenguinArea penguinArea;
    new private Rigidbody rigidbody;
    private GameObject baby;
    private bool isFull; // if true, penguin has full stomach

    // Initial setup, called when the agent is enabled
    public override void Initialize() {
        base.Initialize();
        penguinArea = GetComponentInParent<PenguinArea>();
        baby = penguinArea.penguinBaby;
        rigidbody = GetComponent<Rigidbody>();
    }

    // Perform actions based on a vector of numbers
    // <param name="actionBuffers">The struct of actions to take</param>
    public override void OnActionReceived(ActionBuffers actionBuffers) {

        // Convert the first action to forward movement
        float forwardAmount = actionBuffers.DiscreteActions[0];

        // Convert the second action to turning left or right
        float turnAmount = 0f;
        if (actionBuffers.DiscreteActions[1] == 1f)
        {
            turnAmount = -1f;
        }
        else if (actionBuffers.DiscreteActions[1] == 2f)
        {
            turnAmount = 1f;
        }

        // Apply movement
        rigidbody.MovePosition(transform.position + transform.forward * forwardAmount * moveSpeed * Time.fixedDeltaTime);
        transform.Rotate(transform.up * turnAmount * turnSpeed * Time.fixedDeltaTime);

        // Apply a tiny negative reward every step to encourage action
        if (MaxStep > 0) AddReward(-1f / MaxStep);
    }

    /**********************************************************************************************************
    * This function allows control of the agent without a neural network. This function will read             *
    * inputs from the human player via the keyboard, convert them into actions, and place those actions into  *
    * an array called DiscreteActions. This same array is what is read in the OnActionReceived function       *
    * when a human is playing (rather than an AI).                                                            *
    **********************************************************************************************************/

    // Read inputs from the keyboard and convert them to a list of actions.
    // This is called only when the player wants to control the agent and has set
    // Behavior Type to "Heuristic Only" in the Behavior Parameters inspector.
    // <returns>A vectorAction array of floats that will be passed into <see cref="AgentAction(float[])"/></returns>
    public override void Heuristic(in ActionBuffers actionsOut) {
        int forwardAction = 0;
        int turnAction = 0;

        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            turnAction = 1;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            turnAction = 2;
        }

        // Put the actions into the array
        actionsOut.DiscreteActions.Array[0] = forwardAction;
        actionsOut.DiscreteActions.Array[1] = turnAction;
    }

    // When a new episode begins, reset the agent and area
    public override void OnEpisodeBegin() {
        isFull = false;
        penguinArea.ResetArea();
    }

    // Collect all non-Raycast observations
    // <param name="sensor">The vector sensor to add observations to</param>
    public override void CollectObservations(VectorSensor sensor) {
        // Whether the penguin has eaten a fish (1 float = 1 value)
        sensor.AddObservation(isFull);

        // Distance to the baby (1 float = 1 value)
        sensor.AddObservation(Vector3.Distance(baby.transform.position, transform.position));

        // Direction to baby (1 Vector3 = 3 values)
        sensor.AddObservation((baby.transform.position - transform.position).normalized);

        // Direction penguin is facing (1 Vector3 = 3 values)
        sensor.AddObservation(transform.forward);

        // 1 + 1 + 3 + 3 = 8 total values
    }

    // When the agent collides with something, take action
    // <param name="collision">The collision info</param>
    private void OnCollisionEnter(Collision collision) {
        if (collision.transform.CompareTag("fish"))
        {
            // Try to eat the fish
            EatFish(collision.gameObject);
        }
        else if (collision.transform.CompareTag("baby"))
        {
            // Try to feed the baby
            RegurgitateFish();
        }
    }

    // Check if agent is full, if not, eat the fish and get a reward
    // <param name="fishObject">The fish to eat</param>
    private void EatFish(GameObject fishObject) {
        if (isFull) return; // Can't eat another fish while full
        isFull = true;

        penguinArea.RemoveSpecificFish(fishObject);

        AddReward(1f);
    }

    // Check if agent is full, if yes, feed the baby
    private void RegurgitateFish() {
        if (!isFull) return; // Nothing to regurgitate
        isFull = false;

        // Spawn regurgitated fish
        GameObject regurgitatedFish = Instantiate<GameObject>(regurgitatedFishPrefab);
        regurgitatedFish.transform.parent = transform.parent;
        regurgitatedFish.transform.position = baby.transform.position;
        Destroy(regurgitatedFish, 4f);

        // Spawn heart
        GameObject heart = Instantiate<GameObject>(heartPrefab);
        heart.transform.parent = transform.parent;
        heart.transform.position = baby.transform.position + Vector3.up;
        Destroy(heart, 4f);

        AddReward(1f);

        if (penguinArea.FishRemaining <= 0)
        {
            EndEpisode();
        }
    }
}
