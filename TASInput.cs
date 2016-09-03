using Game;
using SmartInput;

namespace OriTAS {
	public class TASInput {
		public int Frames { get; set; }
		public bool Jump { get; set; }
		public bool Save { get; set; }
		public bool DSave { get; set; }
		public bool DLoad { get; set; }
		public bool Attack { get; set; }
		public bool Action { get; set; }
		public bool Bash { get; set; }
		public bool Start { get; set; }
		public bool Select { get; set; }
		public bool Cancel { get; set; }
		public bool LeftClick { get; set; }
		public bool RightClick { get; set; }
		public float XAxis { get; set; }
		public float YAxis { get; set; }
		public float MouseX { get; set; }
		public float MouseY { get; set; }
		public bool ChargeJump { get; set; }
		public bool Glide { get; set; }
		public bool Dash { get; set; }
		public bool Grenade { get; set; }
		public bool UI { get; set; }
		public int Line { get; set; }

		public TASInput() { }
		public TASInput(int frames) {
			this.Frames = frames;
			this.Cancel = Core.Input.Cancel.IsPressed;
			this.Action = Core.Input.ActionButtonA.IsPressed;
			this.Dash = Core.Input.RightShoulder.IsPressed;
			this.Grenade = Core.Input.LeftShoulder.IsPressed;
			this.Jump = Core.Input.Jump.IsPressed;
			this.Save = Core.Input.SoulFlame.IsPressed;
			this.Attack = Core.Input.SpiritFlame.IsPressed;
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
			this.UI = (MoonInput.GetKey(UnityEngine.KeyCode.LeftAlt) || MoonInput.GetKey(UnityEngine.KeyCode.RightAlt)) && MoonInput.GetKeyDown(UnityEngine.KeyCode.U);
			if (Core.Input.CursorMoved) {
				UnityEngine.Vector2 vector = Core.Input.CursorPosition;
				this.MouseX = vector.x;
				this.MouseY = vector.y;
			}
		}
		public TASInput(string line, int lineNum) {
			try {
				string[] parameters = line.Split(',');

				this.Line = lineNum;
				int frames = 0;
				if (!int.TryParse(parameters[0], out frames)) { return; }
				for (int i = 1; i < parameters.Length; i++) {
					float temp;
					switch (parameters[i].ToUpper().Trim()) {
						case "JUMP": Jump = true; break;
						case "ESC": Cancel = true; break;
						case "ACTION": Action = true; break;
						case "DASH": Dash = true; break;
						case "GRENADE": Grenade = true; break;
						case "SAVE": Save = true; break;
						case "GLIDE": Glide = true; break;
						case "FIRE": Attack = true; break;
						case "BASH": Bash = true; break;
						case "START": Start = true; break;
						case "SELECT": Select = true; break;
						case "LCLICK": LeftClick = true; break;
						case "RCLICK": RightClick = true; break;
						case "CJUMP": ChargeJump = true; break;
						case "LEFT": XAxis = -1; break;
						case "RIGHT": XAxis = 1; break;
						case "UP": YAxis = 1; break;
						case "DOWN": YAxis = -1; break;
						case "UI": UI = true; break;
						case "DSAVE": DSave = true; break;
						case "DLOAD": DLoad = true; break;
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
					}
				}
				Frames = frames;
			} catch { }
		}
		public void UpdateInput(bool initial = false) {
			if (DSave && initial && GameController.Instance != null) {
				GameController.Instance.CreateCheckpoint();
				GameController.Instance.SaveGameController.PerformSave();
			}
			if (DLoad && initial && GameController.Instance != null) {
				GameController.Instance.SaveGameController.PerformLoad();
			}
			UnityEngine.Vector2 vector = new UnityEngine.Vector2(MouseX, MouseY);
			Core.Input.CursorMoved = (UnityEngine.Vector2.Distance(vector, Core.Input.CursorPosition) > 0.0001f);
			Core.Input.CursorPosition = vector;
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
			Core.Input.LeftClick.Update(LeftClick);
			Core.Input.RightClick.Update(RightClick);

			Core.Input.Jump.Update(Jump);
			Core.Input.SpiritFlame.Update(Attack);
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
			Core.Input.Cancel.Update(Cancel);
			Core.Input.Copy.Update(Attack);
			Core.Input.Delete.Update(Bash);
			Core.Input.Focus.Update(Attack);
			Core.Input.Filter.Update(Attack);
			Core.Input.Legend.Update(Bash);

			if (UI && initial) {
				SeinUI.DebugHideUI = !SeinUI.DebugHideUI;
			}
		}
		public static bool operator ==(TASInput one, TASInput two) {
			if ((object)one == null && (object)two != null) {
				return false;
			} else if ((object)one != null && (object)two == null) {
				return false;
			} else if ((object)one == null && (object)two == null) {
				return true;
			}
			return one.Jump == two.Jump && one.Save == two.Save && one.Attack == two.Attack && one.Bash == two.Bash &&
				one.Action == two.Action && one.Cancel == two.Cancel && one.Dash == two.Dash && one.Grenade == two.Grenade &&
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
			return one.Jump != two.Jump || one.Save != two.Save || one.Attack != two.Attack || one.Bash != two.Bash ||
				one.Action != two.Action || one.Cancel != two.Cancel || one.Dash != two.Dash || one.Grenade != two.Grenade ||
				one.Start != two.Start || one.Select != two.Select || one.UI != two.UI || one.XAxis != two.XAxis ||
				one.YAxis != two.YAxis || one.MouseX != two.MouseX || one.MouseY != two.MouseY || one.Glide != two.Glide || one.ChargeJump != two.ChargeJump;
		}
		public override string ToString() {
			return Frames.ToString().PadLeft(4, ' ') + (Jump ? ",Jump" : "") + (Save ? ",Save" : "") + (Attack ? ",Fire" : "") + (Bash ? ",Bash" : "") +
				(ChargeJump ? ",CJump" : "") + (Glide ? ",Glide" : "") + (Start ? ",Start" : "") + (Select ? ",Select" : "") + (UI ? ",UI" : "") +
				(Action ? ",Action" : "") + (Cancel ? ",Esc" : "") + (Dash ? ",Dash" : "") + (Grenade ? ",Grenade" : "") +
				(XAxis == 0 ? "" : (XAxis <= -1 ? ",Left" : (XAxis >= 1 ? ",Right" : ",XAxis," + XAxis.ToString("0.00000")))) +
				(YAxis == 0 ? "" : (YAxis <= -1 ? ",Down" : (YAxis >= 1 ? ",Up" : ",YAxis," + YAxis.ToString("0.00000")))) +
				(MouseX == 0 && MouseY == 0 ? "" : ",Mouse," + MouseX.ToString("0.00000") + "," + MouseY.ToString("0.00000"));
		}
		public string DisplayText() {
			return "Line" + Line + " (" + ToString().Trim() + ")";
		}
		public override bool Equals(object obj) {
			return base.Equals(obj);
		}
		public override int GetHashCode() {
			return Line | (Frames << 16);
		}
	}
}