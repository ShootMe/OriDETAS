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
		OpenDebug = 32
	}
	public class TAS {
		private static TASState tasStateNext, tasState;
		private static string filePath = "Ori.tas";
		private static TASPlayer player = new TASPlayer(filePath);
		public static float deltaTime = 0.016666667f, timeScale = 1f;
		public static int frameRate = 0;
		private static GUIStyle style;
		private static float currentSpeed = 0;
		private static bool slowDownFromKB = false, isSuspended = false;
		private static HashSet<ISuspendable> suspendables = new HashSet<ISuspendable>();

		static TAS() {
			DebugMenuB.MakeDebugMenuExist();
		}
		public static bool UpdateTAS() {
			HandleFrameRates();
			CheckControls();
			FrameStepping();

			if (HasFlag(tasState, TASState.Enable) && !isSuspended) {
				if (HasFlag(tasState, TASState.Record)) {
					player.RecordPlayer();
				} else {
					player.PlaybackPlayer();

					if (!player.CanPlayback) {
						DisableRun();
					}
					return true;
				}
			}
			return false;
		}
		private static void HandleFrameRates() {
			if (HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.FrameStep) && !HasFlag(tasState, TASState.Record)) {
				float rsX = XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX);
				if (!HasFlag(tasStateNext, TASState.ChangeSpeed)) {


					tasStateNext |= TASState.ChangeSpeed;
				} else if (!MoonInput.GetKey(UnityEngine.KeyCode.T) && !MoonInput.GetKey(UnityEngine.KeyCode.R) && !MoonInput.GetKey(UnityEngine.KeyCode.F)) {
					tasStateNext &= ~TASState.ChangeSpeed;
				}

				if (MoonInput.GetKey(UnityEngine.KeyCode.T)) {
					currentSpeed = 0.65f;
					if (isSuspended) {
						SuspensionManager.ResumeExcluding(suspendables);
						suspendables.Clear();
						slowDownFromKB = false;
						isSuspended = false;
					}
				} else if (MoonInput.GetKey(UnityEngine.KeyCode.R)) {
					currentSpeed = -0.75f;
					if (isSuspended) {
						SuspensionManager.ResumeExcluding(suspendables);
						suspendables.Clear();
						slowDownFromKB = false;
						isSuspended = false;
					}
				} else if (MoonInput.GetKey(UnityEngine.KeyCode.G)) {
					currentSpeed = 0;
					if (isSuspended) {
						SuspensionManager.ResumeExcluding(suspendables);
						suspendables.Clear();
						slowDownFromKB = false;
						isSuspended = false;
					}
				} else if (MoonInput.GetKey(UnityEngine.KeyCode.F)) {
					slowDownFromKB = true;
				}

				if (slowDownFromKB) {
					if (isSuspended) {
						SuspensionManager.ResumeExcluding(suspendables);
						suspendables.Clear();
					} else {
						SuspensionManager.GetSuspendables(suspendables, Game.UI.Cameras.Current.GameObject);
						SuspensionManager.SuspendExcluding(suspendables);
						slowDownFromKB = false;
					}
					isSuspended = !isSuspended;
				}

				if (!isSuspended) {
					if (currentSpeed != 0) {
						rsX += currentSpeed;
					}

					if (rsX <= -1.2) {
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
				}
			} else {
				SetFrameRate();
			}
		}
		private static void SetFrameRate(int newFrameRate = 60) {
			if (frameRate == newFrameRate) { return; }

			frameRate = newFrameRate;
			timeScale = (float)newFrameRate / 60f;
			UnityEngine.Time.timeScale = timeScale;
			UnityEngine.Time.captureFramerate = 60;
			Application.targetFrameRate = 60;
			UnityEngine.Time.fixedDeltaTime = 1f / 60f;
			UnityEngine.Time.maximumDeltaTime = UnityEngine.Time.fixedDeltaTime;
			QualitySettings.vSyncCount = 0;
		}
		private static void FrameStepping() {
			float rsX = XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX);
			bool lftShd = XboxControllerInput.GetButton(XboxControllerInput.Button.LeftTrigger);
			bool rhtShd = XboxControllerInput.GetButton(XboxControllerInput.Button.RightTrigger);
			bool dpU = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) > 0.1f;
			bool dpD = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) < -0.1f;

			if (HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && (HasFlag(tasState, TASState.FrameStep) || dpU && !lftShd && !rhtShd)) {
				bool ap = dpU;
				while (HasFlag(tasState, TASState.Enable)) {
					rsX = XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX);
					lftShd = XboxControllerInput.GetButton(XboxControllerInput.Button.LeftTrigger);
					rhtShd = XboxControllerInput.GetButton(XboxControllerInput.Button.RightTrigger);
					dpU = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) > 0.1f;
					dpD = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) < -0.1f;

					CheckControls();
					if (!ap && ((dpU && !lftShd && !rhtShd))) {
						tasState |= TASState.FrameStep;
						break;
					} else if ((dpD && !lftShd && !rhtShd)) {
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
					}
					ap = dpU;
					Thread.Sleep(1);
				}
			}
		}
		private static void DisableRun() {
			tasState &= ~TASState.Enable;
			tasState &= ~TASState.FrameStep;
			tasState &= ~TASState.Record;
			currentSpeed = 0;
		}
		private static void CheckControls() {
			float rsX = XboxControllerInput.GetAxis(XboxControllerInput.Axis.RightStickX);
			bool lftShd = XboxControllerInput.GetButton(XboxControllerInput.Button.LeftTrigger);
			bool rhtShd = XboxControllerInput.GetButton(XboxControllerInput.Button.RightTrigger);
			bool dpU = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) > 0.1f;
			bool dpD = XboxControllerInput.GetAxis(XboxControllerInput.Axis.DpadY) < -0.1f;
			bool kbPlay = MoonInput.GetKey(UnityEngine.KeyCode.B);
			bool kbRec = MoonInput.GetKey(UnityEngine.KeyCode.N);
			bool kbStop = MoonInput.GetKey(UnityEngine.KeyCode.J);
			bool kbDebug = MoonInput.GetKey(UnityEngine.KeyCode.F8);
			bool shift = MoonInput.GetKey(UnityEngine.KeyCode.LeftShift) || MoonInput.GetKey(UnityEngine.KeyCode.RightShift);

			if ((lftShd && rhtShd) || kbPlay || kbRec || kbStop || kbDebug) {
				if (!HasFlag(tasState, TASState.Enable) && (XboxControllerInput.GetButton(XboxControllerInput.Button.RightStick) || (kbPlay && !shift))) {
					tasStateNext |= TASState.Enable;
				} else if (HasFlag(tasState, TASState.Enable) && (dpD || kbStop)) {
					DisableRun();
				} else if (!HasFlag(tasState, TASState.Reload) && HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && (dpU || (kbPlay && shift))) {
					tasStateNext |= TASState.Reload;
				} else if (!HasFlag(tasState, TASState.Record) && (XboxControllerInput.GetButton(XboxControllerInput.Button.LeftStick) || kbRec)) {
					tasStateNext |= TASState.Record;
				} else if (!HasFlag(tasState, TASState.Record) && kbDebug) {
					tasStateNext |= TASState.OpenDebug;
				}
			}

			if (!lftShd && !rhtShd && !kbPlay && !kbRec && !kbDebug) {
				if (HasFlag(tasStateNext, TASState.Enable)) {
					EnableRun();
				} else if (HasFlag(tasStateNext, TASState.Record)) {
					RecordRun();
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
				style.fontSize = (int)Mathf.Round(22f);
				string msg = player.ToString();
				string next = player.NextInput();
				if (next.Trim() != string.Empty) {
					msg += "   Next: " + next;
				}
				msg += "   FPS: " + frameRate;
				float height = 30f;
				string extra = string.Empty;
				if (Game.Characters.Sein != null) {
					SeinCharacter sein = Game.Characters.Sein;
					extra = (sein.IsOnGround ? "OnGround" : "InAir") + (sein.PlatformBehaviour.PlatformMovement.IsOnWall ? " OnWall" : "") + (sein.PlatformBehaviour.PlatformMovement.Falling ? " Falling" : "") + (sein.PlatformBehaviour.PlatformMovement.Jumping ? " Jumping" : "") + (sein.Abilities.Jump.CanJump ? " CanJump" : "") + (GameController.Instance.InputLocked ? " InputLocked" : "");
					int seinsTime = GetSeinsTime();
					extra += GetCurrentTime() == seinsTime && seinsTime > 0 ? " Saved" : "";
				}
				if (GameController.Instance.IsLoadingGame || InstantLoadScenesController.Instance.IsLoading || GameController.FreezeFixedUpdate) {
					extra += " Loading";
				}
				msg += " RNG: " + FixedRandom.FixedUpdateIndex;
				msg += extra.Length > 0 ? " (" + extra.Trim() + ")" : "";

				GUI.Label(new Rect(0f, 0f, Screen.width, height), msg, style);
			}
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