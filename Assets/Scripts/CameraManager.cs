using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {
	// Make a singleton
	public static CameraManager Instance { set; get; }
	public GameObject mainCamera;
	public GameObject secondaryCamera;
	private Vector3 gameBoardCenter;

	// Will always be between 0 and 360
	private float revolutionIndex;
	private float revolutionRadius = 4.5f;

	void Start() {
		Instance = this;
		gameBoardCenter = GameObject.FindGameObjectWithTag("GameBoard")
			.GetComponent<Renderer>().bounds.center;
		revolutionIndex = 180;

		// Default to only the main camera is active
		mainCamera.SetActive(true);
		secondaryCamera.SetActive(false);
	}
	
	
	// Update is called once per frame
	void Update () {
		var zOffset = Mathf.Cos((revolutionIndex * Mathf.PI) / 180)*revolutionRadius;
		var xOffset = Mathf.Sin((revolutionIndex * Mathf.PI) / 180)*revolutionRadius;

		mainCamera.transform.position = new Vector3(
			gameBoardCenter.x + xOffset,
            mainCamera.transform.position.y,
            gameBoardCenter.z + zOffset
		);

        // Centers camera on the game board 
        mainCamera.transform.LookAt(gameBoardCenter);

        increaseRotationIndex();
	}

	public void revolve() {
        // Revolves the camera around the board
		revolutionIndex += 10;
	}

	private void increaseRotationIndex() {
		// If rotation index == 180, stop rotating.
		if (revolutionIndex == 0 || revolutionIndex == 180) {
			return;
		} else if (revolutionIndex == 360) {
            revolutionIndex = 0;
			return;
		} else {
            revolutionIndex += 10;
        }
	}

	public void toggleActiveCameras(){
		mainCamera.SetActive(!mainCamera.activeSelf);
		secondaryCamera.SetActive(!secondaryCamera.activeSelf);
	}
}
