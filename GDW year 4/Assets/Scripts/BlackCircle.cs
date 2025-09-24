using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackCircle : MonoBehaviour
{
    public Transform player;
    public float offset = 0.05f; // lift it slightly to avoid z-fighting

    void Update()
    {
        // Cast a ray downward from the player
        if (Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, 100f))
        {
            transform.position = hit.point + Vector3.up * offset;

            // Align to ground surface AND rotate so the Quad faces up
            Quaternion groundRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            transform.rotation = groundRotation * Quaternion.Euler(90, 0, 0);
        }
        else
        {
            // Hide if no ground
            transform.position = new Vector3(0, -9999, 0);
        }
    }
}
