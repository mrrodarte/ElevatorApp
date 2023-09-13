# ElevatorApp
Elevator System Application that demonstrates basic elevator functionality using a variety of clean architecture and principles. This demo .NET7 core application showcases the SOLID principles, coded in the C# language, employing object-oriented design and fundamentals.

# Architecture
  The Elevator Application adopts a layered architecture, adhering to the separation of concerns as follows:
  - Presentation Layer (ElevatorConsole)
    - This is a C# console application that features a menu with options on how to operate the elevator.
  - Application Layer (ElevatorAPI)
    - This is a Minimal API application that serves as the communication layer, coupled with the use of various services to manage the elevator.
  - Domain Layer (ElevatorDomain)
    - This constitutes our domain library, which encompasses our entities and value objects—in this case, the elevator and its behaviors.

# Object-Oriented Design Principles Demonstrated by the App
  - The application upholds the fundamental SOLID principles: Single Responsibility, Open-Closed, Liskov Substitution, Interface Segregation, and Dependency Inversion. (It might make minor exceptions where it was logical for our application. Annotations are provided in the code comments.)
  - The application incorporates elements of Domain-Driven Design.
  - It employs event-driven design, utilizing a dispatcher method (specifically for logging).
  - The app also illustrates the use of certain design patterns, such as singleton and factory method (occasionally introducing some complexity to demonstrate expertise).
  - A Minimal API approach with endpoint management and middleware error handling is also in place.
  - The project includes unit testing (XUnit) of basic elevator operations located in the ElevatorSystem.Tests.
    
# How to run our application
The application relies on .NET7 run time for Windows Desktops and Core.  File are located in the publish folder or can be downloaded here:
- Windows Desktop .NET7 run time: https://download.visualstudio.microsoft.com/download/pr/2ce1cbbe-71d1-44e7-8e80-d9ae336b9b17/a2706bca3474eef8ef95e10a12ecc2a4/windowsdesktop-runtime-7.0.11-win-x64.exe
- .NET Core runtime: https://download.visualstudio.microsoft.com/download/pr/56fbfa65-4bf5-40a0-8935-57f09ab3c76b/d80afe4b74d01c07ca74c4670fcfa1f8/aspnetcore-runtime-7.0.11-win-x64.exe

Make sure your computer has these runtimes in order to run the application.

- Settings:
    - An initial settings file for the elevator is located in appsettings.json
    - ElevatorSettings: Here, you can define the maximum number of floors for your elevator and its weight capacity by adjusting MaxFloors and MaxWeight. Alternatively, you can retain the default settings.
    - LogFilePath: Specify the path where your elevator system will log operational data and any potential issues. Ensure the file adheres to the JSON syntax or retain the default settings.
    - ApiUrlHost:  This defines the API host that will listen to requests. The default can be kept, or if you have a preferred port, you can specify it. (Note: This demo uses only HTTP and not a secure protocol, intentionally keeping the application simple.)
- Launch:
  After finalizing your settings, initiate ElevatorConsole.exe. This is the entry point for our elevator application. Ensure you launch this and not elevatorAPI.exe. The ElevatorConsole will handle the startup of necessary modules for the elevator's operation.

# Operation
  - The application welcomes you and asks to enter your weight (for simplicity a constant weight will be assumed for all passengers)
  - Following that you can start making your request at the prompt.
  - Input the floor from which you're making the call and your intended direction (e.g., 5U, 2D). Enter 'Q' to exit.
  - Valid entries for Outside Requests [floor number][Desired Direction]
  - Valid entries for Inside Requests  [floor number]
  - Examples:
          - 5U: An outside request from floor 5 intending to go up.
          - 3: An inside request to stop at floor 3. (Direction is contingent on the elevator's current trajectory.)
          - Upon calling the elevator, when it arrives at your floor, you will board automatically.
          - Subsequently, select your destination. Once the destination is reached, you will disembark automatically.
    
          NOTES:  Be informed that a queuing service manages the elevator requests. Immediate responses might not be possible if challenges arise in processing your request (e.g., surpassing the weight limit). Always review the logs for insights and potential troubleshooting.

The application does not offer a console monitor to display the status of your request. Free file monitoring tools can be used to track your logfile, or you can simply open and refresh your log file after an operation to view the activities.
  
