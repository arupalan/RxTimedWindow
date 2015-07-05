# RxTimedWindow
Practical Rx Simplyfying Concurrency,Scheduled Multi-Value handling and Unit Testing. 
You can get detailed information at the following link
http://www.alanaamy.net/rx/practical-rx-simplyfying-concurrencyscheduled-multi-value-handling-and-unit-testing/

# Building From Source
1. Move to your local git repository directory or any directory (with git init) in console.

2. Clone repository.

        git clone https://github.com/arupalan/RxTimedWindow.git

3. Move to source directory, update submodule and build.

        cd AlanAamy.Net.RxTimedWindow/
        git submodule update --init --recursive
        msbuild
        
#Installation

 * installutil /LogFile=svcinstalllog.txt AlanAamy.Net.RxTimedWindow.Service.exe
 
 #Debug 
 * You can execute with switch -console to see the logs on console
 ![Console Mode](http://www.alanaamy.net/wp-content/uploads/2015/07/RxTimedWindowErrors.png)
 
 #Diagnostics
 * The diagnostics snapshot of performance
  ![Diagnostic Mode](http://www.alanaamy.net/wp-content/uploads/2015/07/RxTimedWindowTestsMemory.png)

