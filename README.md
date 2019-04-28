# BatchProcessor

The goal of this project is to allow applications that are computationally expensive a simple way to distribute the workload amongst a group of local computers.  

The user's code will use the BatchProcessorAPI to execute "jobs" through a job scheduler.  This calls out to a remote server, which is running the BatchProcessor service.  The BatchProcessorUI is a way to configure, test, and launch the BatchProcessor service.

Jobs are defined as an executable command with optional arguments.  The job needs to contain a zipped payload of the executable and all necesary files.  There is a utility class in the API to pull the entire working directory of the application.

# Change log

Version 0.0 - This is not released yet.  Still a work in progress.
