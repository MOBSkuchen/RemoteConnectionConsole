# RemoteConnectionConsole
is a console application for windows - similar to PuTTY - which allows a user to remotely run commands on another computer using SSH
## Why
There really is not a good reason for this.
I just didn't like the console in which PuTTY runs that much and could not find a way to change it.
## Further development
I plan on creating a GUI for this app and an installer for both products.
## Usage
1. Download the latest release from the [Release page](https://github.com/MOBSkuchen/RemoteConnectionConsole/releases/tag/Release)
2. Create a JSON or YML file containing the instance data
``` yaml
host: xxx.xx.xxx.xxx
username: myuser
password: mypassword
port: 22
isKeyAuth: false
````
    
Note: If you want to use a key file for a password set isKeyAuth to true and put the file path into the password field

3. Run the program:
    
``` 
RemoteConnectionConsole instance.yml
```
