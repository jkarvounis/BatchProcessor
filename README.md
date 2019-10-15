# BatchProcessor

The goal of this project is to allow applications that are computationally expensive a simple way to distribute the workload amongst a group of local computers.  

The user's code will use the BatchProcessorAPI to execute "jobs" through a job scheduler.  This calls out to a remote server.  The remote server then distributes the jobs to other computers running the BatchProcessor service.  The BatchProcessorUI is a way to configure, test, and launch the BatchProcessor service.  The BatchProcessorServerUI is a way to configure, test, and launch the BatchProcessorServer.

Jobs are defined as an executable command with optional arguments.  A job can refer to a zipped payload of the executable and all necessary files.  There is a utility class in the API to pull the entire working directory of the application.  The payload is uploaded one at a time through the `JobScheduler`

# Batch Processor API

The API is on Nuget under ***BatchProcessorAPI***, or include the API project directly if not using Nuget.

This API currently targets .NET Standard 2.0, which is compatable with .NET Core 2.0 and later, .NET Framework 4.6.1 and later, and Mono 5.4 and later.

The API requires ***Newtonsoft.Json*** and ***RestSharp***.

## Code Sample

Please check out the ***TestApp*** for sample code.

A `JobScheduler` is the main point of entry for the API.  Initialize it with the Server IP and port.  Optionally the maximum number of parallel jobs can be specified, by default it is 256.
~~~~
JobScheduler scheduler = new JobScheduler(SERVER_IP, SERVER_PORT, MAX_PARALLEL);
~~~~

If a payload is required, it is uploaded once.  There are async methods available if needed.  The method returns true if it successfully uploaded to the server.  The parameter can be a zipped file as a byte-array in memory, or a path to the zip file.
~~~~
scheduler.UploadPayload(payload);
~~~~

Afterwards, remember to remove the payload from the server.  The remove method has the same method options as upload.
~~~~
scheduler.RemovePayload();
~~~~

There is a utility to get the payload of the current working directory.
~~~~
byte[] payload = PayloadUtil.CreatePayloadWithWorkingDirectory();
~~~~

The scheduler allows jobs to execute remotely.  A `Job` contains a name, executable name, executable arguments, and output file to return.  The executable can be the same as the calling executable if the arguments are handled properly.
~~~~
List<Job> jobs = new List<Job>();
for (int i = 0; i < 100; i++)
    jobs.Add(new Job($"Job-{i}", "TestApp.exe", i.ToString(), "output.txt"));
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

There are two installers.  One installer for the main server called BatchProcessorServer and one installer for all workers called BatchProcessor.  The server installer can be found in the ***Downloads*** folder.  

The server installer will install the BatchProcessorServer service and the BatchProcessorServerUI configuration tool.  Remember to make sure your firewall has the configured port open.

The worker installer will install the BatchProcessor service and the BatchProcessorUI configuration tool.  No firewall rules need to be modified for this install.  Make sure all dependencies are set up on these computers.  For example: Network Access, VC++ Redestributables, Python, etc.

By default, the installers will install a service and load a default port of 1200.

Please make sure .NET Framework 4.7.1 is installed on all machines.

## Controls

Both the BatchProcessorServerUI and BatchProcessorServer have the same configuration controls.

* *Start Local* - Start a local instance of the BatchProcessor.  The *Console* will show incoming jobs and loaded configuration.
* *Stop Local* - Stops the local instance of the BatchProcessor.
* *Install Service* - Installs the BatchProcessor as a Windows Service.  As a service, this will run always.
* *Uninstall Service* - Uninstalls the BatchProcessor as a Windows Service, but does not remove the application from the computer.
* *Start Service* - Starts the Windows Service version of the BatchProcessor.
* *Stop Service* - Stops the Windows Service version of the BatchProcessor.
* *Check Status* - Opens the server status page.

## Configuration (Server)

### Port
* A single integer port, 1200 by default.  This port is used by applications using the API to send jobs.

### Heartbeat (ms)
* A single integer in milliseconds, 5000 by default.  This is used to restart jobs if the workers go offline.  Make sure this is the same number as configured with workers.


## Configuration (Worker)

### Server Address
* Set this as the address of the server.  Make sure the server is on a static IP in the local network.

### Port
* A single integer port, 1200 by default.  This port is used to fetch jobs from the server.

### Max Local Slots
* Number of simultaneous jobs that can occur on the local machine.  Default value is (Number of Cores - 1).  Make sure this number is at least 1.

### Heartbeat (ms)
* A single integer in milliseconds, 5000 by default.  This is used to restart jobs if the workers go offline.  Make sure this is the same number as configured on the server.


### Buttons

Both the BatchProcessorServerUI and BatchProcessorServer have the same config buttons.

* Save - Save the configuration.  The BatchProcessor will automatically be restarted.
* Load - Load the current configuration, this will undo any local changes.

# Change log

* ***Version 2.1.0*** - Public release.  Improved server status page and data management.
* ***Version 2.0.2*** - Public release.  Fully tested and published on Nuget.
* Version 2.0.1 - Minor fixes
* Version 2.0.0 - Upgraded system to use REST API.  Split server into separate worker and server systems.
* ***Version 1.0.2*** - Public release.  Improved memory efficiency for large jobs.  Updated Server and Nuget.
* Version 1.0.1 - Updated Nuget package to include proper dependencies.  Server install in now more reliable, minor bug fixes.
* Version 1.0.0 - Initial public release!  Downloadable installer created.  Nuget package available.
* Version 0.0.1 - This is not released yet.  Still a work in progress.

*If you feel generous, and wish to support my projects:*

[![paypal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=TU7QHT7UL6PR4&currency_code=USD)
