# 🍋 LemonServer

**The hardware monitor and FPS web server for Windows.**

LemonServer is a lightweight utility that exposes real-time system stats and FPS data through a simple web server. It returns system information in JSON format, making it easy to integrate with dashboards, overlays, or remote monitoring tools.

## 📦 Features

- Displays real-time:
  - CPU and GPU temperatures
  - CPU and GPU loads
  - RAM and VRAM usage
  - Current FPS for the topmost application on the screen
- JSON API over HTTP
- Lightweight and easy to use
- No installation required
- Customizable port
- Perfect for game overlays, stream HUDs, or system dashboards

## 🖥️ Example JSON Output

```json
{
  "cpuName": "11th Gen Intel Core i5-11400H",
  "gpuName": "NVIDIA GeForce RTX 3060 Laptop GPU",
  "totalMemory": 16235,
  "availableMemory": 2283,
  "cpuTemp": 72.3333359,
  "cpuLoad": 89.6999969,
  "gpuTemp": 71,
  "gpuLoad": 37.7296906,
  "gpuTotalVram": 6144,
  "gpuAvailVram": 1340,
  "currFps": 106.343132
}
```

## 🚀 Usage

- Starting the Server
```bash
# LemonServer.exe {port}
```  
*Replace {port} with your desired port number. Defaults to 8080 if no port is provided.*  

- Accessing the API
```
http://localhost:{port}
```
*The server will respond with live system statistics in JSON format.*

- Stopping the server
```
http://localhost:{port}/exit
```

---

## ⚙️ Requirements
- Operating System: **Windows only**
- .NET Framework: Version 4.7.2 or higher

⚠️ Make sure you run LemonServer as **Administrator**

## 🔗 Related Repositories and Website

The complete Lemon Monitor ecosystem includes a Windows desktop application and firmware:

* **Official Website:** [lemon.yourdan.uk](http://lemon.yourdan.uk)
* **Windows Application:** [LemonMonitor App](https://github.com/thr33bricks/LemonMonitor)
* **LemonMonitor Firmware:** [LemonMonitor Firmware](https://github.com/thr33bricks/LemonMonitor-firmware-community)
