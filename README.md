# SmartHome

This project contains a local clone of [this repo](https://github.com/anthturner/TPLinkSmartDevices), which is used to control the Kasa plugs/lights in my home. The library does not support KL125 lights, which I have, so I modified it to support them. Once this project is somewhat functional, I plan to make a pull request making these changes to the library. After this I will change the reference to a be Nuget Package reference.

This is going to be similar to my [other smart home repo](https://github.com/oversizedcanoe/party-lights), but in DotNet. I find working with threading and async/await much easier in C# than it is in Python, and C# is my preferred language.

Functionality I would like to have: 
- Turn plugs and lights on and off
- Set color and brightness of lights, if able to (UI should prevent user from setting if the light does not support it)
- Preset modes such as Lavalamp mode (light color gradually shifts) and Rave mode (lights quickly flash random colors)

---

### Todo
- Get color conversion properly working for KL125 lights
- Get color setting working from app
- Figure out how to remove delay from light transitions
- Implement better on/off functionality and display
