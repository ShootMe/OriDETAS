using Game;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using SmartInput;
using System;
using System.Threading;
using UnityEngine;
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
		private static Vector3 lastTargetPosition;
		private static Vector3 oriPostion;
		private static DateTime lastColorCheck = DateTime.MinValue, lastFileWrite = DateTime.MinValue;
		private static bool customColor = false, customRotation = false;
		private static List<Color> colors = new List<Color>();
		private static int colorIndex = 0;

		static TAS() {
			DebugMenuB.MakeDebugMenuExist();
		}
		public static bool UpdateTAS() {
			if (Characters.Sein != null) {
				oriPostion = Characters.Sein.Position;

				if (DateTime.Now > lastColorCheck.AddSeconds(5)) {
					lastColorCheck = DateTime.Now;
					bool found = false;

					if (File.Exists("Color.txt") && File.GetLastWriteTime("Color.txt") > lastFileWrite) {
						lastFileWrite = DateTime.Now;

						string text = File.ReadAllText("Color.txt").ToLower();

						string[] lines = text.Split(new char[] { '\n' });

						if (lines[0].Trim().Equals("customrotation")) {
							colors.Clear();
							float red = 0, green = 0, blue = 0, alpha = 0;
							float frames, red2, green2, blue2, alpha2;

							for (int i = 1; i < lines.Length - 1; i++) {
								string[] components = lines[i].Split(new char[] { ',' });
								if (components != null && components.Length >= 4) {
									float.TryParse(components[0], out red);
									float.TryParse(components[1], out green);
									float.TryParse(components[2], out blue);
									float.TryParse(components[3], out alpha);

									red /= 511f;
									green /= 511f;
									blue /= 511f;
									alpha /= 511f;

									colors.Add(new Color(red, green, blue, alpha));
								}

								components = lines[i + 1].Split(new char[] { ',' });
								if (components != null && components.Length >= 5) {
									float.TryParse(components[0], out red2);
									float.TryParse(components[1], out green2);
									float.TryParse(components[2], out blue2);
									float.TryParse(components[3], out alpha2);
									float.TryParse(components[4], out frames);

									red2 /= 511f;
									green2 /= 511f;
									blue2 /= 511f;
									alpha2 /= 511f;

									for (int j = 1; j <= (int)frames; j++) {
										colors.Add(new Color(red + (red2 - red) * (float)j / frames,
											green + (green2 - green) * (float)j / frames,
											blue + (blue2 - blue) * (float)j / frames,
											alpha + (alpha2 - alpha) * (float)j / frames));
									}
								}
							}

							customColor = false;
							customRotation = true;
							found = true;
						} else {
							colors.Clear();
							customRotation = false;
							string[] components = text.Split(new char[] { ',' });

							if (components != null) {
								if (components.Length == 3 || components.Length == 4) {
									float red = 0, green = 0, blue = 0, alpha = 0;
									float.TryParse(components[0], out red);
									float.TryParse(components[1], out green);
									float.TryParse(components[2], out blue);

									if (components.Length == 4) {
										float.TryParse(components[3], out alpha);
									} else {
										alpha = 255;
									}

									colors.Add(new Color(red / 511f, green / 511f, blue / 511f, alpha / 511f));

									found = true;
									customColor = true;
								}
							}
						}
					}

					if (!found && (customColor || customRotation)) {
						customColor = false;
						customRotation = false;
						Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = new Color(0.50196f, 0.50196f, 0.50196f, 0.5f);
					}
				}

				if (customRotation) {
					colorIndex %= colors.Count;
					Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = colors[colorIndex++];
				} else if (customColor) {
					Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = colors[0];
				}
			} else {
				oriPostion = Vector3.zero;
			}

			UpdateText();
			UpdateExtraInfo();
			HandleFrameRates();
			CheckControls();
			FrameStepping();

			if (SkillTreeManager.Instance != null && SkillTreeManager.Instance.NavigationManager.IsVisible) {
				if (!player.HasChangedAlpha) {
					SkillTreeManager.Instance.NavigationManager.FadeAnimator.SetParentOpacity((float)player.SkillTreeAlpha / 100f);
					player.HasChangedAlpha = true;
				}
				UberPostProcess.Instance.SetDoBlur(player.SkillTreeAlpha == 100);
			}

			if (HasFlag(tasState, TASState.Enable)) {
				if (HasFlag(tasState, TASState.Record) || HasFlag(tasState, TASState.Rerecord)) {
					player.RecordPlayer();
				} else {
					player.PlaybackPlayer();

					if (!player.CanPlayback) {
						DisableRun();
					}
					if (!InstantLoadScenesController.Instance.IsLoading && !GameController.Instance.IsLoadingGame && player.Break < 0) {
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
			QualitySettings.vSyncCount = newFrameRate == 60 ? 1 : 0;
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
			PlayerInput.Instance.Active = true;
			player.SkillTreeAlpha = 100;
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
			bool kbRerec = MoonInput.GetKey(KeyCode.Backspace) && (MoonInput.GetKey(KeyCode.LeftControl) || MoonInput.GetKey(KeyCode.RightControl));

			if (rhtTrg || lftTrg || kbPlay || kbRec || kbStop || kbDebug || kbReload || kbRerec) {
				if (!HasFlag(tasState, TASState.Enable) && ((lftStick && rhtTrg && lftTrg) || kbPlay)) {
					tasStateNext |= TASState.Enable;
				} else if (!HasFlag(tasState, TASState.Rerecord) && kbRerec) {
					tasStateNext |= TASState.Rerecord;
				} else if (HasFlag(tasState, TASState.Enable) && (dpD || kbStop)) {
					DisableRun();
				} else if (!HasFlag(tasState, TASState.Reload) && HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && (dpU || kbReload)) {
					tasStateNext |= TASState.Reload;
				} else if (!HasFlag(tasState, TASState.Enable) && !HasFlag(tasState, TASState.Record) && kbRec) {
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
			PlayerInput.Instance.Active = false;
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
			if (HasFlag(tasState, TASState.Enable) && player.ShowTAS) {
				style.fontSize = (int)Mathf.Round(14f);
				GUI.Label(new Rect(0f, 0f, 32, 18), "TAS", style);
			}
		}
		public static void UpdateText() {
			string closest = ClosestTargetInfo();
			if (HasFlag(tasState, TASState.Enable)) {
				currentInputLine = player.ToString() + " RNG(" + FixedRandom.FixedUpdateIndex + ")";
				nextInputLine = player.NextInput() + (string.IsNullOrEmpty(closest) ? "" : "  [" + closest + "]");
			} else {
				currentInputLine = string.Empty;
				nextInputLine = (string.IsNullOrEmpty(closest) ? "" : "[" + closest + "]");
			}
		}
		public static string ClosestTargetInfo() {
			SeinCharacter sein = Characters.Sein;
			string info = string.Empty;
			if (sein != null) {
				float minDist = float.MaxValue;
				int index = -1;
				for (int i = 0; i < Targets.Attackables.Count; i++) {
					EntityTargetting attackable = Targets.Attackables[i] as EntityTargetting;
					if (attackable != null && attackable.IsOnScreen && attackable.Activated) {
						float dist = Vector3.Distance(attackable.Position, sein.Position);
						if (dist < minDist) {
							index = i;
							minDist = dist;
						}
					}
				}

				float blockMinDist = 20;
				int blockIndex = -1;
				for (int i = 0; i < PushPullBlock.All.Count; i++) {
					PushPullBlock pushPullBlock = PushPullBlock.All[i];
					float dist = Vector3.Distance(pushPullBlock.Position, sein.Position);
					if (dist < blockMinDist) {
						blockIndex = i;
						blockMinDist = dist;
					}
				}

				if (blockIndex >= 0 && index >= 0) {
					if (blockMinDist < minDist) {
						info = UpdateBlockInfo(blockIndex);
					} else {
						info = UpdateEnemyInfo(index);
					}
				} else if (blockIndex >= 0) {
					info = UpdateBlockInfo(blockIndex);
				} else if (index >= 0) {
					info = UpdateEnemyInfo(index);
				} else {
					lastTargetPosition = new Vector3(-9999999, -9999999);
				}
			} else {
				lastTargetPosition = new Vector3(-9999999, -9999999);
			}
			return info;
		}
		private static string UpdateBlockInfo(int index) {
			PushPullBlock block = PushPullBlock.All[index];
			if (lastTargetPosition.x < -9999990) {
				lastTargetPosition = block.Position;
			}
			string info = "PushBlock (" + block.Position.x.ToString("0.00") + ", " + block.Position.y.ToString("0.00") + ") (" + ((block.Position.x - lastTargetPosition.x) * 60).ToString("0.00") + ", " + ((block.Position.y - lastTargetPosition.y) * 60).ToString("0.00") + ")";
			lastTargetPosition = block.Position;
			return info;
		}
		private static string UpdateEnemyInfo(int index) {
			EntityTargetting attackable = Targets.Attackables[index] as EntityTargetting;
			string enemyType = enemyType = attackable.Entity.GetType().Name;
			if (enemyType.LastIndexOf("Enemy") >= 0) {
				enemyType = enemyType.Substring(0, enemyType.LastIndexOf("Enemy"));
			}
			Rigidbody rb = attackable.GetComponent<Rigidbody>();
			if (lastTargetPosition.x < -9999990) {
				lastTargetPosition = attackable.Position;
			}
			string info = enemyType + " (" + attackable.Position.x.ToString("0.00") + ", " + attackable.Position.y.ToString("0.00") + ") (" + ((attackable.Position.x - lastTargetPosition.x) * 60).ToString("0.00") + ", " + ((attackable.Position.y - lastTargetPosition.y) * 60).ToString("0.00") + ")";
			lastTargetPosition = attackable.Position;
			return info;
		}
		public static void UpdateExtraInfo() {
			string temp = string.Empty;
			if (Characters.Sein != null) {
				SeinCharacter sein = Characters.Sein;

				temp = string.Concat((sein.IsOnGround ? "OnGround" : ""),
					(sein.PlatformBehaviour.PlatformMovement.IsOnWall ? " OnWall" : ""),
					(sein.PlatformBehaviour.PlatformMovement.IsOnCeiling ? " OnCeiling" : ""),
					(sein.PlatformBehaviour.PlatformMovement.Falling ? " Falling" : ""),
					(sein.PlatformBehaviour.PlatformMovement.Jumping ? " Jumping" : ""),
					((sein.Mortality.DamageReciever?.IsInvinsible).GetValueOrDefault(false) ? " Invincible" : ""),
					(CharacterState.IsActive(sein.Abilities.DoubleJump) && sein.Abilities.DoubleJump.CanDoubleJump && !sein.IsOnGround && !sein.PlatformBehaviour.PlatformMovement.IsOnWall ? " CanDblJump" : ""),
					(CharacterState.IsActive(sein.Abilities.Jump) && sein.Abilities.Jump.CanJump ? " CanJump" : ""),
					(CharacterState.IsActive(sein.Abilities.WallJump) && sein.Abilities.WallJump.CanPerformWallJump ? " CanWallJump" : ""),
					(CharacterState.IsActive(sein.Abilities.Bash) && sein.Abilities.Bash.CanBash ? " CanBash" : ""),
					(CanPickup(sein) && !sein.Abilities.Carry.IsCarrying ? " CanPickup" : ""),
					(!sein.Abilities.Carry.LockDroppingObject && sein.Abilities.Carry.IsCarrying && sein.PlatformBehaviour.PlatformMovement.IsOnGround && sein.Controller.CanMove ? " CanDrop" : ""),
					(CharacterState.IsActive(sein.Abilities.Grenade) && sein.Abilities.Grenade.FindAutoAttackable != null ? " NadeTrgt" : ""),
					(CharacterState.IsActive(sein.Abilities.Stomp) && sein.Abilities.Stomp.CanStomp() ? " CanStomp" : ""),
					(CharacterState.IsActive(sein.Abilities.Dash) && sein.Abilities.Dash.CanPerformNormalDash() ? " CanDash" : ""),
					(CharacterState.IsActive(sein.Abilities.Dash) && sein.PlayerAbilities.ChargeDash.HasAbility && sein.Abilities.Dash.HasEnoughEnergy && sein.Abilities.Dash.FindClosestAttackable != null ? " CDashTrgt" : ""),
					(CharacterState.IsActive(sein.Abilities.SpiritFlame) && sein.Abilities.SpiritFlameTargetting.ClosestAttackables?.Count > 0 ? " AtkTrgt" : ""),
					(sein.SoulFlame != null && sein.SoulFlame.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.Safe && sein.SoulFlame.CanAffordSoulFlame && sein.SoulFlame.PlayerCouldSoulFlame && !sein.SoulFlame.InsideCheckpointMarker ? " CanSave" : ""),
					(sein.SoulFlame != null && sein.SoulFlame.IsSafeToCastSoulFlame == SeinSoulFlame.SoulFlamePlacementSafety.SavePedestal ? " SpiritWell" : ""),
					(sein.SoulFlame != null && sein.PlayerAbilities.Rekindle.HasAbility && sein.SoulFlame.InsideCheckpointMarker ? " CanRekindle" : ""),
					(!sein.Controller.CanMove || !sein.Active || sein.Controller.IgnoreControllerInput ? " InputLocked" : ""));
				int seinsTime = GetSeinsTime();
				temp += GetCurrentTime() == seinsTime && seinsTime > 0 ? " Saved" : "";
			}
			Vector2 currentPos = Characters.Sein == null ? Core.Scenes.Manager.CurrentCameraTargetPosition : new Vector2(Characters.Sein.Position.x, Characters.Sein.Position.y);
			if (Core.Scenes.Manager != null && !Core.Scenes.Manager.IsInsideASceneBoundary(currentPos)) {
				FieldInfo fi = Core.Scenes.Manager.GetType().GetField("m_testDelayTime", BindingFlags.NonPublic | BindingFlags.Instance);
				float timeLeft = (float)fi.GetValue(Core.Scenes.Manager);
				temp += " OOB(" + (int)(timeLeft * 60) + ")";
			}
			if (InstantLoadScenesController.Instance.IsLoading || GameController.Instance.IsLoadingGame || Core.Scenes.Manager.PositionInsideSceneStillLoading(currentPos)) {
				temp += " Loading";
			}
			extraInfo = temp.Trim();
		}
		public static bool CanPickup(SeinCharacter sein) {
			if (sein.Controller.CanMove && sein.PlatformBehaviour.PlatformMovement.IsOnGround) {
				for (int i = 0; i < Items.Carryables.Count; i++) {
					ICarryable carryable = Items.Carryables[i];
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