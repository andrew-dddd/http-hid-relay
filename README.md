# Http Hid Relay

A .NET application that exposes a simple HTTP API to control inexpensive HID relay boards through docker container.

## Prerequisites to run on Windows

### 1. WSL2
Ensure you are running **WSL2** with a kernel version of **5.15.150.1** or higher (as it includes HID support).
*   **Check kernel version:** Run 
    ```powershell
    wsl --version
    ```
*   **Verify WSL version:** All installed Linux distributions must be set to version 2. Check this by running 
    ```powershell
    wsl -l -v
    ```
*   **Convert distro:** If a distribution is running on version 1, convert it using:
    ```powershell
    wsl --set-version <distro_name> 2
    ```

### 2. usbipd-win
Required to pass USB devices from Windows to WSL.
*   **Installation:** Download from [GitHub Releases](https://github.com/dorssel/usbipd-win/releases/tag/v5.3.0).
*   **Guide:** Refer to the [official Microsoft documentation](https://learn.microsoft.com/en-us/windows/wsl/connect-usb).

**Connecting your board:**
1.  List available devices: 
    ```powershell
    usbipd list
    ```
2.  Identify your board in the list (look for "USB Input Device" or entries mentioning "HID" or specific hardware vendors).
3.  **Bind** the device: 
    ```pwoershell 
    usbipd bind --busid <busid>
    ```
4.  **Attach** it to WSL: 
    ```powershell
    usbipd attach --wsl --busid <busid>
    ```

5. **Hot-plugging (Automatic Reconnect)**
    By default, when you unplug the device or restart Windows, the connection to WSL is lost. To keep the device attached automatically, use the `--auto-attach` flag in a separate terminal:

    ```powershell
    usbipd attach --wsl --busid <busid> --auto-attach
    ```

### 3. Docker Deployment
Once the environment is ready, navigate to the `.csproj` directory and run the following commands:

*   **Build the image:**
    ```bash
    docker build --no-cache -t http-hid-relay .
    ```
*   **Run the container (Recommended: Least Privilege Method)**
    ```bash
    docker run --rm -it --device /dev/hidraw0:/dev/hidraw0 -p 8080:8080 http-hid-relay
    ```    
*   **Run the container (Alternative: Full Access Method):**
    ```bash
    docker run --rm -it --privileged -v /dev:/dev -p 8080:8080 http-hid-relay
    ```

*   **Run the container with hot plugging support:**
    
    log into wsl and then run:
    ```bash
    ls -l /dev/hidraw*
    ```

    you should get output like: 
    ```
                            v    
    crw------- 1 root root 240, 0 Apr 15 22:11 /dev/hidraw0
    ```

    the number marked with arrow is a number which needs to be passed in following command: 

    ```bash                                                   
                                                              v
    docker run --rm -it -v /dev:/dev --device-cgroup-rule='c 240:* rwm' -p 8080:8080 -e RelayConfig__Relays='1,2,3,4' http-hid-relay
    ```