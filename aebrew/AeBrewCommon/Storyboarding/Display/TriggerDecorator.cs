﻿using AeBrewCommon.Storyboarding.Commands;
using AeBrewCommon.Storyboarding.CommandValues;
using System;
using System.IO;

namespace AeBrewCommon.Storyboarding.Display
{
    public class TriggerDecorator<TValue> : ITypedCommand<TValue>
        where TValue : CommandValue
    {
        private readonly ITypedCommand<TValue> command;
        private double triggerTime;

        public OsbEasing Easing { get { throw new InvalidOperationException(); } }
        public double StartTime => triggerTime + command.StartTime;
        public double EndTime => triggerTime + command.EndTime;
        public TValue StartValue => command.StartValue;
        public TValue EndValue => command.EndValue;
        public double Duration => EndTime - StartTime;
        public bool Active { get; private set; }
        public int Cost => throw new InvalidOperationException();

        public event EventHandler OnStateChanged;

        public TriggerDecorator(ITypedCommand<TValue> command)
        {
            this.command = command;
        }

        public void Trigger(double time)
        {
            if (Active) return;

            Active = true;
            triggerTime = time;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void UnTrigger()
        {
            if (!Active) return;

            Active = false;
            OnStateChanged?.Invoke(this, EventArgs.Empty);
        }

        public TValue ValueAtTime(double time)
        {
            if (!Active) throw new InvalidOperationException("Not triggered");

            var commandTime = time - triggerTime;
            if (commandTime < command.StartTime) return command.StartValue;
            if (command.EndTime < commandTime) return command.EndValue;
            return command.ValueAtTime(commandTime);
        }

        public int CompareTo(ICommand other)
            => CommandComparer.CompareCommands(this, other);

        public void WriteOsb(TextWriter writer, ExportSettings exportSettings, int indentation)
        {
            throw new InvalidOperationException();
        }

        public override string ToString() => $"triggerable ({StartTime}s - {EndTime}s active:{Active})";

    }
}
