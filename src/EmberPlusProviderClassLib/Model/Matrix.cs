﻿#region copyright
/*
 * This code is from the Lawo/ember-plus GitHub repository and is licensed with
 *
 * Boost Software License - Version 1.0 - August 17th, 2003
 *
 * Permission is hereby granted, free of charge, to any person or organization
 * obtaining a copy of the software and accompanying documentation covered by
 * this license (the "Software") to use, reproduce, display, distribute,
 * execute, and transmit the Software, and to prepare derivative works of the
 * Software, and to permit third-parties to whom the Software is furnished to
 * do so, all subject to the following:
 *
 * The copyright notices in the Software and this entire statement, including
 * the above license grant, this restriction and the following disclaimer,
 * must be included in all copies of the Software, in whole or in part, and
 * all derivative works of the Software, unless such copies or derivative
 * works are solely in the form of machine-executable object code generated by
 * a source language processor.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE, TITLE AND NON-INFRINGEMENT. IN NO EVENT
 * SHALL THE COPYRIGHT HOLDERS OR ANYONE DISTRIBUTING THE SOFTWARE BE LIABLE
 * FOR ANY DAMAGES OR OTHER LIABILITY, WHETHER IN CONTRACT, TORT OR OTHERWISE,
 * ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
 #endregion

using System;
using System.Collections.Generic;
using System.Linq;
using EmberPlusProviderClassLib.Model.Parameters;

namespace EmberPlusProviderClassLib.Model
{
    public abstract class Matrix : Element
    {
        protected Matrix(int number,
                        Element parent,
                        string identifier,
                        Dispatcher dispatcher,
                        IEnumerable<Signal> targets,
                        IEnumerable<Signal> sources,
                        Node labelsNode,
                        bool? isWritable,
                        int? targetCount,
                        int? sourceCount,
                        Signal blindSource)
        : base(number, parent, identifier)
        {
            Dispatcher = dispatcher;
            LabelsNode = labelsNode;

            IsWritable = isWritable ?? true;

            _targets = new List<Signal>(targets);
            _sources = new List<Signal>(sources);

            _targetCount = targetCount ?? _targets.Count;
            _sourceCount = sourceCount ?? _sources.Count;

            _blindSource = blindSource ?? null;
        }

        public Dispatcher Dispatcher { get; }
        public Node LabelsNode { get; }
        public bool IsWritable { get; }

        public IEnumerable<Signal> Targets => _targets;

        public IEnumerable<Signal> Sources => _sources;

        public Signal BlindSource => _blindSource;

        public int TargetCount => _targetCount;

        public int SourceCount => _sourceCount;

        public Signal GetTarget(int number)
        {
            return (from signal in _targets
                    where signal.Number == number
                    select signal)
                    .FirstOrDefault();
        }

        public Signal GetSource(int number)
        {
            return (from signal in _sources
                    where signal.Number == number
                    select signal)
                    .FirstOrDefault();
        }

        public bool Connect(Signal target, IEnumerable<Signal> sources, object state, ConnectOperation operation = ConnectOperation.Absolute)
        {
            if(_targets.Contains(target) == false)
                throw new ArgumentException("target");

            var firstSource = sources.FirstOrDefault();

            if (firstSource != null)
            {
                if (_sources.Contains(firstSource) == false) {
                    throw new ArgumentException("sources");
                }
            }

            var result = ConnectOverride(target, sources, operation);

            if (result)
            {
                // absolute(0)
                // Default.This value indicates that the list of source numbers in the sources property of
                // the encompassing Connection object is absolute.When a provider tallies a connection, it
                // must always use this value, independent of the value of the disposition property.
                // Dispatcher.NotifyMatrixConnection(this, target, state, operation);
                Dispatcher.NotifyMatrixConnection(this, target, state, ConnectOperation.Absolute);
            }

            return result;
        }

        protected abstract bool ConnectOverride(Signal target, IEnumerable<Signal> sources, ConnectOperation operation);

        readonly List<Signal> _targets;
        readonly List<Signal> _sources;
        readonly Signal _blindSource;
        readonly int _targetCount;
        readonly int _sourceCount;
    }

    public enum ConnectOperation
    {
        /// <summary>
        /// Absolute makes sure that there is only the sources selected that gets connected.
        /// Default. This value indicates that the list of source numbers in the sources property of
        /// the encompassing Connection object is absolute. When a provider tallies a connection, it
        /// must always use this value, independent of the value of the disposition property.
        /// </summary>
        Absolute,
        Connect,
        Disconnect,
    }
}