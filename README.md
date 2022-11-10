# VR Audio Programming Language

This project began with the idea, "what if we could program the entire world through sound?" [VRAPL](https://ccrma.stanford.edu/~lja/vr/VRAPL/) 
is a block-based programming language used in VR to create audio programs that control sound, react to physics events and controller inputs, 
and can even control physics themselves.

Programming blocks generate audio signals, process them and set audio parameters, and output those audio signals via sound, visuals, or physics behavior. 
Then, world objects behave according to the programs, whether by emitting sound, responding to collisions from other objects, or moving.

At a high level, the programming blocks are like an "augmentation" onto the virtual reality. At some point, the user may just want to spend time 
in the virtual world they have created. They can summon a portal that, when looked through or stepped through, shows the world as it would appear,
sound, and behave as if the programs were internal rules. Moving through the portal again returns the user to the programming augmentation,


## Running the Project

A [playground scene](AudioProgrammingLanguage/Assets/_Scenes/Scene1.unity) contains a bongo and a couple "drumsticks". The user can pull up a palette of objects on their controllers and drag new 
blocks from it into the world. They can also summon portals that let them look into or travel between the worlds where their programs are 
visible and where the world objects simply behave according to their programs.

## Scripts

The programming blocks of VRAPL are modular pieces that can be inserted into each other at will to connect one block's output to another's input.
Each block inherits common functionality and implements certain interfaces depending on what kind of inputs it can accept and behavior it can
display. Because each block controls what kinds of inputs it can accept according to these interfaces, it is not possible to build a program that
won't "compile". Programming blocks have Unity functionality for their interactions with each other and the environment as well as underlying
ChucK programs and/or unit generators that are connected or disconnected as the blocks themselves are arranged.

A smattering of types of blocks that show different kinds of behaviors:

- [WorldObject](AudioProgrammingLanguage/Assets/Scripts/WorldObjects/WorldObject.cs), the controller for any virtual object that is not a programming
block. These objects can be picked up, thrown, rescaled, have audio programs embedded in them, have physics controllers embedded in them, and can report 
their size and movement to audio programs.
- [LanguageObject](AudioProgrammingLanguage/Assets/Scripts/Misc%20LanguageObject%20Scripts/LanguageObject.cs), the base class for all programming blocks.
  - [OscController](AudioProgrammingLanguage/Assets/Scripts/LanguageObjectImplementors/OscController.cs), a programming block that outputs a basic oscillator signal
  - [UGenController](AudioProgrammingLanguage/Assets/Scripts/LanguageObjectImplementors/UGenController.cs), a more general block that can create any 
ChucK unit generator provided it knows the class name and acceptable parameters.
  - [ParamController](AudioProgrammingLanguage/Assets/Scripts/LanguageObjectImplementors/ParamController.cs), a programming block that can connect any 
numerical signal to any input parameter in the block it feeds into.
  - [ScalerController](AudioProgrammingLanguage/Assets/Scripts/LanguageObjectImplementors/SimpleScalerController.cs), which can scale a numerical input
such as the position of a [VR controller](AudioProgrammingLanguage/Assets/Scripts/LanguageObjectImplementors/ControllerDataReporter.cs) 
from one range to another range
   - [FunctionController](AudioProgrammingLanguage/Assets/Scripts/LanguageObjectImplementors/FunctionController.cs), a self-contained subroutine inside
a tiny room that the user can teleport and shrink themselves into to edit.
- [EventLanguageObject](AudioProgrammingLanguage/Assets/Scripts/Misc%20LanguageObject%20Scripts/EventLanguageObject.cs), the base class for all event programming blocks.
  - [Clock event block](AudioProgrammingLanguage/Assets/Scripts/EventLanguageObjectImplementors/EventClock.cs), responsible for triggering the rest
of its event chain according to an embedded metronome from ChucK
  - [Collision event block](AudioProgrammingLanguage/Assets/Scripts/EventLanguageObjectImplementors/EventOnCollision.cs), responsible for triggering the
rest of its event chain after a Unity physics collision
- [WireController](AudioProgrammingLanguage/Assets/Scripts/Misc%20LanguageObject%20Scripts/WireController.cs), a flexible tube that can connect two audio
programs' input/output without physically attaching them as one sculpture.
- [Hidden](AudioProgrammingLanguage/Assets/Materials/Hidden.shader) and [AntiHidden](AudioProgrammingLanguage/Assets/Materials/AntiHidden.shader) 
shaders, which use stencils to make portions of objects visible or not visible depending on whether they are viewed through a portal.
- [Serializer](AudioProgrammingLanguage/Assets/Scripts/GlobalUtilty/Serializer.cs), used to store programs on disk and load them in the future.
