# INFO5380-HW2
INFO5380-HW2


## Cloning the repository
```
git clone git@github.com:bisson2000/INFO5380-HW2.git
```


## How to get started

Requirements:
- Unity 2022.3.7f1

Steps:
1. Clone the project
2. Open the project in Unity 2022.3.7f1
3. In Assets/Scenes, open MainMenu.unity
4. Click Play

## Hotkeys

- Adding line: L (lowercase)
    - If you have a line segment selected and you want to increase the line shift + L 
    - If you have a line segment selected and you want to retract the line del + l 
- Adding curve: C (lowercase)
    - If you have a line segment selected and you want to increase the line shift + L
    - If you have a line segment selected and you want to retract the line del + c
- Rotate clockwise: R
- Counterclockwise: shift + R
- Tighten the curvature (increase or decrease the radius of rotation): k
- Loosen: J
- Toggle length, bend, angle: i
- Erase: backspace 
- Next segment selection: ↑ (Arrow up)
- Previous segment: ↓ (Arrow down)
- Show collisions: M
- Save: Shift + P  to save coordinates to CSV


## How to start developing

First, clone the repository. Follow the following guides.

### Installing Unity Hub and Unity

1. Install Unity Hub [https://unity.com/download#how-get-started](https://unity.com/download#how-get-started)
    - Note: that Unity Hub allows for managing unity versions
2. Install Unity 2022.3.7f1 [unityhub://2022.3.7f1/b16b3b16c7a0](unityhub://2022.3.7f1/b16b3b16c7a0)
    - Note: If the direct link does not work, you can go to [https://unity.com/releases/editor/whats-new/2022.3.7](https://unity.com/releases/editor/whats-new/2022.3.7) and select "Install this version with Unity Hub"
3. In the installation menu, make sure the following is selected:
    1. Android build support
    2. iOS build support
    3. Mac build support (Mono)
    4. Documentation
4. Proceed to install unity

Note: You may need to restart your computer.

### Code editors recommendations

- Recommendation 1: JetBrains Rider (https://www.jetbrains.com/rider/)[https://www.jetbrains.com/rider/]
    - Note that an educational license is available
- Recommendation 2: Visual Studio Code
- Recommendation 3: Visual Studio Community

### Opening the project

1. In Unity Hub, select "Add" to add the project.
2. Select the "WireBender3D" folder in the repository, and click open.
3. Make sure the project editor version is 2022.3.7f1 in Unity Hub
4. Click on the "WireBender3D" project in Unity Hub to open it.

![](./ReadmeImages/AddProject.png)
![](./ReadmeImages/OpenProject.png)

### Setting your code editor in the project

1. Go to Edit > Preferences (macOS: Unity > Settings)
2. Go to External tools
3. Set your external script editor to whichever one you prefer
4. Make sure the 4 first checkboxes are checked

![](./ReadmeImages/ExternalScriptEditor.png)

### Opening the code from Unity

1. Go to Assets > Open C# Project

![](./ReadmeImages/OpenCSharpProject.png)

