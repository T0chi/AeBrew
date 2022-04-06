﻿using BrewLib.Audio;
using AeBrewCommon.Mapset;
using AeBrewCommon.Storyboarding;
using AeBrewEditor.Mapset;
using AeBrewEditor.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AeBrewEditor.Storyboarding
{
    public class EditorGeneratorContext : GeneratorContext
    {
        private readonly Effect effect;
        private readonly MultiFileWatcher watcher;

        private readonly string projectPath;
        public override string ProjectPath => projectPath;

        private readonly string projectAssetPath;
        public override string ProjectAssetPath => projectAssetPath;

        private readonly string mapsetPath;
        public override string MapsetPath
        {
            get
            {
                if (!Directory.Exists(mapsetPath))
                    throw new InvalidOperationException($"The mapset folder at '{mapsetPath}' doesn't exist");

                return mapsetPath;
            }
        }

        private readonly EditorBeatmap beatmap;
        public override Beatmap Beatmap
        {
            get
            {
                BeatmapDependent = true;
                return beatmap;
            }
        }

        private readonly IEnumerable<EditorBeatmap> beatmaps;
        public override IEnumerable<Beatmap> Beatmaps
        {
            get
            {
                BeatmapDependent = true;
                return beatmaps;
            }
        }

        public bool BeatmapDependent { get; private set; }

        private readonly StringBuilder log = new StringBuilder();
        public string Log => log.ToString();

        public List<EditorStoryboardLayer> EditorLayers = new List<EditorStoryboardLayer>();

        public EditorGeneratorContext(Effect effect, string projectPath, string projectAssetPath, string mapsetPath, EditorBeatmap beatmap, IEnumerable<EditorBeatmap> beatmaps, MultiFileWatcher watcher)
        {
            this.projectPath = projectPath;
            this.projectAssetPath = projectAssetPath;
            this.mapsetPath = mapsetPath;
            this.effect = effect;
            this.beatmap = beatmap;
            this.beatmaps = beatmaps;
            this.watcher = watcher;
        }

        public override StoryboardLayer GetLayer(string identifier)
        {
            var layer = EditorLayers.Find(l => l.Identifier == identifier);
            if (layer == null) EditorLayers.Add(layer = new EditorStoryboardLayer(identifier, effect));
            return layer;
        }

        public override void AddDependency(string path)
            => watcher.Watch(path);

        public override void AppendLog(string message)
            => log.AppendLine(message);

        #region Audio data

        private Dictionary<string, FftStream> fftAudioStreams = new Dictionary<string, FftStream>();
        private FftStream getFftStream(string path)
        {
            path = Path.GetFullPath(path);

            if (!fftAudioStreams.TryGetValue(path, out FftStream audioStream))
                fftAudioStreams[path] = audioStream = new FftStream(path);

            return audioStream;
        }

        public override double AudioDuration
            => getFftStream(effect.Project.AudioPath).Duration * 1000;

        public override float[] GetFft(double time, string path = null)
            => getFftStream(path ?? effect.Project.AudioPath).GetFft(time * 0.001);

        public override float GetFftFrequency(string path = null)
            => getFftStream(path ?? effect.Project.AudioPath).Frequency;

        #endregion

        public void DisposeResources()
        {
            foreach (var audioStream in fftAudioStreams.Values)
                audioStream.Dispose();
            fftAudioStreams = null;
        }
    }
}
