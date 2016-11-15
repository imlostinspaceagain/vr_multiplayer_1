/* Original Code from SteamVR Essentials
 * Allow Player to control character with controller/keyboard
 * 
 * Modifications made by Mohammad Alam
 *  - Mouselook properly clamps now
 *  - Body/Camera movement has been separated such that character no longer flies
 *      - Flight can be reenabled by uncommented out the appropriate line
 *  - Gun/GunHand no longer has Parent-Child combo with Camera to allow firing.
 *      - This was important for allowing shooting over multiplayer
 *  - Some Old Child-Parent Combinations have been scrapped for new DamageImage mechanic
 *      - This means old prefabs for the Player are no longer compatible
 *  - There are frequent Get calls that have been optimized by adding variables
 *  - Datalogger Connection added
 *      - Sends Player Name and Transform for DataLogging
 *  - Input for Gunfire was originally Space and has now been changed to left-click
 */

// Note: When incorporating new character models here, either edit code accordingly,
// specifically with the GetChild() calls or follow the current player's prefab style.

// Gun Shot Sound Courtesy of Freesound.org: http://freesound.org/people/Brokenphono/sounds/344143/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class PlayerController : NetworkBehaviour
{
    // Original Variables
	public GameObject bulletPrefab;
	private MouseLook mouseLook;
	public Transform bulletSpawn;
    public float speed = 2f;

    // New Variables
    // For GunArm Position Fixing
    private float PosMag;
    // Object Caching
    private Camera mainCam;
    private Transform GunArm;
    // For Sound
    private AudioSource audioSrc;

    public override void OnStartLocalPlayer()
	{
        // Initialize
        // Spawn is set for VR, so it is moved accordingly for Controller
        transform.position = new Vector3(transform.position.x + -1f, transform.position.y + .8f, transform.position.z);
        transform.rotation = Quaternion.Euler(0,-90,0);
		GetComponent<Renderer>().material.color = Color.blue;
        mainCam = Camera.main;
        GunArm = transform.GetChild(2);
        Transform HUDCanv = transform.GetChild(3);
        audioSrc = GetComponent<AudioSource>();

        // attach camera to player.. 1st person view..
        mainCam.transform.parent = transform;
		mainCam.transform.localPosition = new Vector3 (0, .5f, 0);
		mainCam.transform.localRotation = Quaternion.Euler (6.31f, 0, 0);

        // Get Magnitude of distance from Camera to GunArm
        PosMag = Vector3.Magnitude(mainCam.transform.position - GunArm.position);

        // Get HUDCanvas
        // Note: Modification was made to take out the HP label and add in a new damage image canvas.
        //       This means the object has a different array of children. The original prefab will no
        //       longer work with this code. Only the new modified one will.
        HUDCanv.GetComponent<Canvas>().worldCamera = Camera.main;
        HUDCanv.GetComponent<Canvas>().planeDistance = 1f;
        GetComponent<Health>().damageImage = HUDCanv.GetChild(0).GetComponent<Image>();

        // Initialize MouseLook Script
        mouseLook = new MouseLook ();
		mouseLook.Init (transform, Camera.main.transform);

        // Check for DataLogger Object and give it necessary information
        GameObject dataloggerTest = GameObject.FindGameObjectWithTag("DataLogger");

        if (dataloggerTest)
        {
            DataLogger datalogger = dataloggerTest.GetComponent<DataLogger>();
            datalogger.Player = gameObject.transform;
            datalogger.LeftHand = null;
            datalogger.RightHand = null;
        }
    }

	void Update()
	{
		if (!isLocalPlayer)
		{
			return;
		}

        // non vr player input here
        var x = Input.GetAxis("Horizontal") * speed * Time.fixedDeltaTime;
        var z = Input.GetAxis("Vertical") * speed * Time.fixedDeltaTime;

        transform.Translate(new Vector3(x,0,z));

        mouseLook.LookRotation (transform, mainCam.transform);

        GunArm.position = mainCam.transform.position + 
            (mainCam.transform.forward + 
            mainCam.transform.right * 0.7f - 
            mainCam.transform.up * 0.3f)
            * PosMag;

        GunArm.rotation = Camera.main.transform.rotation * Quaternion.Euler(90,-0.5f,0);

        // For flight.
        // transform.rotation = Camera.main.transform.rotation;

        // common input here
        if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			CmdFire();
		}
	}

    [ClientRpc]
    private void RpcPlayGunShot(Vector3 soundStart)
    {
        // Play Sound over network 
        if (audioSrc)
            audioSrc.Play();
    }

    // This [Command] code is called on the Client …
    // … but it is run on the Server!
    [Command]
	protected void CmdFire()
	{
		var bullet = (GameObject)Instantiate(
			bulletPrefab,
			bulletSpawn.position,
			bulletSpawn.rotation);

		// Add velocity to the bullet
		bullet.GetComponent<Rigidbody>().velocity = bullet.transform.forward * 6;

        // Play Sound over network
        RpcPlayGunShot(bulletSpawn.position);

        // Spawn the bullet on the Clients
        NetworkServer.Spawn(bullet);

		// Destroy the bullet after 2 seconds
		Destroy(bullet, 2.0f);
	}
}