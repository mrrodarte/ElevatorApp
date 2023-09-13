using ElevatorDomain.ValueObjects;

namespace ElevatorAPI.DTOs
{
    public class FloorRequestDto
    {
        public int RequestFromFloor { get; set; }
        public Direction Direction { get; set; }
        public RequestType RequestType { get ; set; }
        public double PassengerWeight { get; set; }
    }
}