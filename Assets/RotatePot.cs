using UnityEngine;

public sealed class RotatePot : MonoBehaviour
{
	[SerializeField]
	private float speed = 20f;

	void Update()
	{
		transform.Rotate(Time.deltaTime * speed * Vector3.up);
	}
}