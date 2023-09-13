using ElevatorDomain.ValueObjects;

public class ElevatorSettingsDto
{
    public int CurrentFloor { get; set; }
    public int MaxFloors { get; set; }
    public double CurrentWeight { get; set; }
    public double MaxWeight { get; set; }
    public ElevatorState State { get; set; }
    public Direction Direction { get; set; }
    public List<int> RequestedFloors { get; set; } = new List<int>();
}
