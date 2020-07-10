using UnityEngine;

public class GroundBuilder : MonoBehaviour
{
    public GameObject DynamicCube;

    public GameObject StaticCube;

    // Start is called before the first frame update
    private void Start()
    {
        int amount = 50;
        for (int i = 0; i < amount; i++)
        {
            Vector3 pos = (i - amount/2) * (1 + Random.value) * Vector3.right + Vector3.up * (1 + Random.value);
            if(Random.value > 0.5f)continue;

            if (Random.value < 0.5f)
            {
                Instantiate(DynamicCube, pos, Quaternion.identity);
            }
            else
            {
                pos.y = 1f;
                Instantiate(StaticCube, pos, Quaternion.identity);
            }
        }
    }
}
