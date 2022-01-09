<div id="top"></div>

<!-- PROJECT SHIELDS -->
[![Contributors][contributors-shield]][contributors-url]
[![Forks][forks-shield]][forks-url]
[![Stargazers][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]
[![LinkedIn][linkedin-shield]][linkedin-url]

<!-- PROJECT LOGO -->
<br />
<div align="center">
  <a href="https://github.com/ShahriyarB/Icecrown">
    <img src="https://icecrown.ir/icecrown_temp.png" alt="Logo" width="160" height="160">
  </a>

<h3 align="center">Hostbot Project</h3>

  <p align="center">
    Warcraft III Lan Only Game Server Written in C# (.NET 6)
    <br />
    <a href="https://github.com/ShahriyarB/Icecrown/wiki"><strong>Explore the docs »</strong></a>
    <br />
    <br />
    <a href="https://github.com/ShahriyarB/Icecrown">Releases</a>
    ·
    <a href="https://github.com/ShahriyarB/Icecrown/issues">Report Bug</a>
    ·
    <a href="https://github.com/ShahriyarB/Icecrown/issues">Request Feature</a>
  </p>
</div>



<!-- TABLE OF CONTENTS -->
<details>
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgments">Acknowledgments</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

This project started first as a port of [ghostpp](https://github.com/uakfdotb/ghostpp) to C# and then went under some heavy modifications to become a clean, organized and well documented project.
It is written with the extensibility in mind, so a plugin system can be implemented later with the minimal effort. There is not any Battle.net connection code in this project, aim of this project is currently LAN only  for use in custom-made platforms like ICCup, RGC and others.

Please note that this project is not production ready in its current state, it lacks some minor features which you can find below on this page and not yet tested in a big scale.

<p align="right">(<a href="#top">back to top</a>)</p>

<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

* [.NET 6](https://dotnet.microsoft.com/en-us/download/dotnet/6.0)
* Visual Studio 2019+ (Optional)

### Building

1. Clone the repo
   ```sh
   git clone https://github.com/ShahriyarB/Icecrown.git
   ```
2. Go to the hostbot directory
   ```sh
   cd Icecrown/Hostbot
   ```
3. Build the hostbot
   ```js
   dotnet build // or "dotnet run" to run the project
   ```

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- USAGE EXAMPLES -->
## Usage

If you're using any game version other than 1.30.1 you need to manually extract ```common.j``` and ```blizzard.j``` file from game files and put it in hostbot's data directory.

Maps should be copied to maps folder.

Some in game commands requires Moderator/Administrator role and can not be used by everyone, to make yourself admin you need to set your in game name to ```admin```.
This will definitely change in the feature when we implement the user roles and database.

To start the game you can use the ```start``` command. To see the available commands please refer to ```GameProtocol.cs``` file, all commands are registered there.

To modify the hostbot settings refer to ```settings.json``` file, this file should be near the binaries.
If this file does not exist, a default ```settings.json``` file will be created at first launch.

This is an example of how ```settings.json``` can be:
   ```json
   {
     "DataPath": ".\\data",
     "CommandDelimiter": "/",
     "WarcraftVersion": 30,
     "DownloadLimit": 800,
     "Latency": 80,
     "BroadcastLan": true,
     "Hostbots": [
       {
          "GameName": "|cFF6495EDDotA All Pick",
          "Map": "Maps\\Download\\DotA v6.83d.w3x",
          "HCL": "ap",
          "MaxLobbies": 3
       },
       {
          "GameName": "|cFF6495EDMelee",
          "Map": "Maps\\(12)IceCrown.w3m",
          "HCL": "",
          "MaxLobbies": 1
       }
     ]
   }
   ```
   
   This ```settings.json``` results the following lobbies:
   <br />
   <div align="center">
   <img src="https://icecrown.ir/icecrown_lobbies.jpg" alt="Lobbies" width="700" height="500">
   </div>
   <br />

_For more information about ```settings.json``` file, please refer to the [Documentation](https://github.com/ShahriyarB/Icecrown)_

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- ROADMAP -->
## Roadmap

- Add lag screen support
- Implement database support
- Implement user roles
- Implement plugin system


See the [open issues](https://github.com/ShahriyarB/Icecrown/issues) for a full list of proposed features (and known issues).

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTRIBUTING -->
## Contributing

Contributions are what make the open source community such an amazing place to learn, inspire, and create. Any contributions you make are **greatly appreciated**.

If you have a suggestion that would make this better, please fork the repo and create a pull request. You can also simply open an issue with the tag "enhancement".
Don't forget to give the project a star! Thanks again!

1. Fork the Project
2. Create your Feature Branch (`git checkout -b feature/AmazingFeature`)
3. Commit your Changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the Branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- LICENSE -->
## License

Distributed under the Apache-2.0 License. See `LICENSE` for more information.

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- CONTACT -->
## Contact

Shahriyar Bazaei - [ShahriyarBa](https://linkedin.com/in/shahriyarba) - shahriyar.ba@icloud.com

Project Link: [https://github.com/ShahriyarB/Icecrown](https://github.com/ShahriyarB/Icecrown)

<p align="right">(<a href="#top">back to top</a>)</p>



<!-- ACKNOWLEDGMENTS -->
## Acknowledgments

* Huge thanks to [ghostpp](https://github.com/uakfdotb/ghostpp) without it this project would take much more time to make

<p align="right">(<a href="#top">back to top</a>)</p>


<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/ShahriyarB/Icecrown.svg?style=for-the-badge
[contributors-url]: https://github.com/ShahriyarB/Icecrown/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/ShahriyarB/Icecrown.svg?style=for-the-badge
[forks-url]: https://github.com/ShahriyarB/Icecrown/network/members
[stars-shield]: https://img.shields.io/github/stars/ShahriyarB/Icecrown.svg?style=for-the-badge
[stars-url]: https://github.com/ShahriyarB/Icecrown/stargazers
[issues-shield]: https://img.shields.io/github/issues/ShahriyarB/Icecrown.svg?style=for-the-badge
[issues-url]: https://github.com/ShahriyarB/Icecrown/issues
[license-shield]: https://img.shields.io/github/license/ShahriyarB/Icecrown.svg?style=for-the-badge
[license-url]: https://github.com/ShahriyarB/Icecrown/blob/master/LICENSE
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://linkedin.com/in/shahriyarba
