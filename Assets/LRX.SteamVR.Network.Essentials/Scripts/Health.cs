/* Original Code from SteamVR Essentials
 * Purpose: Handle health for players and enemies
 * 
 * Modifications made by Mohammad Alam
 *  - Added an image that spawns in front of the player to signify that they got damaged
 *      - This required taking out the old text health information
 *      - This can still work with the old character, but their health text will not change.
 *  - The image is added by the PlayerController or VRPlayerController.
 *      - If there is no image, the method does not happen.
 *  - GetAncestor Function seemed to make things worse for the respawn call, so it has been commented out
 *      - The respawn method was only useful for the PlayerController, but since the players here
 *          do not need to respawn, because loss counts as a stat rather than "death", it is not needed
 *      - Also, when the respawn happened, the spawn location was not correct. This is due to the 
 *          controller spawn being offset from VR spawnpoint.
 *      
 * Currently, health does not really have too much of a purpose other than a method to show that
 * the player got hit. This is because round loss or enemy KO is not dependent of going to 0 Hp. 
 * The implementation could easily be changed so that there is no health and the only purpose is
 * to let the player know that they got damaged, but it's not too costly to keep it here and it 
 * could become useful.
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class Health : NetworkBehaviour 
{
	public const int maxHealth = 100;
	public bool destroyOnDeath;
    public Image damageImage;
    public float flashSpeed = 5f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.1f);

	private NetworkStartPosition[] spawnPoints;
    private bool damaged;

	[SyncVar(hook = "OnChangeHealth")]
	public int currentHealth = maxHealth;

	void Start ()
	{
		if (isLocalPlayer)
		{
			spawnPoints = FindObjectsOfType<NetworkStartPosition>();
            damaged = false;
		}
	}

    void Update ()
    {
        if (damageImage)
        {
            if (damaged)
            {
                damageImage.color = flashColor;
                damaged = false;
            }
            else
            {
                damageImage.color = Color.Lerp(damageImage.color, Color.clear, flashSpeed * Time.deltaTime);
            }
        }
    }

	public void TakeDamage(int amount)
	{
		if (!isServer)
		{
			return;
		}

        currentHealth -= amount;

		if (currentHealth <= 0)
		{
			if (destroyOnDeath) {
				Destroy (gameObject);
			} else {
				currentHealth = maxHealth;
                /* Currently unnecessary for application. Uncomment to reuse.
                if (GetComponent<PlayerController>())
                {
                    RpcRespawn();
                }
                */
			}
		}
	}

	void OnChangeHealth (int currentHealth)
	{
        damaged = true;
	}

	[ClientRpc]
	void RpcRespawn()
	{
		if (isLocalPlayer) {
			// Set the spawn point to origin as a default value
			Vector3 spawnPoint = Vector3.zero;

			// If there is a spawn point array and the array is not empty, pick one at random
			if (spawnPoints != null && spawnPoints.Length > 0)
			{
				spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)].transform.position;
			}

			// Set the player’s position to the chosen spawn point
			//transform.p
			//Transform ancestor = GetAncestor(transform);
			transform.position = spawnPoint;
            transform.position = new Vector3(transform.position.x + -1f, transform.position.y + .8f, transform.position.z);
        }
	}

    /*
	Transform GetAncestor(Transform child)
	{
		Transform currentObject = child;
		while (currentObject.parent != null) {
			currentObject = currentObject.parent;
		}
		return currentObject;
	}
    */
}