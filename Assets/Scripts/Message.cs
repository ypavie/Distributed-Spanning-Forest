public enum NodeStatus { T, N }; // T = having token, N = not having token
public enum MessageType { SELECT, FLIP, HELLO };

public class Message
{
    public int senderID;
    public MessageType action;
    public int targetID;
    public NodeStatus status;
    public int score;
    
    public Message(int senderID, NodeStatus status, MessageType action, int targetID, int score)
    {
        this.senderID = senderID;
        this.status = status;
        this.action = action;
        this.targetID = targetID;
        this.score = score;
    }
}
