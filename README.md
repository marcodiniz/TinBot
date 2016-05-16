(this file and everything else is under  construction)

TinBot
==============
https://www.youtube.com/watch?v=lZ7Tqtb3c_U

##What is TinBot
TinBot (Team Integration Robot) is a robot made to help software development teams on they daile acvities. This is what TinBot can do:
- Speak and react to voice commands
- Tell jokes
- Monitor and loudly speak the team's Indexes
- Watch for checkins and builds from TFS
- Give messages for the entire team sent from Slack
- Turn 360º on it's own axis
- Point to team member who made a Checkin, Commited Bad code, broke a build or commited some code with failing tests
- Give software development tips
- Of course! Shoot laser
- much more!

##Motivation
The TinBot was made as a personal project for studing Arduino, the Firmata Protocol and the Universal Windows Apps. 

##Under the hood
- Some servos (for moving arms, head, body, etc)
- A huge supercapcitor (so the Lion battery can handle all servos at once)
- Leds, resistors, switches (the basic stuff)
- Lumia 520 with Windows 10 (for running a uwp app wich is the TinBot's brain)
- HC-06 bluetooth module (connecting the Arduino with the Lumia Phone)
- [Firmata](https://github.com/firmata/arduino)(the protocol used for remote controle the Arduino)
- [Windows Remote Arduino](https://github.com/ms-iot/remote-wiring)(the Microsoft implementation of Firmata for UWPs)
