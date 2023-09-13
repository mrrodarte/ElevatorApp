using ElevatorDomain.ValueObjects;

namespace ElevatorDomain.Models
{
    //This class is to return settings information that will be requested via API
    //The API will request settings to the elevator service and send it to the caller in a dto
    public class ElevatorSettings
    {
        public int CurrentFloor { get; set; }
        public Direction Direction { get; set; }
        public ElevatorState State { get; set; }
        public int MaxFloors { get; set; }
        public double MaxWeight { get; set; }
        public double Weight { get; set; }
    }

}