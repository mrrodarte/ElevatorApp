namespace ElevatorDomain.ValueObjects
{
    public static class Passenger
    {
        // Assuming a constant weight, this can be changed based on requirements.
        public static double Weight = 160.0; //default value pounds

        public static void SetWeight(double weight)
        {
            Weight = weight;
        }
    }
}