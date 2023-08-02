using Newtonsoft.Json;

public class NewUserResponse
{
    [JsonProperty("auth_token")]
    public string AuthToken { get; set; }
}
public class PanelLayoutResponse
{
    [JsonProperty("numPanels")]
    public int NumPanels { get; set; }
    [JsonProperty("sideLength")]
    public int SideLength { get; set; }
    [JsonProperty("positionData")]
    public Positiondata[] PositionData { get; set; }
}

public class Positiondata
{
    [JsonProperty("panelId")]
    public int PanelId { get; set; }
    [JsonProperty("x")]
    public int X { get; set; }
    [JsonProperty("y")]
    public int Y { get; set; }
    [JsonProperty("o")]
    public int O { get; set; }
    [JsonProperty("shapeType")]
    public int ShapeType { get; set; }
}