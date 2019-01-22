using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Threading;

namespace Mechanix
{
    /// <summary>
    /// Represents a set of mutable <see cref="PointMass"/> entities, that is obeyed by the sets of force evaluation laws
    /// </summary>
    /// <typeparam name="TEntityKey"></typeparam>
    public class PhysicalContext<TEntityKey> : IEnumerable<KeyValuePair<TEntityKey, PointMass>>
    {
        readonly PointMass[] _entities;
        readonly Func<PhysicalContext<TEntityKey>, Force>[][] _forceEvaluationLaws;
        readonly Force[][] _forceValues;
        readonly Dictionary<TEntityKey, int> _indexes;
        bool _isLocked;

        readonly int _capacity;
        /// <summary>
        /// Required number of entities to begin simulation
        /// </summary>
        public int Capacity => _capacity;

        bool _isFilled;
        /// <summary>
        /// <see langword="true"/> if context is filled and simulation is allowed
        /// </summary>
        public bool IsFilled => _isFilled;

        readonly double _timePerTick;
        /// <summary>
        /// A period of time, during that all force values are considered to be uniform
        /// </summary>
        public double TimePerTick => _timePerTick;

        ulong _ticks;
        /// <summary>
        /// Count of <see cref="Tick(bool)"/> calls
        /// </summary>
        public ulong Ticks => _ticks;

        /// <summary>
        /// <para>
        /// Time in the context passed since this <see cref="PhysicalContext{TEntityKey}"/> was created.
        /// </para>
        /// Each time the method <see cref="Tick()"/> is called, 
        /// <see cref="Timer"/> is increased by <see cref="TimePerTick" />
        /// </summary>
        public double Timer => Ticks * TimePerTick;

        /// <summary>
        /// Occurres each time the <see cref="Tick()"/> method is called
        /// </summary>
        /// <remarks>
        /// This event raises before context is unlocked, 
        /// so <see cref="OnTick"/> handlers can't call <see cref="Tick()"/> method
        /// </remarks>
        public event EventHandler<EventArgs> OnTick;

        public IEnumerable<TEntityKey> Keys => _indexes.Keys;

        public IEnumerable<PointMass> Entities
        {
            get
            {
                for (int i = 0; i < _indexes.Count; ++i)
                {
                    yield return _entities[i];
                }
                yield break;
            }
        }

        public int Count => _indexes.Count;

        /// <summary>
        /// A <see cref="PointMass"/> entity with key <paramref name="entityKey"/>
        /// </summary>
        /// <exception cref="UnexistingEntityException{TEntityKey}">
        /// Throws, if entity with this key hasn't been added to this context yet
        /// </exception>
        public ref readonly PointMass this[TEntityKey entityKey]
        {
            get
            {
                try
                {
                    return ref _entities[_indexes[entityKey]];
                }
                catch (KeyNotFoundException)
                {
                    throw new UnexistingEntityException<TEntityKey>(this, entityKey);
                }
            }
        }

        public PhysicalContext(double timePerTick, int capacity)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException("Context capacity can't be < 0", nameof(capacity));
            _capacity = capacity;

            _timePerTick = timePerTick;
            _ticks = 0;
            _indexes = new Dictionary<TEntityKey, int>(capacity);
            _forceValues = new Force[capacity][];
            _entities = new PointMass[capacity];
            _forceEvaluationLaws = new Func<PhysicalContext<TEntityKey>, Force>[capacity][];
            _isLocked = false;
            _isFilled = false;
        }

        /// <summary>
        /// Adds <see cref="PointMass"/> entity to this context and binds some force evaluation laws, that affect it
        /// </summary>
        public void AddEntity(TEntityKey key, in PointMass entity, params Func<PhysicalContext<TEntityKey>, Force>[] forceEvaluationLaws)
        {
            if (IsFilled) throw new FilledPhysicalContextException<TEntityKey>(this);

            var index = _indexes.Count;

            _entities[index] = entity;
            _forceEvaluationLaws[index] = (Func<PhysicalContext<TEntityKey>, Force>[])forceEvaluationLaws.Clone();
            _forceValues[index] = new Force[forceEvaluationLaws.Length];

            _indexes.Add(key, index);
            if (_indexes.Count == Capacity) _isFilled = true;
        }

        /// <summary>
        /// Updates all entities as <paramref name="timeSpan"/> was wasted
        /// </summary>
        /// <param name="usingMultithreading">
        /// If <see langword="true"/>, then calculating next 
        /// state of entities will be paralleled 
        /// (and exceptions that have occured while force values evaluating will be wrapped into <see cref="AggregateException"/>)
        /// </param>
        /// <exception cref="AggregateException"> </exception>
        /// <exception cref="LockedPhysicalContextException{TEntityKey}"> </exception>
        /// <exception cref="UninitializedPhysicalContextException{TEntityKey}"> </exception>
        public void Tick(double timeSpan, bool usingMultithreading = true)
        {
            ulong count = (ulong)Math.Round(timeSpan / _timePerTick);
            for (ulong t = 0; t < count; t++) Tick(usingMultithreading);
        }

        /// <summary>
        /// Updates all entities as <paramref name="timeSpan"/> was wasted
        /// </summary>
        /// <param name="usingMultithreading">
        /// If <see langword="true"/>, then calculating next 
        /// state of entities will be paralleled 
        /// (and exceptions that have occured while force values evaluating will be wrapped into <see cref="AggregateException"/>)
        /// </param>
        /// <exception cref="AggregateException"> </exception>
        /// <exception cref="LockedPhysicalContextException{TEntityKey}"> </exception>
        /// <exception cref="UninitializedPhysicalContextException{TEntityKey}"> </exception>
        public void Tick(double timeSpan, CancellationToken cancellationToken, bool usingMultithreading = true)
        {
            ulong count = (ulong)Math.Round(timeSpan / _timePerTick);
            if (cancellationToken.CanBeCanceled)
            {
                for (ulong t = 0; t < count; t++)
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    Tick(usingMultithreading);
                }
            }
            else Tick(timeSpan, usingMultithreading);
        }

        /// <summary>
        /// Updates all entities as <see cref="TimePerTick"/> was wasted
        /// </summary>
        /// <param name="usingMultithreading">
        /// If <see langword="true"/>, then calculating next 
        /// state of entities will be paralleled 
        /// (and exceptions that have occured while force values evaluating will be wrapped into <see cref="AggregateException"/>)
        /// </param>
        /// <exception cref="AggregateException"> </exception>
        /// <exception cref="LockedPhysicalContextException{TEntityKey}"> </exception>
        /// <exception cref="UninitializedPhysicalContextException{TEntityKey}"> </exception>
        public void Tick(bool usingMultithreading = true)
        {
            if (!_isFilled) throw new UninitializedPhysicalContextException<TEntityKey>(this);
            if (_isLocked) throw new LockedPhysicalContextException<TEntityKey>(this);

            _isLocked = true;
            var count = _capacity;
            try
            {
                if (usingMultithreading)
                {
                    Parallel.For
                    (
                        0,
                        count,
                        c =>
                        {
                            for (int i = 0; i < _forceValues[c].Length; ++i)
                            {
                                _forceValues[c][i] = _forceEvaluationLaws[c][i].Invoke(this);
                            }
                        }
                    );
                }
                else
                {
                    for (int c = 0; c < count; ++c)
                    {
                        for (int i = 0; i < _forceValues[c].Length; ++i)
                        {
                            _forceValues[c][i] = _forceEvaluationLaws[c][i].Invoke(this);
                        }
                    }
                }
            }
            catch
            {
                _isLocked = false;
                throw;
            }

            if (usingMultithreading)
            {
                Parallel.For
                (
                    0,
                    count,
                    c =>
                    {
                        _entities[c] = _entities[c].Next(_timePerTick, _forceValues[c]);
                    }
                );
            }
            else
            {
                for (int c = 0; c < count; ++c)
                {
                    _entities[c] = _entities[c].Next(_timePerTick, _forceValues[c]);
                }
            }

            _ticks++;
            try
            {
                OnTick?.Invoke(this, EventArgs.Empty);
            }
            finally
            {
                _isLocked = false;
            }          
        }

        public bool ContainsKey(TEntityKey key)
        {
            return _indexes.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<TEntityKey, PointMass>> GetEnumerator()
        {
            foreach (var key in Keys)
            {
                yield return new KeyValuePair<TEntityKey, PointMass>(key, _entities[_indexes[key]]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
