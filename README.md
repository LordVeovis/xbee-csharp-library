XBee C# Library
=================

This project contains the source code of the XBee C# Library, an easy-to-use 
API developed in C# that allows you to interact with Digi International's
[XBee](http://www.digi.com/xbee/) radio frequency (RF) modules.

This source has been converted from the Java version provided by [Digi International](http://www.digi.com).

The main features of the library include:

* Support for ZigBee, 802.15.4, DigiMesh and Point-to-Multipoint XBee devices.
* Support for API and API escaped operating modes.
* Management of local (attached to the PC) and remote XBee device objects.
* Discovery of remote XBee devices associated with the same network as the 
local device.
* Configuration of local and remote XBee devices:
  * Configure common parameters with specific setters and getters.
  * Configure any other parameter with generic methods.
  * Execute AT commands.
  * Apply configuration changes.
  * Write configuration changes.
  * Reset the device.
* Transmission of data to all the XBee devices on the network or to a specific 
device.
* Reception of data from remote XBee devices:
  * Data polling.
  * Data reception callback.
* Reception of network status changes related to the local XBee device.
* IO lines management:
  * Configure IO lines.
  * Set IO line value.
  * Read IO line value.
  * Receive IO data samples from any remote XBee device on the network.
* Support for explicit frames and application layer fields (Source endpoint, 
Destination endpoint, Profile ID, and Cluster ID).

License
-------
This software is open-source software. Copyright Sébastien RAULT, 2014-2015. 

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this file,
you can obtain one at http://mozilla.org/MPL/2.0/.
