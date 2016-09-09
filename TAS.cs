using System;
using SmartInput;
using System.Threading;
using UnityEngine;
using System.Collections.Generic;
namespace OriTAS {
	[Flags]
	public enum TASState {
		None = 0,
		Enable = 1,
		Record = 2,
		Reload = 4,
		FrameStep = 8,
		ChangeSpeed = 16,
		OpenDebug = 32,
		Rerecord = 64
	}
	public class TAS {
		private static TASState tasStateNext, tasState;
		private static string filePath = "Ori.tas";
		private static TASPlayer player = new TASPlayer(filePath);
		public static float deltaTime = 0.016666667f, timeScale = 1f;
		public static int frameRate = 0;
		private static GUIStyle style;
		private static char currentKeyPress;
		private static string currentInputLine, nextInputLine, extraInfo, savedExtraInfo;

		static TAS() {
			DebugMenuB.MakeDebugMenuExist();
		}
		public static bool UpdateTAS() {
			UpdateText();
			HandleFrameRates();
			CheckControls();
			FrameStepping();

			if (HasFlag(tasState, TASState.Enable)) {
				if (HasFlag(tasState, TASState.Record) || HasFlag(tasState, TASState.Rerecord)) {
					player.RecordPlayer();
				} else {
					player.PlaybackPlayer();

					if (!player.CanPlayback) {
						DisableRun();
					}
					if (!GameController.Instance.IsLoadingGame && !InstantLoadScenesController.Instance.IsLoading && player.Break < 0) {
						tasState |= TASState.FrameStep;
						player.Break = 0;
					}
					return true;
				}
			}
			return false;
		}
		private static void HandleFrameRates() {
			if (HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.FrameStep) && !HasFlag(tasState, TASState.Record)) {
				float rsX = XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX);

				if (player.FastForward) {
					SetFrameRate(180);
				} else if (rsX <= -1.2) {
					SetFrameRate(1);
				} else if (rsX <= -1.1) {
					SetFrameRate(2);
				} else if (rsX <= -1.0) {
					SetFrameRate(3);
				} else if (rsX <= -0.9) {
					SetFrameRate(4);
				} else if (rsX <= -0.8) {
					SetFrameRate(6);
				} else if (rsX <= -0.7) {
					SetFrameRate(12);
				} else if (rsX <= -0.6) {
					SetFrameRate(16);
				} else if (rsX <= -0.5) {
					SetFrameRate(20);
				} else if (rsX <= -0.4) {
					SetFrameRate(28);
				} else if (rsX <= -0.3) {
					SetFrameRate(36);
				} else if (rsX <= -0.2) {
					SetFrameRate(44);
				} else if (rsX <= 0.2) {
					SetFrameRate();
				} else if (rsX <= 0.3) {
					SetFrameRate(75);
				} else if (rsX <= 0.4) {
					SetFrameRate(90);
				} else if (rsX <= 0.5) {
					SetFrameRate(105);
				} else if (rsX <= 0.6) {
					SetFrameRate(120);
				} else if (rsX <= 0.7) {
					SetFrameRate(135);
				} else if (rsX <= 0.8) {
					SetFrameRate(150);
				} else if (rsX <= 0.9) {
					SetFrameRate(165);
				} else {
					SetFrameRate(180);
				}
			} else {
				SetFrameRate();
			}
		}
		private static void ClearKeyPress(bool ignoreCheck = false) {
			if (ignoreCheck || (HasFlag(tasState, TASState.Enable))) {
				currentKeyPress = '\0';
			}
		}
		private static void SetFrameRate(int newFrameRate = 60) {
			if (frameRate == newFrameRate) { return; }

			frameRate = newFrameRate;
			timeScale = (float)newFrameRate / 60f;
			Time.timeScale = timeScale;
			Time.captureFramerate = newFrameRate;
			Application.targetFrameRate = newFrameRate;
			Time.fixedDeltaTime = 1f / 60f;
			Time.maximumDeltaTime = Time.fixedDeltaTime;
			QualitySettings.vSyncCount = 0;
		}
		private static void FrameStepping() {
			char kp = currentKeyPress;
			float rsX = XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX);
			bool rhtTrg = XboxControllerInput.GetButton(XboxControllerInput.Button.RightTrigger);
			bool dpU = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) > 0.1f || kp == '[';
			bool dpD = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) < -0.1f || kp == ']';

			if (HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && (HasFlag(tasState, TASState.FrameStep) || dpU && !rhtTrg)) {
				bool ap = dpU;
				while (HasFlag(tasState, TASState.Enable)) {
					kp = currentKeyPress;
					rsX = XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX);
					rhtTrg = XboxControllerInput.GetButton(XboxControllerInput.Button.RightTrigger);
					dpU = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) > 0.1f || kp == '[';
					dpD = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) < -0.1f || kp == ']';

					CheckControls();
					if (!ap && ((dpU && !rhtTrg))) {
						tasState |= TASState.FrameStep;
						break;
					} else if (dpD && !rhtTrg) {
						tasState &= ~TASState.FrameStep;
						break;
					} else if (rsX >= 0.2) {
						tasState |= TASState.FrameStep;
						int sleepTime = 0;
						if (rsX <= 0.3) {
							sleepTime = 200;
						} else if (rsX <= 0.4) {
							sleepTime = 100;
						} else if (rsX <= 0.5) {
							sleepTime = 80;
						} else if (rsX <= 0.6) {
							sleepTime = 64;
						} else if (rsX <= 0.7) {
							sleepTime = 48;
						} else if (rsX <= 0.8) {
							sleepTime = 32;
						} else if (rsX <= 0.9) {
							sleepTime = 16;
						}
						Thread.Sleep(sleepTime);
						break;
					} else if (kp == ':' || savedExtraInfo != null) {
						tasState |= TASState.FrameStep;

						ClearKeyPress();

						if (savedExtraInfo == null) {
							savedExtraInfo = extraInfo;
						} else {
							if (extraInfo == savedExtraInfo) {
								break;
							}
							savedExtraInfo = null;
						}
					}
					ap = dpU;
					ClearKeyPress();
					Thread.Sleep(1);
				}
				ReloadRun();
			}
			ClearKeyPress();
		}
		private static void DisableRun() {
			tasState &= ~TASState.Enable;
			tasState &= ~TASState.FrameStep;
			tasState &= ~TASState.Record;
			tasState &= ~TASState.Rerecord;
		}
		private static void CheckControls() {
			char kp = currentKeyPress;
			float rsX = XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX);
			bool rhtTrg = XboxControllerInput.GetButton(XboxControllerInput.Button.RightTrigger);
			bool lftTrg = XboxControllerInput.GetButton(XboxControllerInput.Button.LeftTrigger);
			bool lftStick = XboxControllerInput.GetButton(XboxControllerInput.Button.LeftStick);
			bool dpU = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) > 0.1f;
			bool dpD = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) < -0.1f;
			bool kbPlay = MoonInput.GetKey(KeyCode.LeftBracket);
			bool kbRec = MoonInput.GetKey(KeyCode.Backspace) && (MoonInput.GetKey(KeyCode.LeftShift) || MoonInput.GetKey(KeyCode.RightShift));
			bool kbStop = MoonInput.GetKey(KeyCode.Backslash) || kp == '\\';
			bool kbDebug = MoonInput.GetKey(KeyCode.F8);
			bool kbReload = MoonInput.GetKey(KeyCode.Quote) || kp == '\'';
			bool kbRerec = MoonInput.GetKey(KeyCode.Backspace) && (MoonInput.GetKey(KeyCode.RightControl) || MoonInput.GetKey(KeyCode.RightControl));

			if (rhtTrg || lftTrg || kbPlay || kbRec || kbStop || kbDebug || kbReload || kbRerec) {
				if (!HasFlag(tasState, TASState.Enable) && ((lftStick && rhtTrg) || kbPlay)) {
					tasStateNext |= TASState.Enable;
				} else if (!HasFlag(tasState, TASState.Rerecord) && kbRerec) {
					tasStateNext |= TASState.Rerecord;
				} else if (HasFlag(tasState, TASState.Enable) && (dpD || kbStop)) {
					DisableRun();
				} else if (!HasFlag(tasState, TASState.Reload) && HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && (dpU || kbReload)) {
					tasStateNext |= TASState.Reload;
				} else if (!HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && ((lftStick && lftTrg) || kbRec)) {
					tasStateNext |= TASState.Record;
				} else if (!HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && kbDebug) {
					tasStateNext |= TASState.OpenDebug;
				}
			}

			if (!rhtTrg && !kbPlay && !kbRec && !kbDebug && !kbReload && !kbRerec) {
				if (HasFlag(tasStateNext, TASState.Enable)) {
					ClearKeyPress(true);
					EnableRun();
				} else if (HasFlag(tasStateNext, TASState.Record)) {
					RecordRun();
				} else if (HasFlag(tasStateNext, TASState.Rerecord)) {
					RerecordRun();
				} else if (HasFlag(tasStateNext, TASState.Reload)) {
					ReloadRun();
				} else if (HasFlag(tasStateNext, TASState.OpenDebug)) {
					tasStateNext &= ~TASState.OpenDebug;
					CheatsHandler.Instance.ActivateDebugMenu();
				}
			}
		}
		private static void EnableRun() {
			tasStateNext &= ~TASState.Enable;

			UpdateVariables(false);
		}
		private static void RecordRun() {
			tasStateNext &= ~TASState.Record;

			UpdateVariables(true);
		}
		private static void RerecordRun() {
			tasStateNext &= ~TASState.Rerecord;
			tasState |= TASState.Enable;
			tasState |= TASState.Rerecord;
			player.InitializeRerecording();
		}
		private static void UpdateVariables(bool recording) {
			tasState |= TASState.Enable;
			tasState &= ~TASState.FrameStep;
			if (recording) {
				tasState |= TASState.Record;
				player.InitializeRecording();
			} else {
				tasState &= ~TASState.Record;
				player.InitializePlayback();
			}
		}
		private static void ReloadRun() {
			tasStateNext &= ~TASState.Reload;

			player.ReloadPlayback();
		}
		private static bool HasFlag(TASState state, TASState flag) {
			return (state & flag) == flag;
		}
		public static void DrawText() {
			if (style == null) {
				style = new GUIStyle(DebugMenuB.DebugMenuStyle);
				style.fontStyle = FontStyle.Bold;
				style.alignment = TextAnchor.MiddleLeft;
				style.normal.textColor = Color.white;
			}
			if (HasFlag(tasState, TASState.Enable)) {
				style.fontSize = (int)Mathf.Round(14f);
				GUI.Label(new Rect(0f, 0f, 32, 18), "TAS", style);
			}
		}
		public static void UpdateText() {
			if (HasFlag(tasState, TASState.Enable)) {
				currentInputLine = player.ToString();
				nextInputLine = player.NextInput();
				extraInfo = string.Empty;
				if (Game.Characters.Sein != null) {
					SeinCharacter sein = Game.Characters.Sein;
					extraInfo = string.Concat((sein.IsOnGround ? "OnGround" : "InAir"),
						(sein.PlatformBehaviour.PlatformMovement.IsOnWall ? " OnWall" : ""),
						(sein.PlatformBehaviour.PlatformMovement.IsOnCeiling ? " OnCeiling" : ""),
						(sein.PlatformBehaviour.PlatformMovement.Falling ? " Falling" : ""),
						(sein.PlatformBehaviour.PlatformMovement.Jumping ? " Jumping" : ""),
						((sein.Mortality.DamageReciever?.IsInvinsible).GetValueOrDefault(false) ? " Invincible" : ""),
						(CharacterState.IsActive(sein.Abilities.DoubleJump) && sein.Abilities.DoubleJump.CanDoubleJump ? " CanDoubleJump" : ""),
						(CharacterState.IsActive(sein.Abilities.Jump) && sein.Abilities.Jump.CanJump ? " CanJump" : ""),
						(CharacterState.IsActive(sein.Abilities.WallJump) && sein.Abilities.WallJump.CanPerformWallJump ? " CanWallJump" : ""),
						(CharacterState.IsActive(sein.Abilities.Bash) && sein.Abilities.Bash.CanBash ? " CanBash" : ""),
						(CanPickup(sein) && !sein.Abilities.Carry.IsCarrying ? " CanPickup" : ""),
						(!sein.Abilities.Carry.LockDroppingObject && sein.Abilities.Carry.IsCarrying && sein.PlatformBehaviour.PlatformMovement.IsOnGround && sein.Controller.CanMove ? " CanDrop" : ""),
						(CharacterState.IsActive(sein.Abilities.Grenade) && sein.Abilities.Grenade.FindAutoAttackable != null ? " GrenadeTarget" : ""),
						(CharacterState.IsActive(sein.Abilities.Dash) && sein.Abilities.Dash.CanPerformNormalDash() ? " CanDash" : ""),
						(CharacterState.IsActive(sein.Abilities.Dash) && sein.PlayerAbilities.ChargeDash.HasAbility && sein.Abilities.Dash.HasEnoughEnergy && sein.Abilities.Dash.FindClosestAttackable != null ? " CDashTarget" : ""),
						(CharacterState.IsActive(sein.Abilities.SpiritFlame) && sein.Abilities.SpiritFlameTargetting.ClosestAttackables?.Count > 0 ? " AttackTarget" : ""),
						(!sein.Controller.CanMove ? " InputLocked" : ""));
					int seinsTime = GetSeinsTime();
					extraInfo += GetCurrentTime() == seinsTime && seinsTime > 0 ? " Saved" : "";
				}
				if (GameController.Instance.IsLoadingGame || InstantLoadScenesController.Instance.IsLoading) {
					extraInfo += " Loading";
				}
				currentInputLine += " RNG(" + FixedRandom.FixedUpdateIndex + ")";
				extraInfo = extraInfo.Trim();
			} else {
				currentInputLine = string.Empty;
				nextInputLine = string.Empty;
				extraInfo = string.Empty;
			}
		}
		public static bool CanPickup(SeinCharacter sein) {
			if (sein.Controller.CanMove && sein.PlatformBehaviour.PlatformMovement.IsOnGround) {
				for (int i = 0; i < Game.Items.Carryables.Count; i++) {
					ICarryable carryable = Game.Items.Carryables[i];
					if (Vector3.Distance(((Component)carryable).transform.position, sein.Position) < sein.Abilities.Carry.CarryRange && carryable != null && carryable.CanBeCarried()) {
						return true;
					}
				}
			}
			return false;
		}
		private static int GetCurrentTime() {
			return GameController.Instance.Timer.Hours * 3600 + GameController.Instance.Timer.Minutes * 60 + GameController.Instance.Timer.Seconds;
		}
		private static int GetSeinsTime() {
			if (GameStateMachine.Instance.CurrentState == GameStateMachine.State.Game && Game.Characters.Sein != null) {
				return SaveSlotsManager.CurrentSaveSlot.Hours * 3600 + SaveSlotsManager.CurrentSaveSlot.Minutes * 60 + SaveSlotsManager.CurrentSaveSlot.Seconds;
			}
			return -1;
		}
	}
}