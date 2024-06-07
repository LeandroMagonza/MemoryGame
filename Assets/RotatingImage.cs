using UnityEngine;

public class RotatingImage : MonoBehaviour
{
    public float maxSpeed = 100f; // Velocidad máxima en grados por segundo
    public float minSpeed = 10f; // Velocidad mínima en grados por segundo
    public float acceleration = 20f; // Aceleración en grados por segundo cuadrado

    public float maxScale = 2f; // Escala máxima en X e Y
    public float scaleTime = 1f; // Tiempo para completar un ciclo de escalado

    private float currentSpeed = 0f; // Velocidad actual
    private bool accelerating = true; // Indicador de si está acelerando
    private Vector3 originalScale; // Escala original del objeto
    private float scaleTimer = 0f; // Temporizador para el escalado
    private bool scalingUp = true; // Indicador de si está escalando hacia arriba

    private void Start()
    {
        originalScale = transform.localScale; // Guardar la escala original
    }

    private void Update()
    {
        // Actualizar la velocidad actual dependiendo del estado
        if (accelerating)
        {
            currentSpeed += acceleration * Time.deltaTime;
            if (currentSpeed >= maxSpeed)
            {
                currentSpeed = maxSpeed;
                accelerating = false;
            }
        }
        else
        {
            currentSpeed -= acceleration * Time.deltaTime;
            if (currentSpeed <= minSpeed)
            {
                currentSpeed = minSpeed;
                accelerating = true;
            }
        }

        // Rotar la imagen
        transform.Rotate(Vector3.forward, currentSpeed * Time.deltaTime);

        // Actualizar la escala del objeto
        scaleTimer += Time.deltaTime;
        float scaleLerp = Mathf.PingPong(scaleTimer, scaleTime) / scaleTime;
        float currentScale = Mathf.Lerp(1f, maxScale, scaleLerp);
        transform.localScale = originalScale * currentScale;
    }
}