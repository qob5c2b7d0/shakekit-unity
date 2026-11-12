# ShakeKit

Camera shake is usually the first piece of feedback you add and the first one
that starts fighting the player. ShakeKit keeps it authored: every shake is a
`ShakeProfile` asset with a duration, amplitude and an `AnimationCurve` falloff,
so recoil settles the way you drew it instead of the way a random number
generator decides. Layers stack additively and are normalized, blasts can opt
into `Replace`, and the base pose is restored exactly when the stack drains.

Small on purpose: four runtime scripts, no allocations per frame, no coupling to
a specific camera rig. Works on any local transform.

![Shake preview placeholder](docs/shake-preview.gif)

## Install

Via Unity Package Manager, `Add package from git URL`:

```
https://github.com/qob5c2b7d0/shakekit-unity.git
```

Or copy `Runtime/` and `package.json` into `Assets/ShakeKit/`. Tested on Unity
2022.3 LTS; the runtime code has no external dependencies.

## Usage

1. Create profiles via `Assets > Create > ShakeKit > Shake Profile`.
2. Add `CameraShaker` to the camera (or any transform you want to displace) and
   drag the camera into `Target`.
3. Play a profile from gameplay code:

```csharp
public ShakeProfile hitShake;

private void OnPlayerHit()
{
    shaker.Play(hitShake, scale: 0.7f);
}
```

Authoring notes:

- The falloff curve is the envelope. Keep it at 1 for a hold, dip below 1 to
  settle, and go negative for a short overshoot.
- `positionAmplitude` and `rotationAmplitude` are separate, and Z displacement
  is scaled to 35% so shake reads as lateral without pushing through walls.
- `Additive` layers sum and divide by `sqrt(layerCount)`. `Replace` layers clear
  the stack when played and mute everything under them.

## Demo controls

`ShakeTrigger` (optional, on the same object as `CameraShaker`) fires a random
profile from the array with **Space** and clears the stack with **C**. The demo
uses the legacy Input Manager; swap it for the Input System if your project has
legacy input disabled.

## Build

1. Open the project in Unity 2022.3 LTS or newer.
2. Add a scene with a `CameraShaker`, assign a target and two or three profiles.
3. `File > Build Settings`, pick a platform, press `Build And Run`.

## Layout

```
Runtime/
  CameraShaker.cs   layer stack, per-frame application and base-pose restore
  ShakeMath.cs      Perlin sampling, layer combine, ShakeOffset value type
  ShakeProfile.cs   authored duration/amplitude/curve asset
  ShakeTrigger.cs   demo Space/C input
  ShakeKit.asmdef   isolated assembly
```

`docs/shake-preview.gif` is the preview placeholder; drop a capture in and the
README picks it up.

## License

MIT, 2026 Marta Nowak.
