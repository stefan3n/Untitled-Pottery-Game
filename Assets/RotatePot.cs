using UnityEngine;

public sealed class RotatePot : MonoBehaviour
{
	[SerializeField]
	private float speed = 20f;

	private bool IsRunning { get; set; }
	
	void Update()
	{
		if(IsRunning)
		{
			transform.Rotate(Time.deltaTime * speed * Vector3.up);
		}
	}
	public void ToggleWheel(){
		IsRunning = !IsRunning;
    }

	public bool IsRotating()
	{
		return IsRunning;
	}
}
