using ElevatorDomain.ValueObjects;

namespace ElevatorAPI.DTOs
{
    public class SelectFloorDto
    {
        public int SelectedFloor { get; set; }
        public Direction Direction { get; set; }
        public RequestType RequestType { get; set; }
    }
}