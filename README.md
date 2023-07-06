# OCB Stop Fuel Waste Mod - 7 Days to Die (A21) Addon

A small harmony mod to stop fuel waste in forges, campfires and
chemical stations. Will stop burning fuel when production or melting
is finished. You can still use the station to only produce heat if
you enable the station with nothing to melt or produce in it.

This mod is not EAC (Easy Anti-Cheat) compatible, so turn it off!

[![GitHub CI Compile Status][3]][2]

## Download and Install

Simply [download here from GitHub][1] and put into your A21 Mods folder:

- https://github.com/OCB7D2D/OcbStopFuelWaste/archive/master.zip (master branch)

## Changelog

### Version 1.0.1

- Update ModInfo.xml to A21 version

### Version 1.0.0

- Update compatibility for 7D2D A21.0(b313)

### Version 0.2.2

- Add a few more null pointer checks
- Automated deployment and release packaging

### Version 0.2.1

- Remove debug statements (stops spamming the log)

### Version 0.2.0

- Fix issue when workstation tiles are unloaded/loaded  
  Should now correctly preserve fuel for big time delta steps

### Version 0.1.1

- Fix issue with stations not having any fuel

### Version 0.1.0

- Initial version

## Compatibility

Developed initially for version A20(b218), updated through A21.0(b324).

[1]: https://github.com/OCB7D2D/OcbStopFuelWaste/releases
[2]: https://github.com/OCB7D2D/OcbStopFuelWaste/actions/workflows/ci.yml
[3]: https://github.com/OCB7D2D/OcbStopFuelWaste/actions/workflows/ci.yml/badge.svg
