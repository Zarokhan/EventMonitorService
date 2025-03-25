# Event Monitor Service
A lightweight Windows service tool for monitoring Windows Event Viewer and sending alerts.

## Features
* Monitor Windows Event Viewer for critical application errors.
* Send email notifications when an application error occurs.
* Customizable email template with event details.

## Configuration
The service stores its configuration in a config.json file, located in the following directory:

`%AppData%\EventMonitorService`

Note you have to start applicaiton once to generate config.json file

### Available Settings
* Event Filters – Define which events to monitor.
* SMTP Settings – Configure email notifications.

### Severity Levels
Severity levels in the configuration file:
* 1 – Critical
* 2 – Error
* 3 – Warning
* 4 – Information
* 5 – Verbose

### TODOS
* Alerts to Microsoft Teams