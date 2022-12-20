using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using TMPro;

public class PenguinArea : MonoBehaviour
{
    public PenguinAgent penguinAgent;
    public GameObject penguinBaby;
    public TextMeshPro cumulativeRewardText;
    public Fish fish;
    private List<GameObject> fishList;

    // called when the game starts
    private void Start() {
        ResetArea();
    }

    // called every frame
    private void Update() {
        // Update the cumulative reward text
        cumulativeRewardText.text = penguinAgent.GetCumulativeReward().ToString("0.00");
    }

    // reset the Area, including Fish and Penguins
    public void ResetArea() {
        RemoveAllFish();
        PlacePenguin();
        PlaceBaby();
        SpawnFish(4, 0.5f);
    }

    // remove Fish from area once eaten
    // <param name="fishObject">The fish to remove</param>
    public void RemoveSpecificFish(GameObject fishObject) {
        fishList.Remove(fishObject);
        Destroy(fishObject);
    }

    // returns the number of fish remaining
    public int FishRemaining {
        get { return fishList.Count; }
    }

    // Choose a random position on the X-Z plane within a partial donut shape
    // <param name="center">The center of the donut</param>
    // <param name="minAngle">Minimum angle of the wedge</param>
    // <param name="maxAngle">Maximum angle of the wedge</param>
    // <param name="minRadius">Minimum distance from the center</param>
    // <param name="maxRadius">Maximum distance from the center</param>
    // <returns>A position falling within the specified region</returns>
    public static Vector3 ChooseRandomPosition(Vector3 center, float minAngle, float maxAngle, float minRadius, float maxRadius) {
        float radius = minRadius;
        float angle = minAngle;

        if (maxRadius > minRadius) {
            // Pick a random radius
            radius = UnityEngine.Random.Range(minRadius, maxRadius);
        }

        if (maxAngle > minAngle) {
            // Pick a random angle
            angle = UnityEngine.Random.Range(minAngle, maxAngle);
        }

        // Center position + forward vector rotated around the Y axis by "angle" degrees, multiplies by "radius"
        return center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
    }

    // Remove all fish from the area
    private void RemoveAllFish() {
        if (fishList != null) {
            for (int i = 0; i < fishList.Count; i++) {
                if (fishList[i] != null) {
                    Destroy(fishList[i]);
                }
            }
        }

        fishList = new List<GameObject>();
    }

    // place penguin in the area
    private void PlacePenguin() {
        Rigidbody rigidbody = penguinAgent.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        penguinAgent.transform.position = ChooseRandomPosition(transform.position, 0f, 360f, 0f, 9f) + Vector3.up * .5f;
        penguinAgent.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
    }

    // place baby penguin in the area
    private void PlaceBaby() {
        Rigidbody rigidbody = penguinBaby.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        penguinBaby.transform.position = ChooseRandomPosition(transform.position, -45f, 45f, 4f, 9f) + Vector3.up * .5f;
        penguinBaby.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    /***************************************************************************************************************
    * Both of the above functions set rigidbody velocities to zero because unexpected things can happen when       *
    * training for long periods of time at 100x speed. For example, the penguin could fall through the floor,      *
    * then accelerate downward. When the area resets, the position would be reset, but if the downward velocity is *
    * not reset, the penguin might blast through the ground.                                                       *
    ***************************************************************************************************************/

    // spawn a random amount of fish in the scene and set their speed 
    // <param name="count">The number to spawn</param>
    // <param name="fishSpeed">The swim speed</param>
    private void SpawnFish(int count, float fishSpeed) {
        for (int i = 0; i < count; i++)
        {
            // Spawn and place the fish
            GameObject fishObject = Instantiate<GameObject>(fish.gameObject);
            fishObject.transform.position = ChooseRandomPosition(transform.position, 100f, 260f, 2f, 13f) + Vector3.up * .5f;
            fishObject.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);

            // Set the fish's parent to this area's transform
            fishObject.transform.SetParent(transform);

            // Keep track of the fish
            fishList.Add(fishObject);

            // Set the fish speed
            fishObject.GetComponent<Fish>().fishSpeed = fishSpeed;
        }
    }
}