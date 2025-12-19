using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public float delay = 5f; // duración de la bomba
    [SerializeField] AudioClip clip; //sonido de explosion
    void Start()
    {
        //Explota tras 5s reproduciendo un audio
        Invoke("Explode", delay);
    }

    void Explode()
    {
        AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        
        Destroy(gameObject);
    }
    
}
