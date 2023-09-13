using ElevatorDomain.Entities;

namespace ElevatorDomain.Interfaces
{
    public interface IElevatorFactory
    {
        public Elevator CreateElevator();
    }
}