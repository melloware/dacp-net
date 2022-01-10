[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

# DACP.NET
DACP.NET is an open source library for handling the complexities of Apple's DACP Protocol

DACP.NET is an open source library for handling the server-side complexities of Apple's DACP Protocol. DACP helps you remote control your desktop or laptop media player from any DACP Client:

Apple Remote for iPhone/iPod/iPad
TunesRemote+ for Android
'''Digital Audio Control Protocol''' (DACP) is a protocol used by the Apple Inc.'s Remote application (app) on the iPhone to control iTunes running on a remote computer. This way mobile devices such as the iPhone/iPod Touch/iPad/Android can be used as a two way WLAN music remote control.

The DACP protocol are 100% RESTful HTTP calls that have to be reverse engineered by "sniffing" the communication between Itunes and the Apple Remote. It is a tedious task every time Apple changes something even in the slightest manner there is a lot of work to be done by the individual developers.

A few server side DACP implementations already exist such as:

Apple iTunes
MonkeyTunes for MediaMonkey
AlbumPlayer
MusicBee
The GOAL of this project is to implement a .NET Library that does all of the heavy lifting of the DACP Protocol. This way developers who want to build media server application that emulate Itunes server can do so and reap the benefit of the current DACP Clients that are already out there.
