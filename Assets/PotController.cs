using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PotController : MonoBehaviour
{
	[Header("VR Controller")]
	[SerializeField]
	private Transform controllerTransform;
	
	[SerializeField]
	private InputActionProperty triggerAction;
	
	[SerializeField]
	private float maxInteractionDistance = 2f;
	
	[Header("Pottery")]
	[SerializeField]
	private Potter pottery;
	
	[SerializeField]
	private GameObject selector;
	
	[Header("Visual Feedback")]
	[SerializeField]
	private Color hoverColor = new Color(0f, 1f, 1f, 0.3f);
	
	[SerializeField]
	private Color activeColor = new Color(0f, 1f, 0f, 0.5f);
	
	private Ray ray;
	private RaycastHit hit;
	private bool isEditing;
	private int selectedRing;
	private Vector3 previousHitPoint;
	private bool wasTriggerPressed;
	private Renderer selectorRenderer;
	private LayerMask raycastMask;

	void Start()
	{
		if (selector != null)
		{
			selector.SetActive(false);
			selectorRenderer = selector.GetComponent<Renderer>();
			
			selector.layer = LayerMask.NameToLayer("Ignore Raycast");
			foreach (Transform child in selector.GetComponentsInChildren<Transform>())
			{
				child.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
			}
		}
		
		raycastMask = ~LayerMask.GetMask("Ignore Raycast");
	}

	void Update()
	{
		float triggerValue = triggerAction.action?.ReadValue<float>() ?? 0f;
		bool triggerPressed = triggerValue > 0.5f;
		
		ray = new Ray(controllerTransform.position, controllerTransform.forward);
		
		if (Physics.Raycast(ray, out hit, maxInteractionDistance, raycastMask))
		{
			if (hit.collider.gameObject != pottery.gameObject)
			{
				HideSelector();
				isEditing = false;
				wasTriggerPressed = triggerPressed;
				return;
			}
			
			float localY = hit.point.y - pottery.transform.position.y;
			int hoveredRing = 0;
			
			for (int i = 0; i < pottery.ringsCount; i++)
			{
				float ringY = i * pottery.ringHeight;
				float nextRingY = (i + 1) * pottery.ringHeight;
				
				if (localY >= ringY && localY < nextRingY)
				{
					hoveredRing = i;
					break;
				}
			}
			
			selectedRing = hoveredRing;
			ShowSelector(triggerPressed);
			
			if (triggerPressed && !wasTriggerPressed)
			{
				isEditing = true;
				previousHitPoint = hit.point;
			}
			else if (!triggerPressed && wasTriggerPressed)
			{
				isEditing = false;
			}
			
			if (isEditing && triggerPressed)
			{
				Vector3 controllerRight = controllerTransform.right;
				Vector3 movement = hit.point - previousHitPoint;
				float delta = Vector3.Dot(movement, controllerRight);
				
				pottery.ringsRadius[selectedRing] += delta * 2f;
				pottery.ringsRadius[selectedRing] = Mathf.Clamp(pottery.ringsRadius[selectedRing], 0.1f, 2f);
				
				previousHitPoint = hit.point;
			}
		}
		else
		{
			HideSelector();
			isEditing = false;
		}
		
		wasTriggerPressed = triggerPressed;
	}
	
	void ShowSelector(bool isActive)
	{
		if (selector == null)
			return;
			
		selector.SetActive(true);
		
		float selectorY = selectedRing * pottery.ringHeight;
		selector.transform.position = pottery.transform.position + new Vector3(0, selectorY + pottery.ringHeight * 0.5f, 0);
		selector.transform.localScale = new Vector3(
			pottery.ringsRadius[selectedRing] * 2.5f, 
			pottery.ringHeight * 0.5f, 
			pottery.ringsRadius[selectedRing] * 2.5f
		);
		
		if (selectorRenderer != null && selectorRenderer.material != null)
		{
			Color targetColor = isActive ? activeColor : hoverColor;
			selectorRenderer.material.color = targetColor;
		}
	}
	
	void HideSelector()
	{
		if (selector != null)
			selector.SetActive(false);
	}

	void OnDrawGizmos()
	{
		if (controllerTransform == null)
			return;
			
		Gizmos.color = Color.blue;
		Gizmos.DrawRay(ray.origin, ray.direction * maxInteractionDistance);

		if (isEditing)
		{
			Gizmos.color = Color.green;
			Gizmos.DrawSphere(hit.point, 0.02f);
		}
	}
}