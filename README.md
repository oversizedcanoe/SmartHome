# SmartHome

This repo is going to be similar to my [other smart home repo](https://github.com/oversizedcanoe/party-lights), but in DotNet. I find working with threading and async/await much easier in C# than it is in Python, and C# is my preferred language.

Functionality I would like to have: 
- Turn plugs and lights on and off
- Set color and brightness of lights, if able to (UI should prevent user from setting if the light does not support it)
- Preset modes such as Lavalamp mode (light color gradually shifts) and Rave mode (lights quickly flash random colors)
- Scheduling (i.e. turn this plug on at 10pm every day)
- Wi-fi Monitoring. I would like to somehow use my [python network monitor repo](https://github.com/oversizedcanoe/NetworkMonitor), running on a raspberry pi, and maybe something like MQTT to subscribe to new devices connected?
    - I could even use my [CSV to model repo](https://github.com/oversizedcanoe/CsvToModel) to store device info in CSV (such as a friendly name for a given connected MAC Address, last connected time etc.). A DB is propbably more appropriate, but it would be cool to use my own code.
- Change light color based on a currently playing song, or to the beat of a song

---

### Todo
- Implement "modes" such as Lavalamp mode
- Implement Schedules
- Implement Configurations -- delay times, set aliases?
- Change the TPLink Library to use the nuget package instead of the local repo clone
- Figure out how to remove delay from light transitions
- Bug: Bulb control has a small div to show current bulb color. This works for all colors except "normal" white/yellow ones.
- Bug: When bulbs change to non-normal colors, brightness display shows 255%+ when it should max out at 100%.
