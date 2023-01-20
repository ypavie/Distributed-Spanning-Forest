using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class UAV_Behaviour : MonoBehaviour
{
    #region uav properties
        public float speed = 2.0f; // The speed at which the UAV moves
        public float range; // The range of the UAV's movement
        private Vector3 spawn; // The initial position of the UAV
        private Vector3 destination; // The destination position of the UAV
        
        private bool lifting = true; // Indicates whether the UAV is in the process of lifting off or not
        private bool moveRandomly = false; // Indicates whether the UAV is moving randomly or not
    #endregion uav

    #region rendering
        MeshRenderer meshRenderer;
        public Material redMaterial;
        public Material blackMaterial;
    #endregion rendering

    #region distributed spanning forest
        public int ID;
        public float delayBetweenRounds = 1f;
        [SerializeField] private NodeStatus status;
        [SerializeField] private int parent;
        [SerializeField] private int score;
        [SerializeField] private List<int> children;
        [SerializeField] private List<int> neighbors;
        [SerializeField] private Dictionary<int, GameObject> neighborsGameObject;

        [SerializeField] private Dictionary<int, Message> mailbox;
        [SerializeField] private Message outMessage;
    #endregion distributed spanning forest

    void Start()
    {
        children = new List<int>();
        neighbors = new List<int>();
        mailbox = new Dictionary<int, Message>();
        neighborsGameObject = new Dictionary<int, GameObject>();

        status = NodeStatus.T;
        parent = -1;

        meshRenderer = gameObject.GetComponent<MeshRenderer>();     
    }

    void Update()
    {
        // Move the UAV randomly within the specified range
        MoveRandomly();

        // Update the lines (connections) to other UAVs
        UpdateLines();

        // Change the material of the UAV based on its status (root or not)
        meshRenderer.material = (status == NodeStatus.T) ? redMaterial : blackMaterial;
    }

    /// <summary>
    /// Lifts the UAV to a random height and starts the main algorithm.
    /// </summary>
    public void Lift()
    {
        score = ID;

        // Prepare the initial message (HELLO)
        outMessage = new Message(ID, NodeStatus.T, MessageType.HELLO, -1, score);

        // Save the initial position of the UAV and set the destination to a random height
        spawn = transform.position;
        destination = transform.position;
        destination.y = transform.position.y + Random.Range(5, 10);

        // Move the UAV toward the destination
        while (Vector3.Distance(transform.position, destination) < 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);    
        }

        lifting = false;
        moveRandomly = true;

        // Invoking the "Broadcast" method every (delayBetweenRounds / 2) seconds. 
        // Only half the delay now, the other half will be used to synchronize the UAV's mailbox before processing the messages.
        InvokeRepeating("Broadcast", 1f, delayBetweenRounds / 2);
    }

    /// <summary>
    /// Makes the UAV move randomly in the x, y and z axis within a certain range.
    /// </summary>
    private void MoveRandomly()
    {
        if (moveRandomly)
        {
            // Move the UAV towards its destination
            transform.position = Vector3.MoveTowards(transform.position, destination, Time.deltaTime * speed);
            // If the UAV has reached its destination
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                // Set a new random destination within the range
                destination.x = Random.Range(spawn.x - 5, spawn.x + 5);
                destination.y = Random.Range(spawn.y + 5, spawn.y + 10);
                destination.z = Random.Range(spawn.z - 5, spawn.z + 5);
            }
        }
    }

    /// <summary>
    /// Update the lines (connections) to other UAVs in the scene view (not visible in game).
    /// </summary>
    private void UpdateLines()
    {
        foreach(int id in neighbors)
        {
            GameObject go = neighborsGameObject[id];

            if (!children.Contains(id))
            {
                if (id == parent)
                {
                    // Draw red line between parent and child
                    Color color = Color.red;
                    Vector3 direction = (go.transform.position - transform.position).normalized;
                    float arrowLength = Vector3.Distance(transform.position, go.transform.position);

                    Debug.DrawRay(transform.position, direction * arrowLength, color);
                }
            }
        }
    }
    /// <summary>
    /// Implements the Broadcast step of the distributed algorithm described in the paper "Maintaining a Distributed Spanning Forest in Highly Dynamic Networks". 
    /// </summary>
    private void Broadcast()
    {
        // Send messages to all neighboring nodes
        SEND();

        // A small delay is added here to ensure that all UAVs' mailboxes are synchronized before processing received messages
        StartCoroutine(WaitAndContinue());
    }

    IEnumerator WaitAndContinue()
    {
        // need to wait a little bit to make sure mailboxs are synchonized 
        yield return new WaitForSeconds(delayBetweenRounds / 2);

        neighbors = mailbox.Keys.ToList();
        children = children.Intersect(neighbors).ToList();

        // Updates the dictionary of neighboring UAV GameObjects
        foreach (int id in neighbors) 
        {
            if (!neighborsGameObject.ContainsKey(id)) {
                neighborsGameObject.Add(id, GameObject.Find("UAV " + id));
            }
        }

        // Regenerates a token if parent link is lost
        if(status == NodeStatus.N && !neighbors.Contains(parent))
        {
            BECOME_ROOT();
        }

        // Checks if the outgoing FLIP or SELECT (if any) was successful
        if((outMessage.action == MessageType.FLIP || outMessage.action == MessageType.SELECT) && neighbors.Contains(outMessage.targetID))
        {
            ADOPT_PARENT(outMessage);
        }

        // Processes the received messages
        int contender = -1;
        int contenderScore = 0;
        foreach (var message in mailbox.Values)
        {
            if (message.targetID == ID)
            {
                if (message.action == MessageType.FLIP)
                {
                    BECOME_ROOT();
                }
                ADOPT_CHILD(message); // called for both FLIP or SELECT
            }
            else if (message.status == NodeStatus.T && message.score > contenderScore)
            {
                contender = message.senderID;
                contenderScore = message.score;
            }
        }

        // Prepares the message to be sent
        outMessage = null;

        if(status == NodeStatus.T)
        {
            if(contenderScore > score)
            {
                PREPARE_MESSAGE(MessageType.SELECT, contender);
            }
            else if(children.Count > 0)
            {
                PREPARE_MESSAGE(MessageType.FLIP, children[Random.Range(0, children.Count)]);
            }
        }

        if(outMessage == null)
        {
            PREPARE_MESSAGE(MessageType.HELLO, -1);
        }
        
        mailbox.Clear();
    }

    private void SEND()
    {
        // Detect UAVs within range
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask("UAV"));

        foreach (Collider collider in collidersInRange)
        {
            UAV_Behaviour uav = collider.gameObject.GetComponent<UAV_Behaviour>();
            if (uav != null && GetComponent<Collider>() != collider)
            {
                // Send the message to the UAV
                uav.RECEIVE(outMessage);
            }
        }
    }

    private void RECEIVE(Message message)
    {
        if (mailbox.ContainsKey(message.senderID))
        {
            mailbox[message.senderID] = message;
        }
        else
        {
            mailbox.Add(message.senderID, message);
        }
    }

    private void BECOME_ROOT()
    {
        status = NodeStatus.T;
        parent = -1;
    }

    private void ADOPT_PARENT(Message outMessage)
    {
        status = NodeStatus.N;
        parent = outMessage.targetID;
        if (outMessage.action == MessageType.FLIP)
        {
            children.Remove(parent);

            int targetScore = mailbox[outMessage.targetID].score;
            score = score < targetScore ? score : targetScore;
        }
    }

    private void ADOPT_CHILD(Message message)
    {
        if (!children.Contains(message.targetID))
        {
            children.Add(message.senderID);
        }

        if (message.action == MessageType.FLIP)
        {
            score = score > message.score ? score : message.score;
        }
    }

    private void PREPARE_MESSAGE(MessageType action, int targetID)
    {
        switch(action)
        {
            case MessageType.SELECT:
                outMessage = new Message(ID, NodeStatus.N, MessageType.SELECT, targetID, score);
                break;
            case MessageType.FLIP:
                outMessage = new Message(ID, NodeStatus.T, MessageType.FLIP, targetID, score);
                break;
            case MessageType.HELLO:
                outMessage = new Message(ID, NodeStatus.N, MessageType.HELLO, targetID, score);
                break;
        }
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}

