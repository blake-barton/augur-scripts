using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Spawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] Range distanceToSpawn;

    public void Spawn()
    {
        // get a vector with random direction and magnitude between distance min and max
        Vector2 randomVector = UtilMath.RandomVector2(360f, 0f, distanceToSpawn.GetRandomValueInRange());

        // get a world point from the target and vector
        Vector3 point = transform.position + (Vector3)randomVector;

        // find closest walkable node to world point
        GraphNode node = AstarPath.active.GetNearest(point, NNConstraint.Default).node;

        // get the world position of said node
        Vector3 nodePosition = (Vector3)node.position;

        Instantiate(objectToSpawn, nodePosition, Quaternion.identity);
    }

    public void SpawnOnSelf()
    {
        Instantiate(objectToSpawn, transform.position, Quaternion.identity);
    }

    public void Spawn(Vector3 position)
    {
        Instantiate(objectToSpawn, position, Quaternion.identity);
    }

    public void Spawn(Vector3 position, Quaternion rotation)
    {
        Instantiate(objectToSpawn, position, rotation);
    }
}
