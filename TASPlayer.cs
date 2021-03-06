﻿using Game;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace OriTAS {
    public class TASPlayer {
        public static float LastMouseX, LastMouseY;
        public bool FastForward { get; set; }
        public int Break { get; set; }
        public List<TASInput> inputs = new List<TASInput>();
        public TASInput lastInput;
        public int currentFrame, inputIndex, frameToNext, fixedRandom, gameFrame;
        private string filePath;
        private int skillTreeAlpha = 100;
        public bool ShowTAS { get; set; } = true;
        public int SkillTreeAlpha {
            get { return skillTreeAlpha; }
            set {
                skillTreeAlpha = value;
                HasChangedAlpha = false;
            }
        }
        public bool HasChangedAlpha { get; set; } = true;

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

            SkillTreeAlpha = 100;
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
            int playedBackRNG = fixedRandom;
            int skillTree = SkillTreeAlpha;
            InitializePlayback(false);
            currentFrame = playedBackFrames;
            fixedRandom = playedBackRNG;
            SkillTreeAlpha = skillTree;

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
                Vector2 currentPos = Characters.Sein == null ? Core.Scenes.Manager.CurrentCameraTargetPosition : new Vector2(Characters.Sein.Position.x, Characters.Sein.Position.y);
                if (!InstantLoadScenesController.Instance.IsLoading && (!GameController.Instance.IsLoadingGame || (lastInput != null && lastInput.Restore)) && !Core.Scenes.Manager.PositionInsideSceneStillLoading(currentPos)) {
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

                    if (lastInput.SkillTree >= 0) {
                        SkillTreeAlpha = lastInput.SkillTree;
                    }

                    if (currentFrame >= frameToNext && inputIndex + 1 < inputs.Count) {
                        TASInput nextInput = inputs[inputIndex + 1];
                        if (nextInput.Line > Break && Break > 0) {
                            Break = -Break;
                            FastForward = false;
                        }
                    }
                }
                if (inputs[inputIndex].SpamAction) {
                    Core.Input.ActionButtonA.Update(!Core.Input.ActionButtonA.IsPressed);
                    Core.Input.AnyStart.Update(!Core.Input.AnyStart.IsPressed);
                }
                gameFrame++;
            }
        }
        public void RecordPlayer() {
            TASInput input = new TASInput(currentFrame);
            if (currentFrame == 0 && input == lastInput) {
                return;
            } else {
                if (!InstantLoadScenesController.Instance.IsLoading && !GameController.Instance.IsLoadingGame) {
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
            ShowTAS = true;
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

                        if (line.IndexOf("Read", System.StringComparison.OrdinalIgnoreCase) == 0 && line.Length > 5) {
                            if (!ReadFile(line.Substring(5), lines)) {
                                return;
                            }
                        }

                        TASInput input = new TASInput(line, lines, 0);
                        if (input.Frames != 0) {
                            inputs.Add(input);
                            if (input.TAS) {
                                ShowTAS = false;
                            }
                        }
                    } else {
                        lines++;
                        fixedRandom = int.Parse(line);
                        firstLine = false;
                    }
                }
            }
        }
        private bool ReadFile(string extraFile, int lines) {
            if (!File.Exists(extraFile)) { return true; }

            int subLine = 0;
            using (StreamReader sr = new StreamReader(extraFile)) {
                while (!sr.EndOfStream) {
                    string line = sr.ReadLine();

                    if (line.IndexOf("Stop", System.StringComparison.OrdinalIgnoreCase) == 0) { return false; }

                    subLine++;
                    if (Break == 0 && line.IndexOf("BreakQuick", System.StringComparison.OrdinalIgnoreCase) == 0) {
                        FastForward = true;
                    }
                    if (Break == 0 && line.IndexOf("Break", System.StringComparison.OrdinalIgnoreCase) == 0) {
                        Break = lines + subLine - 1;
                        continue;
                    }

                    TASInput input = new TASInput(line, lines, subLine);
                    if (input.Frames != 0) {
                        inputs.Add(input);
                        if (input.TAS) {
                            ShowTAS = false;
                        }
                    }
                }
            }
            return true;
        }
    }
}