using UnityEngine;

public class Dice : MonoBehaviour {
    [SerializeField] private string diceName;
    [SerializeField] private int[] values;
    [SerializeField] private float rotationSpeed;
    [SerializeField] private float tiltAmplitude;
    [SerializeField] private float tiltFrequency;
    private float tiltTime = 0f;
    public string DiceName => diceName;
    public int[] Values => values;

    void Update() {
        Spin();
    }

    private void Spin() {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

        tiltTime += Time.deltaTime * tiltFrequency;
        var tiltAngle = Mathf.Sin(tiltTime) * tiltAmplitude;

        transform.rotation = Quaternion.Euler(tiltAngle, transform.rotation.eulerAngles.y, 0);
    }
}