using UnityEngine;

public sealed class RotatePot : MonoBehaviour
{
	[SerializeField]
	private float speed = 20f;
	public bool IsRunning { get; private set; }
	
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
}
