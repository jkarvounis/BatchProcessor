# BatchProcessor

The goal of this project is to allow applications that are computationally expensive a simple way to distribute the workload amongst a group of local computers.  

The user's code will use the BatchProcessorAPI to execute "jobs" through a job scheduler.  This calls out to a remote server, which is running the BatchProcessor service.  The BatchProcessorUI is a way to configure, test, and launch the BatchProcessor service.

Jobs are defined as an executable command with optional arguments.  The job needs to contain a zipped payload of the executable and all necessary files.  There is a utility class in the API to pull the entire working directory of the application.

# Batch Processor API

The API is on Nuget under ***BatchProcessorAPI***, or include the API project directly if not using Nuget.

This API currently targets .NET Standard 2.0, which is compatable with .NET Core 2.0 and later, .NET Framework 4.6.1 and later, and Mono 5.4 and later.

The API requires ***Newtonsoft.Json*** and ***Newtonsoft.Json.Bson***.

## Code Sample

Please check out the ***TestApp*** for sample code.

A `JobScheduler` is the main point of entry for the API.  Initialize it with the Server IP and port.
~~~~
JobScheduler scheduler = new JobScheduler(SERVER_IP, SERVER_PORT);
~~~~

The scheduler allows jobs to execute remotely.  A `Job` contains a name, executable name, executable arguments, binary payload, and output file to return.  The executable can be the same as the calling executable if the arguments are handled properly.
~~~~
List<Job> jobs = new List<Job>();
for (int i = 0; i < 100; i++)
    jobs.Add(new Job($"Job-{i}", "TestApp.exe", i.ToString(), payload, "output.txt"));
~~~~

There is a utility to get the payload of the current working directory.
~~~~
byte[] payload = PayloadUtil.CreatePayloadWithWorkingDirectory();
~~~~

To execute jobs, call the `JobScheduler` with a list of `Job` elements.  There is an `Action<JobResponse>` that is called on job completion or failure.
~~~~
scheduler.ScheduleAll(jobs, response => 
{     
    //Add lock statement to cleanup console writing       
    Console.WriteLine($"Job Response {response.Completed}: {response.Name}");

    //Output from the file
    string returnFile = "Empty";
    if (response.ReturnFile != null)
        returnFile = System.Text.Encoding.Default.GetString(response.ReturnFile);
    Console.WriteLine($"File: [{returnFile}]);

    //Output from the executable
    Console.WriteLine($"Output: [{response.ConsoleOutput}] Error: [{response.ConsoleError}]");

    if (response.Completed)
        //Increment a counter if you want    
});
~~~~

# Server Application

The server installer can be found in the ***Downloads*** folder.  The installer will install the BatchProcessor service, the BatchProcessorUI configuration tool, and add Windows Firewall exceptions.  

Please make sure .NET Framework 4.7.1 is installed on the server.

## Controls

* *Start Local* - Start a local instance of the BatchProcessor, make sure the service is not active, they will interfere.  The *Console* will show incoming jobs and loaded configuration.
* *Stop Local* - Stops the local instance of the BatchProcessor.
* *Install Service* - Installs the BatchProcessor as a Windows Service.  As a service, this will run always.
* *Uninstall Service* - Uninstalls the BatchProcessor as a Windows Service, but does not remove the application from the computer.
* *Start Service* - Starts the Windows Service version of the BatchProcessor.
* *Stop Service* - Stops the Windows Service version of the BatchProcessor.

## Configuration

### Mode
* *Server Mode* - there should only be one server on the network, this is the master node that controls job flow.
* *Worker Mode* - a worker adds more computing power to an existing server.  You must put an address in the *Server Address* field.  Make sure the *Worker Port* matches the configuration on the server.

### Job Port
* A single integer port, 1200 by default.  This port is used by applications using the API to send jobs.  If in *Worker Mode*, this port is used by the server to send jobs.
 
### Worker Port
* A single integer port, 1201 by default.  This port is specified by both workers and server to establish communication.  Make sure when adding workers that this port matches the server.

### Server Address
* Only used in *Worker Mode* - set this as the address of the server.  Make sure the server is on a static IP in the network.

### Max Local Slots
* Number of simultaneous jobs that can occur on the local machine.  Default value is (Number of Cores - 1).  Make sure this number is at least 1.

### Buttons

* Save - Save the configuration.  The BatchProcessor must be restarted in order for the settings to load.
* Load - Load the current configuration, this will undo any local changes.

# Change log

* Version 1.0 - Initial public release!  Downloadable installer created.  Nuget package available.
* Version 0.0 - This is not released yet.  Still a work in progress.

*If you feel generous, and wish to support my projects:*

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=TU7QHT7UL6PR4&currency_code=USD)
