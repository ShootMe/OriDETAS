using System.Collections.Generic;
using System.IO;
namespace OriTAS {
	public class TASPlayer {
		public static float LastMouseX, LastMouseY;
		private List<TASInput> inputs = new List<TASInput>();
		private TASInput lastInput;
		private int currentFrame, inputIndex, frameToNext, fixedRandom;
		private string filePath;

		public TASPlayer(string filePath) {
			this.filePath = filePath;
		}

		public bool CanPlayback { get { return inputIndex < inputs.Count; } }
		public int CurrentFrame { get { return currentFrame; } }
		public override string ToString() {
			if (frameToNext == 0 && lastInput != null) {
				return lastInput.DisplayText() + " (" + currentFrame.ToString() + ")";
			} else if (inputIndex < inputs.Count && lastInput != null) {
				int inputFrames = lastInput.Frames;
				int startFrame = frameToNext - inputFrames;
				return lastInput.DisplayText() + " (" + (currentFrame - startFrame).ToString() + " / " + inputFrames + " : " + currentFrame + ")";
			}
			return string.Empty;
		}
		public string NextInput() {
			if (frameToNext != 0 && inputIndex + 1 < inputs.Count) {
				return inputs[inputIndex + 1].DisplayText();
			}
			return string.Empty;
		}
		public void InitializePlayback() {
			ReadFile();

			currentFrame = 0;
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
			InitializePlayback();
			currentFrame = playedBackFrames;

			while (currentFrame >= frameToNext) {
				if (inputIndex + 1 >= inputs.Count) {
					inputIndex++;
					return;
				}
				lastInput = inputs[++inputIndex];
				frameToNext += lastInput.Frames;
			}
		}
		public void InitializeRecording() {
			currentFrame = 0;
			inputIndex = 0;
			lastInput = new TASInput();
			frameToNext = 0;
			inputs.Clear();
			File.Delete(filePath);
		}
		public void PlaybackPlayer() {
			if (inputIndex < inputs.Count) {
				bool changed = false;
				if (!GameController.Instance.IsLoadingGame && !InstantLoadScenesController.Instance.IsLoading && !GameController.FreezeFixedUpdate) {
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
				}
				FixedRandom.SetFixedUpdateIndex(fixedRandom + currentFrame);
				lastInput.UpdateInput(changed);
			}
		}
		public void RecordPlayer() {
			TASInput input = new TASInput(currentFrame);
			if (currentFrame == 0 && input == lastInput) {
				return;
			} else if (input != lastInput) {
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
		}
		private void ReadFile() {
			inputs.Clear();
			if (!File.Exists(filePath)) { return; }

			bool firstLine = true;
			int lines = 0;
			using (StreamReader sr = new StreamReader(filePath)) {
				while (!sr.EndOfStream) {
					string line = sr.ReadLine();

					if (!firstLine) {
						TASInput input = new TASInput(line, ++lines);
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