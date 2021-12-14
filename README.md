<h1 align="center">Overseer Inspector</h1>
<br>A collection of automatic attributes to serve user the purpose of dynamically, comfortable experience with Unity Inspector and increase workflow speed. Not only that, Overseer Inspector also has the ability to extend custom attribute as you need.</br>
<br>More attributes and features will be developed.</br>

<h1 align="center">Requirements</h1>
Overseer Inspector are developed and run in Unity 2020.3. So only version above 2020.3 can be used for Overseer Inspector.</br>
.NET 4.x might be required in the future</br>

<h1 align="center">Installation</h1>
<br>1. Grab the git URL of the repo:</br>
<p align="center">
  <img src="https://i.imgur.com/qxmiqM1.png" alt="Grab Installation URL">
</p>
2. Go to Unity's Package Manager, and find "Add Package from git URL..." option and paste the URL in</br>
3. Press add, sit back and enjoy hell forming</br>

<h1 align="center">How to use this thing?</h1>
<br>It's as simple as marking the Behaviour class with a special attribute in the "RealityProgrammer.OverseerInspector.Runtime" namespace (yes my ego is huge):</br>

<p align="center">
  <img src="https://i.imgur.com/2eCmE1k.png" alt="Begin Hell">
</p>
And there you go, you can bind Attributes for your fields/class members now. And they will be display correspond to what you assigned inside the Inspector. Amazing, isn't it?</br>

# Attributes</h1>

<p align="center"> 
  <a href="#conditional">Conditionals</a> - 
  <a href="#addition">Additions</a> - 
  <a href="#primary">Primary</a> - 
  <a href="#group">Grouping</a> -
  <a href="#miscs">Miscs</a>
</p>

<a id="conditional">
  <h2>Conditionals</h2>
</a>
  Conditionals is a group of attributes that determine whether to show the field, or hide it from displaying. Belong to

```cs
RealityProgrammer.OverseerInspector.Runtime.Validation
```
namespace.

```cs
public Transform nullableField;
[field: SerializeField] public Transform NullableProperty { get; set; }
  
[ShowIfNull("nullableField")] public float dummy1;
[HideIfNull("nullableField")] public float dummy2;
  
// Backing fields also works
[ShowIfNull("NullableProperty")] public float dummy3;
[HideIfNull("NullableProperty")] public float dummy4;
```
![Validator 1](https://i.imgur.com/mZANVzu.png)</br>
Or if you want to have dynamically condition
```cs
public float floatValue;
bool ConditionMethod() {
    return floatValue > 10;
}

[ShowIf("ConditionMethod()")]
public float display;
  
public bool boolValue;

[ShowIf("boolValue")]     // Also works with Property
public float display2;
  
[ShowIf("!boolValue")]    // Also works with Property
public float display3;
```
![Validator 2](https://i.imgur.com/OkuMfQI.png)

~~(Stop complaining, I'm too lazy to do string expression)~~
Good (and bad at the same time) new, Aurora Interpreter are added to the package, and they can do simple expression which no one probably need.
Tired of creating methods for just a simple check? Now you can do dynamic
```cs
[Range(0, 1)] public float value1;
[Range(0, 1)] public float value2;

[ShowIf("value1 >= 0.5 && value2 >= 0.5")]
public float target;
```

and it will update correctly. (I'm too lazy to upload images)

<a id="addition">
  <h2>Addition</h2>
  Additions is a collection of attributes that provide extra displayable element to primary drawer. One field can have multiple additions. Belong to
  
```cs
RealityProgrammer.OverseerInspector.Runtime.Drawers
```
namespace.
  
  <h3>MessageBox</h3>
  Show a helpbox above the current field, with custom icon support
  
```cs
[MessageBox("Message Box Info", IconArgument = "Info")]
public Transform info;

[MessageBox("Message Box Warning", IconArgument = "Warning")]
public Transform warning;

[MessageBox("Message Box Error", IconArgument = "Error")]
public Transform error;
```
![Message Box](https://i.imgur.com/7YL9JNz.png)
  <h3>Seperator</h3>
  Seperate between fields with colored lines
  
```cs
public float f1;

[Seperator(ColorParameter = "#FF0000")]
public float f2;
  
[Seperator(ColorParameter = "rgb(0, 255, 0)")]
public float f3;
  
[Seperator(ColorParameter = "rgba(255, 0.5, 0, 1.)")]
public float f4;
```
![Seperator](https://i.imgur.com/MWn0Y66.png)
  <h3>Miscs</h3>
  OverseerSpace: Works like Unity's default Space, but compatible with Overseer Inspector
</a>

<a id="primary">
  <h2>Primary Drawers</h2>
  Primary is a master drawer for field that handle everything, from determine layout progress, to handling the Additions. Same namespace to the Addtions.
</a>

<a id="group">
  <h2>Grouping</h2>
  Group is a container that contains all the fields inside it in a form of BeginGroup/EndGroup. Belong to
  
```cs
RealityProgrammer.OverseerInspector.Runtime.Drawers.Group
```
  namespace
  
  <h3>Foldout Group</h3>
  Surround fields inside a foldout group with nice transition

```cs
[BeginFoldoutGroup("Survivability")]
public float health;
public float defense;
public float damageReduction;
[EndGroup]
public float regeneration;
```
![Foldout Group](https://i.imgur.com/qyO5wVH.png)
  
  <h2>Tab Group</h2>
  WIP...
</a>

<a id="miscs">
  <h2>Miscellaneous</h2>
</a>
  Uncategorized attributes of Overseer to serve as a collection for function and stuff like that. Belong to
  
```cs
RealityProgrammer.OverseerInspector.Runtime.Miscs;
```
namespace.

# Some rule need to be remembered
## Coloring parameter
Coloring will always be in either #RRGGBB, #RRGGBBAA, rgb(red, green, blue) or rgba(red, green, blue, alpha). In rgb or rgba representation, numerical values are treated as integer by default (0-255), and treated as floating point number if there is a decimal separator "." (0-1).

## Member include rule
By default, field will be cached as much as possible based on Attribute, Serialization rule. But method and property might work different, they need to contains at least 1 Overseer Attribute to be cached. Therefore, it might broke BeginGroup. To fix this, either use any of Overseer attribute, or use \[OverseerInclude\] attribute.

## Inheritance
Inheritance might not work very well for now (especially parent's methods, properties). But it's currently work in progress.

## Aurora Interpreter
Made in... 2-3 days (with the code got from RPUnity Toolkit, check that out too), it might not be very performance or works correctly. Exception (and crash) might happen at certain degree, and initial overhead is drastically increased (probably because of the use of dynamic object). Report any error if something went wrong.  

Also, complicated member access like namespace, classes are not added yet and probably never will. Enum, however, are comparable with integer numeric type (Long)

### Pro
1. It's a small interpreter, so it's pretty fast
2. Can do most of C# syntax

### Con
1. No constructor, only primitive (int, long, unsigned, string, boolean, null)
2. Initial overhead

# Acknowledgement
This is one person project, everything might not be performance, nor be bug free, but suggestion and bug report are highly welcomed.
TODO table:
|Context|Status|Notes|
|:--:|:-----:|:---:|
|Nested group|✔️||
|Method context attributes|✔️||
|Property context attributes|-|Supported, but serve no functionality yet|
|Property set call|✔️|\[FieldAssignCallbackAttribute\]|
|Light mode support|❌|haha no|
|Code emit support for Aurora|-|Maybe|
|Regret everything|✔️||
