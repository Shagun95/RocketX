using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{

	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	public Transform camTransform;

	// How long the object should shake for.
	public float shakeDuration = 0f;

	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.7f;
	public float decreaseFactor = 1.0f;

	Vector3 reset;

	void Awake()
	{
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
		}
		reset = new Vector3(0, 0, 0);
	}

	void Update()
	{
		//Debug.Log(shakeDuration);
			if (shakeDuration > 0)
			{
				camTransform.localPosition += Random.insideUnitSphere * shakeAmount * Time.timeScale;

				shakeDuration -= Time.deltaTime * decreaseFactor;
			}
			else
			{
				shakeDuration = 0f;
				StartCoroutine(getBackToPosition());
			}
	}

	/// <summary>
    /// Shake effect da esterno
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="magnitude"></param>
	public void startShake(float duration, float magnitude)
    {
		//Debug.Log("impost" + duration);
		//non serve impostare un flag, perchè dipende tutto da shakeDuration che rimane a 0 fino a quando non rimachiamato
		shakeDuration = duration;
		shakeAmount = magnitude;
    }

	/// <summary>
    /// Torno alla posizione della camera originale
    /// </summary>
    /// <returns></returns>
	IEnumerator getBackToPosition()
    {
		if (camTransform.localPosition == reset)
        {
			yield break;
        }
		else
        {
			camTransform.localPosition = Vector3.MoveTowards(camTransform.localPosition, reset, Time.deltaTime*10);
		}
	}
}