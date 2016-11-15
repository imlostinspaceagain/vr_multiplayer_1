/* Original Code by Joe Snider and some borrowed code from Bullet script from SteamVR Network Essentials
 * Purpose: Have Bullet cause damage or destroy on collisions and record its data
 * 
 * Modifications made by Mohammad Alam
 *  - Changed original method to utilize some new scripts
 *      - Uses EnemySpawner EventSystem to Decrement Enemy Count
 *      - Uses DataLogger to watch bullets and collisions
 *      - Calls Health Script on Player
 */

using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BulletCollide : NetworkBehaviour
{
    // Lifetime of the bullet when fired
    public float lifeTime = 10f;

    private DataLogger datalogger;
    // Needed to identify the object in the server
    [SyncVar]
    public NetworkInstanceId projectileSourceId;

    void Start()
    {
        if (!isServer)
            return;
        // Start timer to destroy
        Invoke("DestroyMe", lifeTime);

        // Call Datalogger to add bullet
        GameObject dataloggerTest = GameObject.FindGameObjectWithTag("DataLogger");

        if (dataloggerTest)
            datalogger = dataloggerTest.GetComponent<DataLogger>();
        if(datalogger)
            datalogger.AddBullet(this.transform);
    }
   
    // Destroy after time lifetime
    void DestroyMe()
    {
        NetworkServer.Destroy(gameObject);
        if (datalogger)
            datalogger.RemoveBullet(this.transform);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Do not invoke collision methods below if the weapon is hit and do not perform locally
        if (!isServer)
            return;

        // Don't hit the weapon. This is a stylistic choice. If desired, this can be modified
        // such that the player/enemy recieves damage when their weapon is hit, but then there
        // needs to be a way to handle it so that the player/enemy does not hit themselves when
        // they fire.
        if (collision.gameObject.CompareTag("Weapon"))
            return;

        // Destroy object on collision
        NetworkServer.Destroy(gameObject);
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EventManager.TriggerEvent("EnemyDec");
            if (datalogger)
                datalogger.RemoveEnemy(collision.transform);
            Destroy(collision.gameObject);
            //add an explosion or something
        }
        /*
        else if (collision.gameObject.CompareTag("Player"))
        {
            Game Over Routine???
            Currently, it is set such that whenever either player is hit, that marks as a loss for that
            round. The handling for that could be changed so that it is moved here instead of in the 
            datalogger, which is where it is now. 
        }
        */

        //If target has health, take damage
        var health = collision.gameObject.GetComponent<Health>();
        if (health != null)
        {
            health.TakeDamage(10);
        }

        // Record Data
        if (datalogger)
        {
            datalogger.RecordHit(collision.transform);
            datalogger.RemoveBullet(this.transform);
        }
    }
}
