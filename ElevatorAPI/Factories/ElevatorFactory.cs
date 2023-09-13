using ElevatorDomain.Entities;
using ElevatorDomain.Models;
using ElevatorDomain.Interfaces;
using Microsoft.Extensions.Options;

namespace ElevatorAPI.Factories
{
    //The Factory Pattern
    //Comments so you can know my thought process
    //I was having an error related to dependency injection, where the application was unable to
    //contruct and instance of the ElevatorService, it could not resolve a dependency on the 
    //Elevator Entity. I would have to register Elevator in the DI, but Elevator is a domain
    //entity.  I did not feel it was the correct approach.  And took advantage of showing
    //a creational pattern the Factory Method, so that the ElevatorService would not depend of the
    //elevator but on a factory that will create it.
    public class ElevatorFactory : IElevatorFactory
    {
        private readonly ElevatorSettings _settings;
        private Elevator? _elevatorInstance;

        public ElevatorFactory(IOptions<ElevatorSettings> settingsOptions)
        {
            _settings = settingsOptions.Value;
        }

        public Elevator CreateElevator()
        {
            if (_elevatorInstance == null)
            {
                _elevatorInstance = new Elevator(_settings.MaxFloors, _settings.MaxWeight);
            }
            return _elevatorInstance;
        }
    }
}