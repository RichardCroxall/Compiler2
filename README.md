# Compiler2
Compiler v2 for Home Automation Rules

This compiler reads a file of Rules "Smart.txt" and writes a file of compiled rules "smart.smt".

The compiler is written in C#.
The runtime (elsewhere) reads the compiled rules and interacts with X-10 devices and Philips Hue Lights
to produce a smart house.

When the house program occasionally crashes, my wife resents having to switch lights on by hand after many
years of having lights come on automatically when we enter a room (and switch off automatically after
a few minutes of leaving the room).
