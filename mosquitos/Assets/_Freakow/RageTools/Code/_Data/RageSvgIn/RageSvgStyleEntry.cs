using UnityEngine;

public class RageSvgStyleEntry : ScriptableObject {

    public string Command;
    public string Value;
    
    private RageSvgStyleEntry() {
        Command = "";
        Value = "";
    }

    public static RageSvgStyleEntry NewInstance() {
        return (RageSvgStyleEntry)CreateInstance(typeof(RageSvgStyleEntry));
    }
}