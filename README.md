# radar-radius
Automatically exported from code.google.com/p/radar-radius

This is a small RADIUS server implementation written in VB.NET. It is almost RFC-compliant (RFC2865) and includes some vendor-specific attribute helpers pertaining to Cisco IOS-based devices.

## History
This RADIUS server implementation came into being back in early 2008 after an unsuccessful search for a simple and inexpensive, yet flexible RADIUS server (or .NET RADIUS library, for that matter) with VoIP call accounting and authorization handling for Cisco IOS routers in mind.

The version available at https://github.com/oskarb/radar-radius is a forked version from 2016, with some minor updates since upstream is no longer actively maintaining the project.

## Structure
At large, the project includes several modules:

* core RADIUS server (nick-named RADAR) (Currently, the code only for this module is posted here.) -- a .NET library implementing RADIUS.
* Call Logic module (CalLogic) -- processes RADIUS accounting requests to compose call records saved in a database. The module also processes authorization requests using a number of custom authorization schemes (used originally to communicate with custom Cisco TCL IVR scripts running against VoIP calls on Cisco IOS routers).
* Bill Logic module (BilLogic) -- processes call records applying call rates, etc.
* WebUI module -- Web interface allowing for the configuration of the CalLogic and BilLogic modules as well as presenting call reports.

## Dependencies
The code targets version 4.5 of the Microsoft .NET Framework and loads in Visual Studio 2015.

## Contact
Please direct questions and comments to Nikolay Semov.
