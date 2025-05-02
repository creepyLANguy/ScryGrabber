# ScryGrabber

**Generate detailed card info as JSON from an MTGO decklist.**

---

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Usage](#usage)
- [Contributing](#contributing)
- [License](#license)

---

## Overview

ScryGrabber is a tool designed for Magic: The Gathering Online (MTGO) enthusiasts. This project takes decklists from MTGO and generates detailed JSON output containing card information. Whether you're a developer building applications with MTGO data or a player wanting to analyze your decks programmatically, ScryGrabber simplifies the process.

---

## Features

- Convert MTGO decklists into JSON format.
- Retrieve detailed card information from decklists.

---

## Installation

1. Clone the repository:

   ```bash
   git clone https://github.com/creepyLANguy/ScryGrabber.git
   cd ScryGrabber
   ```

2. Build the project using your preferred C# IDE or the .NET CLI:

   ```bash
   dotnet build
   ```

3. Ensure any dependencies are installed.

---

## Usage

1. Prepare your MTGO decklist file (e.g., `decklist.txt`).
2. Run the application, specifying your decklist file as input:

   ```bash
   dotnet run -- path/to/decklist.txt
   ```

3. The JSON output will be generated in the specified output location.

---

## Contributing

We welcome contributions! To contribute:

1. Fork the repository.
2. Create a new branch for your feature or bug fix.
3. Submit a pull request with a detailed description of your changes.

---

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

---

Let me know if you would like to customize any sections further or add specific details!
