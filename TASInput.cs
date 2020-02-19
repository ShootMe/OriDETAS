using Game;
using System;
using System.Reflection;
using UnityEngine;
namespace OriTAS {
    public class TASInput {
        public int Frames { get; set; }
        public bool Jump { get; set; }
        public bool Save { get; set; }
        public bool DSave { get; set; }
        public bool DLoad { get; set; }
        public bool Fire { get; set; }
        public bool Action { get; set; }
        public bool Bash { get; set; }
        public bool Start { get; set; }
        public bool Select { get; set; }
        public bool Esc { get; set; }
        public float XAxis { get; set; }
        public float YAxis { get; set; }
        public float MouseX { get; set; }
        public float MouseY { get; set; }
        public bool ChargeJump { get; set; }
        public bool Glide { get; set; }
        public bool Dash { get; set; }
        public bool Grenade { get; set; }
        public bool UI { get; set; }
        public int Line1 { get; set; }
        public int Line2 { get; set; }
        public bool Position { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public bool Color { get; set; }
        public bool NoColor { get; set; }
        public bool Speed { get; set; }
        public float SpeedX { get; set; }
        public float SpeedY { get; set; }
        public bool EntityPos { get; set; }
        public float EntityPosX { get; set; }
        public float EntityPosY { get; set; }
        public int SaveSlot { get; set; }
        public int XP { get; set; }
        public float EN { get; set; }
        public float HP { get; set; }
        public int Random { get; set; }
        public bool Restore { get; set; }
        public int Copy { get; set; }
        public int SkillTree { get; set; }
        public bool TAS { get; set; }
        public bool BlockPos { get; set; }
        public float BlockPosX { get; set; }
        public float BlockPosY { get; set; }
        public bool SeinPos { get; set; }
        public float SeinPosX { get; set; }
        public float SeinPosY { get; set; }
        public float EntityHP { get; set; }
        public string Spawn { get; set; }
        public float SpawnX { get; set; }
        public float SpawnY { get; set; }
        public bool ResetDash { get; set; }
        public bool SpamAction { get; set; }

        public bool KBUp { get; set; }
        public bool KBDown { get; set; }
        public bool KBOverride { get; set; }

        public TASInput() {
            this.MouseX = -1;
            this.MouseY = -1;
            this.SaveSlot = -1;
            this.Copy = -1;
            this.XP = -1;
            this.EN = -1;
            this.HP = -1;
            this.Random = -1;
            this.SkillTree = -1;
            this.EntityHP = -1;
            this.SpamAction = false;
        }
        public TASInput(int frames) {
            this.Frames = frames;
            this.XP = -1;
            this.EN = -1;
            this.HP = -1;
            this.Random = -1;
            this.Copy = -1;
            this.SkillTree = -1;
            this.EntityHP = -1;
            this.Esc = Core.Input.Cancel.IsPressed;
            this.Action = Core.Input.ActionButtonA.IsPressed;
            this.Dash = Core.Input.RightShoulder.IsPressed;
            this.Grenade = Core.Input.LeftShoulder.IsPressed;
            this.Jump = Core.Input.Jump.IsPressed;
            this.Save = Core.Input.SoulFlame.IsPressed;
            this.Fire = Core.Input.SpiritFlame.IsPressed;
            this.Bash = Core.Input.Bash.IsPressed;
            this.Start = Core.Input.Start.IsPressed;
            this.Select = Core.Input.Select.IsPressed;
            this.ChargeJump = Core.Input.ChargeJump.IsPressed;
            this.Glide = Core.Input.Glide.IsPressed;

            float dp = Core.Input.HorizontalDigiPad;
            float ana = Core.Input.HorizontalAnalogLeft;
            this.XAxis = dp < -0.1f ? -1 : (ana < -0.04f ? ana : (dp > 0.1f ? 1 : (ana > 0.04f ? ana : 0)));
            dp = Core.Input.VerticalDigiPad;
            ana = Core.Input.VerticalAnalogLeft;
            this.YAxis = dp < -0.1f ? -1 : (ana < -0.04f ? ana : (dp > 0.1f ? 1 : (ana > 0.04f ? ana : 0)));
            this.UI = (MoonInput.GetKey(KeyCode.LeftAlt) || MoonInput.GetKey(UnityEngine.KeyCode.RightAlt)) && MoonInput.GetKeyDown(UnityEngine.KeyCode.U);
            if (Core.Input.CursorMoved) {
                Vector2 vector = Core.Input.CursorPosition;
                this.MouseX = vector.x;
                this.MouseY = vector.y;
            } else {
                this.MouseX = -1;
                this.MouseY = -1;
            }
            this.SaveSlot = -1;
            this.SpamAction = false;
        }
        public TASInput(string line, int lineNum1, int lineNum2) {
            try {
                string[] parameters = line.Split(',');

                this.MouseX = -1;
                this.MouseY = -1;
                this.XP = -1;
                this.EN = -1;
                this.HP = -1;
                this.Random = -1;
                this.Line1 = lineNum1;
                this.Line2 = lineNum2;
                this.SaveSlot = -1;
                int frames = 0;
                this.Copy = -1;
                this.SkillTree = -1;
                this.EntityHP = -1;
                this.SpamAction = false;
                if (!int.TryParse(parameters[0], out frames)) { return; }
                for (int i = 1; i < parameters.Length; i++) {
                    float temp;
                    switch (parameters[i].ToUpper().Trim()) {
                        case "JUMP": Jump = true; break;
                        case "ESC": Esc = true; break;
                        case "ACTION": Action = true; break;
                        case "DASH": Dash = true; break;
                        case "GRENADE": Grenade = true; break;
                        case "SAVE": Save = true; break;
                        case "GLIDE": Glide = true; break;
                        case "FIRE": Fire = true; break;
                        case "BASH": Bash = true; break;
                        case "START": Start = true; break;
                        case "SELECT": Select = true; break;
                        case "CJUMP": ChargeJump = true; break;
                        case "LEFT": XAxis = -1; break;
                        case "RIGHT": XAxis = 1; break;
                        case "UP": YAxis = 1; break;
                        case "DOWN": YAxis = -1; break;
                        case "UI": UI = true; break;
                        case "COLOR": Color = true; break;
                        case "NOCOLOR": NoColor = true; break;
                        case "DSAVE": DSave = true; break;
                        case "DLOAD": DLoad = true; break;
                        case "RESTORE": Restore = true; break;
                        case "TAS": TAS = true; break;
                        case "SPAWN":
                            Spawn = parameters[i + 1];
                            if (float.TryParse(parameters[i + 2], out temp)) { this.SpawnX = temp; }
                            if (float.TryParse(parameters[i + 3], out temp)) { this.SpawnY = temp; }
                            i += 3;
                            break;
                        case "RESETDASH": ResetDash = true; break;
                        case "RANDOM":
                        case "RNG":
                            int rngAmount = 0;
                            if (int.TryParse(parameters[i + 1], out rngAmount)) { this.Random = rngAmount; }
                            i += 1;
                            break;
                        case "XP":
                            int xpAmount = 0;
                            if (int.TryParse(parameters[i + 1], out xpAmount)) { this.XP = xpAmount; }
                            i += 1;
                            break;
                        case "EN":
                            if (float.TryParse(parameters[i + 1], out temp)) { this.EN = temp; }
                            i += 1;
                            break;
                        case "HP":
                            if (float.TryParse(parameters[i + 1], out temp)) { this.HP = temp; }
                            i += 1;
                            break;
                        case "ENTITYHP":
                            if (float.TryParse(parameters[i + 1], out temp)) { this.EntityHP = temp; }
                            i += 1;
                            break;
                        case "SLOT":
                            int saveSlot = 0;
                            if (int.TryParse(parameters[i + 1], out saveSlot)) { this.SaveSlot = saveSlot - 1; }
                            i += 1;
                            break;
                        case "COPY":
                            int copySlot = 0;
                            if (int.TryParse(parameters[i + 1], out copySlot)) { this.Copy = copySlot - 1; }
                            i += 1;
                            break;
                        case "SKILLTREE":
                            int treeAlpha = 0;
                            if (int.TryParse(parameters[i + 1], out treeAlpha)) { this.SkillTree = treeAlpha; }
                            i += 1;
                            break;
                        case "ANGLE":
                            if (float.TryParse(parameters[i + 1], out temp)) {
                                this.XAxis = (float)Math.Sin(temp * Math.PI / 180.0);
                                this.YAxis = (float)Math.Cos(temp * Math.PI / 180.0);
                            }
                            i += 1;
                            break;
                        case "XAXIS":
                            if (float.TryParse(parameters[i + 1], out temp)) { this.XAxis = temp; }
                            i += 1;
                            break;
                        case "YAXIS":
                            if (float.TryParse(parameters[i + 1], out temp)) { this.YAxis = temp; }
                            i += 1;
                            break;
                        case "MOUSE":
                            if (float.TryParse(parameters[i + 1], out temp)) { this.MouseX = temp; }
                            if (float.TryParse(parameters[i + 2], out temp)) { this.MouseY = temp; }
                            i += 2;
                            break;
                        case "POS":
                        case "POSITION":
                            Position = true;
                            if (float.TryParse(parameters[i + 1], out temp)) { this.PositionX = temp; }
                            if (float.TryParse(parameters[i + 2], out temp)) { this.PositionY = temp; }
                            i += 2;
                            break;
                        case "SPEED":
                            Speed = true;
                            if (float.TryParse(parameters[i + 1], out temp)) { this.SpeedX = temp; }
                            if (float.TryParse(parameters[i + 2], out temp)) { this.SpeedY = temp; }
                            i += 2;
                            break;
                        case "SEINPOS":
                            SeinPos = true;
                            if (float.TryParse(parameters[i + 1], out temp)) { this.SeinPosX = temp; }
                            if (float.TryParse(parameters[i + 2], out temp)) { this.SeinPosY = temp; }

                            i += 2;
                            break;
                        case "KBOVERRIDE":
                            KBOverride = true;
                            break;
                        case "KBUP":
                            KBUp = true;
                            break;
                        case "KBDOWN":
                            KBDown = true;
                            break;
                        case "ENTITYPOS":
                        case "ENTITYPOSITION":
                            EntityPos = true;
                            if (float.TryParse(parameters[i + 1], out temp)) { this.EntityPosX = temp; }
                            if (float.TryParse(parameters[i + 2], out temp)) { this.EntityPosY = temp; }
                            i += 2;
                            break;
                        case "BLOCKPOS":
                        case "BLOCKPOSITION":
                            BlockPos = true;
                            if (float.TryParse(parameters[i + 1], out temp)) { this.BlockPosX = temp; }
                            if (float.TryParse(parameters[i + 2], out temp)) { this.BlockPosY = temp; }
                            i += 2;
                            break;
                        case "SPAMACTION":
                            SpamAction = true;
                            break;
                    }
                }
                Frames = frames;
            } catch { }
        }
        public void UpdateInput(bool initial = false) {
            if (SaveSlot >= 0) {
                SaveSlotsManager.CurrentSlotIndex = SaveSlot;
                SaveSlotsManager.BackupIndex = -1;
            }
            if (DSave && initial && GameController.Instance != null) {
                GameController.Instance.CreateCheckpoint();
                GameController.Instance.SaveGameController.PerformSave();
            }
            if (Copy >= 0) {
                SaveSlotsManager.CopySlot(Copy, SaveSlotsManager.CurrentSlotIndex);
                SaveSlotsManager.BackupIndex = -1;
            }
            if ((DLoad || Restore) && initial && GameController.Instance != null) {
                GameController.Instance.SaveGameController.PerformLoad();
                if (Core.Scenes.Manager != null) {
                    FieldInfo fieldDelayTime = typeof(ScenesManager).GetField("m_testDelayTime", BindingFlags.NonPublic | BindingFlags.Instance);
                    fieldDelayTime.SetValue(Core.Scenes.Manager, 1f);
                }
            }
            if (BlockPos && initial && Characters.Sein != null && CharacterState.IsActive(Characters.Sein.Abilities.GrabBlock)) {
                float minDist = 20;
                int index = -1;
                for (int i = 0; i < PushPullBlock.All.Count; i++) {
                    PushPullBlock pushPullBlock = PushPullBlock.All[i];
                    float dist = Vector3.Distance(pushPullBlock.Position, Characters.Sein.Position);
                    if (dist < minDist) {
                        index = i;
                        minDist = dist;
                    }
                }

                if (index >= 0) {
                    FieldInfo fieldTransform = typeof(PushPullBlock).GetField("m_transform", BindingFlags.NonPublic | BindingFlags.Instance);
                    Transform transform = (Transform)fieldTransform.GetValue(PushPullBlock.All[index]);
                    transform.position = new Vector3(BlockPosX, BlockPosY);
                }
            }
            if (!string.IsNullOrEmpty(Spawn) && Characters.Sein != null) {
                int index = -1;
                float dist = float.MaxValue;
                for (int i = 0; i < RespawningPlaceholder.All.Count; i++) {
                    RespawningPlaceholder rp = RespawningPlaceholder.All[i];
                    bool wasNull = false;
                    if (rp.CurrentEntity == null) {
                        rp.Spawn();
                        wasNull = true;
                    }
                    if (rp.CurrentEntity.GetType().Name.IndexOf(Spawn, StringComparison.OrdinalIgnoreCase) >= 0) {
                        float fdist = Vector3.Distance(rp.Position, Characters.Sein.Position);
                        if (fdist < dist) {
                            dist = fdist;
                            index = i;
                        }
                    }
                    if (wasNull) {
                        rp.DestroyCurrentInstance();
                    }
                }

                if (index >= 0 && dist < 1000) {
                    RespawningPlaceholder rp = RespawningPlaceholder.All[index];
                    Vector3 old = rp.transform.position;
                    rp.transform.position = new Vector3(SpawnX, SpawnY, old.z);
                    rp.Spawn();
                    rp.transform.position = old;
                }
            }
            if (ResetDash && initial && Characters.Sein != null) {
                Characters.Sein.Abilities.Dash.ResetDashLimit();
            }
            if ((EntityPos || EntityHP >= 0) && initial && Characters.Sein != null) {
                SeinCharacter sein = Characters.Sein;
                string info = string.Empty;
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

                if (index >= 0) {
                    EntityTargetting attackable = Targets.Attackables[index] as EntityTargetting;
                    if (EntityPos) {
                        attackable.Entity.Position = new Vector3(EntityPosX, EntityPosY);
                    }
                    if (EntityHP >= 0) {
                        if (attackable.Entity.DamageReciever.MaxHealth < EntityHP) {
                            attackable.Entity.DamageReciever.MaxHealth = EntityHP;
                        }
                        attackable.Entity.DamageReciever.Health = EntityHP;
                    }
                }
            }
            if (HP >= 0 && initial && Characters.Sein != null) {
                Characters.Sein.Mortality.Health.SetAmount(HP);
            }
            if (EN >= 0 && initial && Characters.Sein != null) {
                Characters.Sein.Energy.SetCurrent(EN);
            }
            if (Position && initial) {
                SeinCharacter sein = Characters.Sein;
                sein.Position = new UnityEngine.Vector3(PositionX, PositionY);
            }

            if (SeinPos && initial) {
                Ori oriChar = Characters.Ori;
                oriChar.Position = new UnityEngine.Vector3(SeinPosX, SeinPosY);
            }

            if (Speed && initial) {
                SeinCharacter sein = Characters.Sein;
                sein.Speed = new UnityEngine.Vector3(SpeedX, SpeedY);
            }
            if (XP >= 0 && initial) {
                SeinCharacter sein = Characters.Sein;
                sein.Level.Experience = XP;
            }
            if (UI && initial) {
                SeinUI.DebugHideUI = !SeinUI.DebugHideUI;
            }
            if (Color && initial && Characters.Sein != null) {
                CheatsHandler.Instance.ChangeCharacterColor();
            }
            if (NoColor && initial && Characters.Sein != null) {
                Characters.Sein.PlatformBehaviour.Visuals.SpriteRenderer.material.color = new Color(0.50196f, 0.50196f, 0.50196f, 0.5f);
            }

            if (!Position && MouseX > -0.1 && MouseY > -0.1) {
                UnityEngine.Vector2 vector = new UnityEngine.Vector2(MouseX, MouseY);
                Core.Input.CursorMoved = (UnityEngine.Vector2.Distance(vector, Core.Input.CursorPosition) > 0.0001f);
                Core.Input.CursorPosition = vector;
                TASPlayer.LastMouseX = MouseX;
                TASPlayer.LastMouseY = MouseY;
            } else {
                Core.Input.CursorPosition = new UnityEngine.Vector2(TASPlayer.LastMouseX, TASPlayer.LastMouseY);
                Core.Input.CursorMoved = false;
            }

            Core.Input.HorizontalAnalogLeft = XAxis;
            Core.Input.Horizontal = XAxis;
            Core.Input.VerticalAnalogLeft = YAxis;
            Core.Input.Vertical = YAxis;
            PlayerInput.Instance.ApplyDeadzone(ref Core.Input.HorizontalAnalogLeft, ref Core.Input.VerticalAnalogLeft);
            Core.Input.HorizontalAnalogRight = 0;
            Core.Input.VerticalAnalogRight = 0;
            PlayerInput.Instance.ApplyDeadzone(ref Core.Input.HorizontalAnalogRight, ref Core.Input.VerticalAnalogRight);
            Core.Input.HorizontalDigiPad = (int)XAxis;
            Core.Input.VerticalDigiPad = (int)YAxis;

            Core.Input.AnyStart.Update(Jump || Start);
            Core.Input.ZoomIn.Update(Glide);
            Core.Input.ZoomOut.Update(ChargeJump);
            Core.Input.LeftClick.Update(false);
            Core.Input.RightClick.Update(false);

            Core.Input.Jump.Update(Jump);
            Core.Input.SpiritFlame.Update(Fire);
            Core.Input.SoulFlame.Update(Save);
            Core.Input.Bash.Update(Bash);
            Core.Input.ChargeJump.Update(ChargeJump);
            Core.Input.Glide.Update(Glide);
            Core.Input.Grab.Update(Glide);
            Core.Input.LeftShoulder.Update(Grenade);
            Core.Input.RightShoulder.Update(Dash);
            Core.Input.Select.Update(Select);
            Core.Input.Start.Update(Start);
            Core.Input.LeftStick.Update(false);
            Core.Input.RightStick.Update(false);

            Core.Input.MenuDown.Update(YAxis == -1);
            Core.Input.MenuUp.Update(YAxis == 1);
            Core.Input.MenuLeft.Update(XAxis == -1);
            Core.Input.MenuRight.Update(XAxis == 1);
            Core.Input.MenuPageLeft.Update(ChargeJump);
            Core.Input.MenuPageRight.Update(Glide);
            Core.Input.ActionButtonA.Update(Action);
            Core.Input.Cancel.Update(Esc);
            Core.Input.Copy.Update(Fire);
            Core.Input.Delete.Update(Bash);
            Core.Input.Focus.Update(Fire);
            Core.Input.Filter.Update(Fire);
            Core.Input.Legend.Update(Bash);
        }
        public static bool operator ==(TASInput one, TASInput two) {
            if ((object)one == null && (object)two != null) {
                return false;
            } else if ((object)one != null && (object)two == null) {
                return false;
            } else if ((object)one == null && (object)two == null) {
                return true;
            }
            return one.Jump == two.Jump && one.Save == two.Save && one.Fire == two.Fire && one.Bash == two.Bash &&
                one.Action == two.Action && one.Esc == two.Esc && one.Dash == two.Dash && one.Grenade == two.Grenade &&
                one.Start == two.Start && one.Select == two.Select && one.UI == two.UI && one.XAxis == two.XAxis &&
                one.YAxis == two.YAxis && one.MouseX == two.MouseX && one.MouseY == two.MouseY && one.Glide == two.Glide && one.ChargeJump == two.ChargeJump;
        }
        public static bool operator !=(TASInput one, TASInput two) {
            if ((object)one == null && (object)two != null) {
                return true;
            } else if ((object)one != null && (object)two == null) {
                return true;
            } else if ((object)one == null && (object)two == null) {
                return false;
            }
            return one.Jump != two.Jump || one.Save != two.Save || one.Fire != two.Fire || one.Bash != two.Bash ||
                one.Action != two.Action || one.Esc != two.Esc || one.Dash != two.Dash || one.Grenade != two.Grenade ||
                one.Start != two.Start || one.Select != two.Select || one.UI != two.UI || one.XAxis != two.XAxis ||
                one.YAxis != two.YAxis || one.MouseX != two.MouseX || one.MouseY != two.MouseY || one.Glide != two.Glide || one.ChargeJump != two.ChargeJump;
        }
        public override string ToString() {
            return Frames.ToString().PadLeft(4, ' ') + (Jump ? ",Jump" : "") + (Save ? ",Save" : "") + (Fire ? ",Fire" : "") + (Bash ? ",Bash" : "") +
                (ChargeJump ? ",CJump" : "") + (Glide ? ",Glide" : "") + (Start ? ",Start" : "") + (Select ? ",Select" : "") + (UI ? ",UI" : "") +
                (Action ? ",Action" : "") + (Esc ? ",Esc" : "") + (Dash ? ",Dash" : "") + (Grenade ? ",Grenade" : "") +
                Axis() + (DLoad ? ",DLoad" : "") + (DSave ? ",DSave" : "") + (SaveSlot >= 0 ? ",Slot," + (SaveSlot + 1) : "") +
                (!Position ? "" : ",Pos," + PositionX.ToString("0.####") + "," + PositionY.ToString("0.####")) +
                (!Speed ? "" : ",Speed," + SpeedX.ToString("0.####") + "," + SpeedY.ToString("0.####")) +
                (XP >= 0 ? ",XP," + XP : "") + (Color ? ",Color" : "") + (Random >= 0 ? ",Random," + Random : "") +
                (!EntityPos ? "" : ",EntityPos," + EntityPosX.ToString("0.####") + "," + EntityPosY.ToString("0.####")) +
                (!BlockPos ? "" : ",BlockPos," + BlockPosX.ToString("0.####") + "," + BlockPosY.ToString("0.####")) +
                (EntityHP < 0 ? "" : ",EntityHP," + EntityHP.ToString("0.##")) + (HP >= 0 ? ",HP," + HP.ToString("0.#") : "") + (EN >= 0 ? ",EN," + EN.ToString("0.##") : "") +
                (Restore ? ",Restore" : "") + (Copy >= 0 ? ",Copy," + (Copy + 1) : "") + (SkillTree >= 0 ? ",SkillTree," + SkillTree : "") +
                (MouseX < 0 && MouseY < 0 ? "" : ",Mouse," + MouseX.ToString("0.####") + "," + MouseY.ToString("0.####"));
        }
        public string Axis() {
            bool hasX = XAxis > 0.001f || XAxis < -0.001f;
            bool hasY = YAxis > 0.001f || YAxis < -0.001f;
            if (hasX && !hasY) {
                return XAxis <= -0.99f ? ",Left" : XAxis >= 0.99f ? ",Right" : ",XAxis," + XAxis.ToString("0.####");
            } else if (hasY && !hasX) {
                return YAxis <= -0.99f ? ",Down" : YAxis >= 0.99f ? ",Up" : ",YAxis," + YAxis.ToString("0.####");
            } else if (hasX && hasY) {
                double magnitude = Math.Sqrt(XAxis * XAxis + YAxis * YAxis);
                if (magnitude < 0.9) {
                    return ",XAxis," + XAxis.ToString("0.####") + ",YAxis," + YAxis.ToString("0.####");
                } else {
                    double angle = Math.Atan2(XAxis, YAxis) * 180 / Math.PI;
                    if (angle < 0) {
                        angle += 360;
                    }
                    return ",Angle," + angle.ToString("0.##");
                }
            }
            return "";
        }
        public string DisplayText() {
            return "Line " + Line1 + (Line2 > 0 ? "-" + Line2 : "") + " (" + ToString().Trim() + ")";
        }
        public override bool Equals(object obj) {
            return base.Equals(obj);
        }
        public override int GetHashCode() {
            return (Line1 + Line2) | (Frames << 16);
        }
        public int Line {
            get { return Line1 + (Line2 > 0 ? Line2 - 1 : 0); }
        }
    }
}