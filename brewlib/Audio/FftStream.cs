﻿using ManagedBass;
using System;

namespace BrewLib.Audio
{
    public class FftStream : IDisposable
    {
        private readonly string path;
        private int stream;
        
        private readonly float frequency;
        /// <summary>
        /// Samples per second
        /// </summary>
        public float Frequency => frequency;

        public double Duration { get; }

        public FftStream(string path)
        {
            this.path = path;
            stream = Bass.CreateStream(path, 0, 0, BassFlags.Decode | BassFlags.Prescan);
            Duration = Bass.ChannelBytes2Seconds(stream, Bass.ChannelGetLength(stream));

            Bass.ChannelGetAttribute(stream, ChannelAttribute.Frequency, out frequency);
        }

        public float[] GetFft(double time)
        {
            var position = Bass.ChannelSeconds2Bytes(stream, time);
            Bass.ChannelSetPosition(stream, position);

            var data = new float[1024];
            Bass.ChannelGetData(stream, data, unchecked((int)DataFlags.FFT2048));
            return data;
        }

        #region IDisposable Support

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }
                Bass.StreamFree(stream);
                stream = 0;
                disposedValue = true;
            }
        }

        ~FftStream()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
