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
