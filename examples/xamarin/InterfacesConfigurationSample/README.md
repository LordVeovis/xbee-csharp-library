Interfaces Configuration Sample Application
===========================================

This application demonstrates the usage of the XBee Library for Xamarin in order
to communicate with ConnectCore devices over Bluetooth Low Energy and configure
device interfaces.

The example scans for Bluetooth devices and allows you to connect to your
ConnectCore device. Once connected, you can read and write the device interfaces
settings.

Demo requirements
-----------------

To run this example you need:

* One ConnectCore device with BLE support. This support can be either native or
  through a BLE XBee3 capable device.
* The ConnectCore device must have the "Configure Interfaces" ConnectCore BLE
  sample application installed.
* An Android or iOS device.

Demo setup
----------

Make sure the hardware is set up correctly:

1. The mobile device is powered on.
2. The ConnectCore device is powered on.
3. Ensure that Bluetooth is enabled in the ConnectCore device or an XBee3 BLE
   module is correctly attached to it.
4. Ensure that the "Configure Interfaces" ConnectCore BLE sample is installed
   and running in the device with the service started.

Demo run
--------

The example is already configured, so all you need to do is to build and launch
the project.

In the first page of the application you have to select your ConnectCore device
from the list of Bluetooth devices. Tap on it and enter the Bluetooth password
you configured when you started the "Configure Interfaces" ConnectCore BLE
sample, by default "1234".

If the connection is successful, the device page appears. It shows the list of
interfaces that can be configured. Tap the interface you want to configure, for
example "Ethernet".

When the "Ethernet" page loads, all the settings are read. Modify one setting,
for example toggle the "Enabled" switch button to "On" and tap "Save" button.

Verify that the "interfaces/Ethernet.txt" demo file of the ConnectCore device has
been updated and the value of the "Disabled" setting has changed to "true".

Compatible with
---------------

* Android devices
* iOS devices

License
-------

Copyright 2022, Digi International Inc.

This Source Code Form is subject to the terms of the Mozilla Public
License, v. 2.0. If a copy of the MPL was not distributed with this
file, you can obtain one at http://mozilla.org/MPL/2.0/.

THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES
WITH REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR
ANY SPECIAL, DIRECT, INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES
WHATSOEVER RESULTING FROM LOSS OF USE, DATA OR PROFITS, WHETHER IN AN
ACTION OF CONTRACT, NEGLIGENCE OR OTHER TORTIOUS ACTION, ARISING OUT OF
OR IN CONNECTION WITH THE USE OR PERFORMANCE OF THIS SOFTWARE.