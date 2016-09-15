using System.Collections.Generic;
using System.IO;
namespace OriTAS {
	public class TASPlayer {
		public static float LastMouseX, LastMouseY;
		public bool FastForward { get; set; }
		public int Break { get; set; }
		private List<TASInput> inputs = new List<TASInput>();
		private TASInput lastInput;
		private int currentFrame, inputIndex, frameToNext, fixedRandom, gameFrame;
		private string filePath;

		public TASPlayer(string filePath) {
			this.filePath = filePath;
		}

		public bool CanPlayback { get { return inputIndex < inputs.Count; } }
		public int CurrentFrame { get { return currentFrame; } }
		public int GameFrame { get { return gameFrame; } }
		public override string ToString() {
			if (frameToNext == 0 && lastInput != null) {
				return lastInput.DisplayText() + " (" + currentFrame.ToString() + " | " + gameFrame.ToString() + ")";
			} else if (inputIndex < inputs.Count && lastInput != null) {
				int inputFrames = lastInput.Frames;
				int startFrame = frameToNext - inputFrames;
				return lastInput.DisplayText() + " (" + (currentFrame - startFrame).ToString() + " / " + inputFrames + " : " + currentFrame + " | " + gameFrame + ")";
			}
			return string.Empty;
		}
		public string NextInput() {
			if (frameToNext != 0 && inputIndex + 1 < inputs.Count) {
				return inputs[inputIndex + 1].DisplayText();
			}
			return string.Empty;
		}
		public void InitializePlayback(bool resetGame = true) {
			ReadFile();

			currentFrame = 0;
			if (resetGame) {
				gameFrame = 0;
			}
			inputIndex = 0;
			if (inputs.Count > 0) {
				lastInput = inputs[0];
				frameToNext = lastInput.Frames;
			} else {
				lastInput = new TASInput();
				frameToNext = 1;
			}
		}
		public void ReloadPlayback() {
			int playedBackFrames = currentFrame;
			InitializePlayback(false);
			currentFrame = playedBackFrames;

			while (currentFrame > frameToNext) {
				if (inputIndex + 1 >= inputs.Count) {
					inputIndex++;
					return;
				}

				lastInput = inputs[++inputIndex];
				frameToNext += lastInput.Frames;

				TASInput nextInput = inputs[inputIndex + 1 < inputs.Count ? inputIndex + 1 : inputIndex];
				if (nextInput.Line > Break && Break > 0) {
					Break = -Break;
					FastForward = false;
				} else if (Break < 0) {
					Break = 0;
				}
			}
		}
		public void InitializeRecording() {
			currentFrame = 0;
			inputIndex = 0;
			lastInput = new TASInput();
			frameToNext = 0;
			gameFrame = 0;
			inputs.Clear();
			string oldFile = "Old" + Path.GetFileNameWithoutExtension(filePath) + ".tas";
			string oldFile2 = "Old" + Path.GetFileNameWithoutExtension(filePath) + "2.tas";
			if (File.Exists(oldFile)) {
				File.Delete(oldFile2);
				File.Move(oldFile, oldFile2);
			}
			if (File.Exists(filePath)) {
				File.Move(filePath, oldFile);
			}
			File.Delete(filePath);
		}
		public void InitializeRerecording() {
			inputs = inputs.GetRange(0, inputIndex + 1);
			string oldFile = "Old" + Path.GetFileNameWithoutExtension(filePath) + ".tas";
			string oldFile2 = "Old" + Path.GetFileNameWithoutExtension(filePath) + "2.tas";
			if (File.Exists(oldFile)) {
				File.Delete(oldFile2);
				File.Move(oldFile, oldFile2);
			}
			if (File.Exists(filePath)) {
				File.Move(filePath, oldFile);
			}
			inputs[inputs.Count - 1].Frames = currentFrame + lastInput.Frames - frameToNext;

			File.AppendAllText(filePath, fixedRandom.ToString() + "\r\n");

			foreach (TASInput input in inputs) {
				File.AppendAllText(filePath, input.ToString() + "\r\n");
			}
			File.AppendAllText(filePath, "// ");
			lastInput.Frames = 0;
		}
		public void PlaybackPlayer() {
			if (inputIndex < inputs.Count) {
				bool changed = false;
				if (!InstantLoadScenesController.Instance.IsLoading) {
					if (currentFrame == 0) {
						LastMouseX = 0;
						LastMouseY = 0;
						SeinUI.DebugHideUI = false;
					}
					changed = currentFrame == 0;

					if (currentFrame >= frameToNext) {
						if (inputIndex + 1 >= inputs.Count) {
							inputIndex++;
							return;
						}
						lastInput = inputs[++inputIndex];
						frameToNext += lastInput.Frames;
						changed = true;
					}

					currentFrame++;

					if (lastInput.Random >= 0 && changed) {
						fixedRandom = lastInput.Random - currentFrame + 1;
					}
					FixedRandom.SetFixedUpdateIndex(fixedRandom + currentFrame);
					lastInput.UpdateInput(changed);

					if (currentFrame >= frameToNext && inputIndex + 1 < inputs.Count) {
						TASInput nextInput = inputs[inputIndex + 1];
						if (nextInput.Line > Break && Break > 0) {
							Break = -Break;
							FastForward = false;
						}
					}
				}
				gameFrame++;
			}
		}
		public void RecordPlayer() {
			TASInput input = new TASInput(currentFrame);
			if (currentFrame == 0 && input == lastInput) {
				return;
			} else {
				if (!InstantLoadScenesController.Instance.IsLoading) {
					if (input != lastInput) {
						if (currentFrame == 0) {
							fixedRandom = FixedRandom.FixedUpdateIndex;
							File.AppendAllText(filePath, fixedRandom.ToString() + "\r\n");
						}
						lastInput.Frames = currentFrame - lastInput.Frames;
						if (lastInput.Frames != 0) {
							File.AppendAllText(filePath, lastInput.ToString() + "\r\n");
						}
						lastInput = input;
					}
					currentFrame++;
					FixedRandom.SetFixedUpdateIndex(fixedRandom + currentFrame);
				}
				gameFrame++;
			}
		}
		private void ReadFile() {
			inputs.Clear();
			if (!File.Exists(filePath)) { return; }

			bool firstLine = true;
			int lines = 0;
			FastForward = false;
			Break = 0;
			using (StreamReader sr = new StreamReader(filePath)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine();

					if (!firstLine) {
						if (line.IndexOf("Stop", System.StringComparison.OrdinalIgnoreCase) == 0) { return; }
						lines++;
						if (Break == 0 && line.IndexOf("BreakQuick", System.StringComparison.OrdinalIgnoreCase) == 0) {
							FastForward = true;
						}
						if (Break == 0 && line.IndexOf("Break", System.StringComparison.OrdinalIgnoreCase) == 0) {
							Break = lines;
							continue;
						}

						TASInput input = new TASInput(line, lines);
						if (input.Frames != 0) {
							inputs.Add(input);
						}
					} else {
						lines++;
						fixedRandom = int.Parse(line);
						firstLine = false;
					}
				}
			}
		}
	}
}