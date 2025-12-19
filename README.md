# Descrición

Escena con una multitud de NPC y un jugador que pone bombas pulsando E. Cuando los NPC ven la bomba, huyen durante 5s en direccion a la zona de reunión

# Título principal

**Escena bomba**

## Subtítulo

Lista de cambios:

-Añadido al script *PlayerController* una función que instancia una bomba al pulsar E.

-Creado un prefab para una bomba y para un NPC llamado Fuxidor que huye de ella.

-Creado un script *Bomb* que destruye la bomba tras 5s desde que se instanció y reproduce un clip de audio.

-Añadido audio de explosión para cuando la bomba explota.

-Añadida una escena nueva donde se define un punto de reunión hacia el cual los NPC Fuxidor se dirigen en caso de detectar una bomba.

-Creado un script *FuxidorController* que establece el comportamiento de los mismos. Los fuxidores deambulan por la escena al azar y cuando una bomba entra en su campo de visión aumentan su velocidad y huyen hacia el lugar de reunión durante 5 segundos. Transcurrido este tiempo vuelven a deambular. 

[Ligazón](https://github.com/Xabi66/IAFiniteStateMachineSimple/blob/main/Assets/Scripts/FuxidorController.cs)


```csharp
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))] // Require un NavMeshAgent para o movemento
public class FuxidorController : MonoBehaviour
{
    enum State { Patrol, Escape } //O patrulla o huye hacia la zona segura
    State currentState = State.Patrol; //Al principio patrulla

    [SerializeField] GameObject meetingPoint; //Punto de reunion
    float escapeSpeed = 10f; //Velocidad de huida
    float patrolSpeed = 5f; //Velocidad de patrulla
    float patrolDistance = 10f;// Radio de patrulla
    float patrolWait = 3f;// Tempo de espera entre puntos de patrulla
    float patrolTimePassed = 0; // Tempo pasado desde o último cambio de punto de patrulla

    float fovDistance = 20.0f; // Distancia do campo de visión
    float fovAngle = 120.0f; // Ángulo do campo de visión (en graos)
    
    float escapeDuration = 5f; // tiempo que huye
    float escapeTimer = 0f;

    void Start()
    {
        patrolTimePassed = patrolWait; // Inicia listo para escoller un punto de patrulla
        GetComponent<NavMeshAgent>().speed=patrolSpeed; //establece la velocidad de patrulla
    }

    void Update()
    {
        if (ICanSee())
        {
            currentState = State.Escape; //Comienza a escapar
            escapeTimer = escapeDuration; //Establece la duracion del escape
            GetComponent<NavMeshAgent>().speed=escapeSpeed; //cambia a velocidad de huida
        }
        else if (currentState == State.Escape) //Si ya no lo ve y estaba escapando
        {
            escapeTimer -= Time.deltaTime; //Reduce el tiempo de escape
            Debug.Log(escapeTimer);
            if (escapeTimer <= 0)//Si se acabo el tiempo vuelve a patrullar
            {
                currentState = State.Patrol;
                GetComponent<NavMeshAgent>().speed=patrolSpeed; //cambia a velocidad de patrulla
            }
        }
        //Segun el estado patrulla o huye
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;

            case State.Escape:
                Escape();
                break;
        }
    }


    //Para deambular de forma aleatoria
    void Patrol() // Patrulla por puntos aleatorios
    {
        patrolTimePassed += Time.deltaTime; // Incrementa o contador de tempo

        if (patrolTimePassed > patrolWait) // Se pasou o tempo de espera
        {
            patrolTimePassed = 0; // Reinicia o contador

            Vector3 patrollingPoint = transform.position;//Deambula alrededor de su ubicación actual
            patrollingPoint+= new Vector3(Random.Range(-patrolDistance, patrolDistance), 0, Random.Range(-patrolDistance, patrolDistance)); // Xera un punto aleatorio

            GetComponent<NavMeshAgent>().SetDestination(patrollingPoint); // Establece o destino
        }
    }
    
    //Comprueba si se ve alguna bomba
    bool ICanSee()
    {
        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");//Busca objetos con la etiqueta bomb

        foreach (GameObject bomb in bombs)
        {
            Vector3 direction = bomb.transform.position - transform.position; // Dirección á bomba
            float angle = Vector3.Angle(direction, transform.forward);  // Ángulo entre fronte e bomba

            RaycastHit hit;
            if (
                Physics.Raycast(transform.position, direction, out hit) && // Lanza un raio cara á bomba
                hit.collider.CompareTag("Bomb") && // O raio acertou á bomba?
                direction.magnitude < fovDistance &&  // Está a bomba dentro da distancia do FOV?
                angle < fovAngle  // Está a bomba dentro do ángulo do FOV?
            )
            {
                return true;
            }
        }
        return false;
    }

    //Escapa hacia el lugar seguro
    void Escape()
    {
        if (meetingPoint == null) return;
        //Accede a NavMeshAgent y fija el destino
        GetComponent<NavMeshAgent>().SetDestination(meetingPoint.transform.position);
    }
}


```

```csharp
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

```