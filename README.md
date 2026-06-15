# CubeBallShooter

A third-person obstacle-course shooter built in Unity 6. Navigate an industrial
arena using vaulting, climbing, and jumping mechanics to find and eliminate 5
hidden targets before time runs out.

![Gameplay Screenshot]
![Gameplay 1](Screenshots/Screenshot%202026-06-15%20220841.png)

![Gameplay 2](Screenshots/Screenshot%202026-06-15%20220900.png)

![Gameplay 3](Screenshots/Screenshot%202026-06-15%20220930.png)

![Gameplay 4](Screenshots/Screenshot%202026-06-15%20221002.png)

![Gameplay 5](Screenshots/Screenshot%202026-06-15%20221140.png)

![Gameplay 6](Screenshots/Screenshot%202026-06-15%20221252.png)

## 🎮 Play It
- [Download Build](#) (Windows .exe via Releases)
- [Watch Gameplay](#) (YouTube link)

## ✨ Features
- Custom finite state machine driving all player behavior (Movement, Jumping,
  Climbing, Vaulting)
- Third-person Cinemachine camera with camera-relative 8-directional movement
- Full Mixamo animation pipeline with 1D Blend Tree locomotion and
  state-driven transitions
- Dynamic climb animation system — a single clip plays forward, pauses, or
  reverses based on player input via an animator speed multiplier
- Context-aware shooting — restricted during traversal states (vault/climb/
  normal jump), allowed during movement and running jumps
- Aim mode with smooth FOV zoom and movement speed adjustment
- Win/lose game loop with target counter, timer, and UI feedback

## 🛠️ Tech Stack
- Unity 6.3 LTS
- C#
- Cinemachine
- Unity New Input System
- Mixamo (animation source)

## 🏗️ Architecture
The player controller follows a Single Responsibility split:

| Script | Responsibility |
|---|---|
| `PlayerController` | Owns state machine, reads input, coordinates other systems |
| `PlayerMover` | Movement math, gravity, coyote time |
| `PlayerVault` | Vault detection and execution |
| `PlayerClimb` | Climb detection, wall-jump, dynamic climb speed |
| `PlayerAnimator` | Drives all Animator parameters from game state |
| `BallShooter` | Shooting, aim mode, FOV control |

## 🧩 Technical Highlights

**Animation-driven state sync**
The animator reads `PlayerState` every frame and converts it into Animator
parameters (`Speed`, `IsRunning`, `IsClimbing`, etc.), keeping all visual
feedback in sync with gameplay logic without animation code living inside
gameplay scripts.

**Single-clip reversible climb animation**
Instead of importing separate climb-up/climb-down/idle clips, one looping
climb animation is driven by a `ClimbSpeed` float (1 = forward, 0 = pause,
-1 = reverse) using the Animator's Speed Multiplier — reducing asset count
and keeping the animation perfectly responsive to input.

**CharacterController.velocity caveat**
Combined all per-frame movement into a single `CharacterController.Move()`
call, since `.velocity` only reflects the most recent `Move()` call — calling
it twice per frame (once for horizontal, once for gravity) silently zeroed
out the value the animator depended on.

## 🐛 Challenges Faced
- **Execution order bug**: `IsRunning` was being reset to false by the
  locomotion update before the jump-trigger logic could read it, causing
  RunningJump to never play. Fixed by caching the value one frame earlier
  (`_wasRunning`) before state-blocking logic could overwrite it.
- **Floating point thresholds**: Speed values asymptotically approached but
  never reached `1.0`, so Blend Tree thresholds needed to be set below 1.0
  (e.g. 0.9) to account for floating-point precision.
- **Missing EventSystem**: UI buttons silently stopped responding after a
  scene edit — traced to an accidentally deleted EventSystem GameObject,
  which Unity UI requires for any click interaction.

## 📚 Lessons Learned
- Debugging animation systems is most effective by isolating which "layer"
  a bug lives in: script value → execution timing → Animator transition logic.
- `[SerializeField]` defaults changed in code don't apply if the Inspector
  already has a serialized value — always verify the Inspector after a code
  default change.
- Designing systems with clear single responsibilities (one script = one job)
  made debugging dramatically faster than a single monolithic controller.

## 📂 Project Structure
