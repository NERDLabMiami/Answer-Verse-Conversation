using System;

[System.Serializable]
public class TextMessage : System.IEquatable<TextMessage>
{
    public Character from;        // Who sent the message (Character.NONE for player)
    public string message;        // The content of the message
    public string location;       // Where the message was sent or refers to

    public TextMessage positiveResponseBranch; // The single follow-up message if the player responds positively
    public TextMessage negativeResponseBranch; // The single follow-up message if the player responds negatively

    // Constructor
    public TextMessage(Character from, string message, string location)
    {
        this.from = from;
        this.message = message;
        this.location = location;
    }

    // Retrieve the next message based on the player's choice (positive or negative)
    public TextMessage GetNextMessage(bool isPositiveResponse)
    {
        return isPositiveResponse ? positiveResponseBranch : negativeResponseBranch;
    }

    // Equality checks for TextMessage
    public bool Equals(TextMessage other)
    {
        return from == other.from && message == other.message && location == other.location;
    }

    public override bool Equals(object obj)
    {
        if (obj is TextMessage otherMessage)
            return Equals(otherMessage);

        return false;
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 23 + from.GetHashCode();
            hash = hash * 23 + (message?.GetHashCode() ?? 0);
            hash = hash * 23 + (location?.GetHashCode() ?? 0);
            return hash;
        }
    }
}
