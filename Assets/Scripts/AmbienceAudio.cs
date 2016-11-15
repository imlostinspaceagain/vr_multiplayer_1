/* Code by Mohammad Alam
 * Purpose: Allow ambience audio to be played over network and have every player hear.
 *      This requires an Audio Source component and needs for the Audio Source component
 *      to have a sound to play and loop. Without it, then this will instantly be disabled.
 *      
 *      Current sound file from: http://freesound.org/people/leonelmail/sounds/366100/
 *      NOTE: Sound has an awkward silence right at the end of the file. Please create/find
 *      a better replacement.
 */

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class AmbienceAudio : NetworkBehaviour {

	// Use this for initialization
	public override void OnStartLocalPlayer () {
        if (GetComponent<AudioSource>().clip)
        {
            CmdReadyAmbience();
        }
	}

    // Client tells server to play sound
    [Command]
    private void CmdReadyAmbience()
    {
        RpcPlayAmbience();
    }

    // Server plays sound
    [ClientRpc]
    private void RpcPlayAmbience()
    {
        GetComponent<AudioSource>().Play();
    }
}
