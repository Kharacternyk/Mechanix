﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Numerics;

namespace Mechanix
{
    /// <summary>
    /// Represents a set of mutable <see cref="PointMass"/> entities, that is obeyed by the sets of force evaluation laws
    /// </summary>
    /// <typeparam name="TEntityKey"></typeparam>
    public class PhysicalContext<TEntityKey> : IEnumerable<KeyValuePair<TEntityKey, PointMass>>
    {
        readonly double[][] _forceValues = new double[3][];
        readonly double[][] _velocities = new double[3][];
        readonly double[][] _positions = new double[3][];
        readonly double[] _masses;

        readonly Func<PhysicalContext<TEntityKey>, Force>[][] _forceEvaluationLaws;
        readonly Dictionary<TEntityKey, int> _indexes;
        bool _isLocked;

        /// <summary>
        /// Parallel options that is used during entities processing. 
        /// If is <see langword="null"/>, entities is processed sequentially
        /// </summary>
        public ParallelOptions EntitiesParallelOptions { get; set; } = null;

        /// <summary>
        /// Parallel options that is used during subscriber invoking. 
        /// If is <see langword="null"/>, subscribers are invoked sequentially in order of subscribing
        /// </summary>
        public ParallelOptions SubscribersParallelOptions { get; set; } = null;

        /// <summary>
        /// Required number of entities to begin simulation
        /// </summary>
        public int Capacity { get; }

        /// <summary>
        /// <see langword="true"/> if context is filled and simulation is allowed
        /// </summary>
        public bool IsFilled { get; private set; }

        /// <summary>
        /// A period of time, during that all force values are considered to be uniform
        /// </summary>
        public double TimePerTick { get; }

        /// <summary>
        /// Count of elapsed ticks
        /// </summary>
        public ulong Ticks { get; private set; }

        /// <summary>
        /// Time in the context passed since this <see cref="PhysicalContext{TEntityKey}"/> was created.
        /// </summary>
        public double Timer => Ticks * TimePerTick;

        /// <summary>
        /// Occurs when <see cref="TimePerTick"/> has elapsed
        /// </summary>
        /// <remarks>
        /// This event raises before context is unlocked, 
        /// so <see cref="OnTick"/> handlers can't call <see cref="Tick()"/> method
        /// </remarks>
        public event EventHandler OnTick;

        public IEnumerable<TEntityKey> Keys => _indexes.Keys;

        public IEnumerable<PointMass> Entities
        {
            get
            {
                for (int axis = 0; axis < _indexes.Count; ++axis)
                {
                    yield return new PointMass
                    (
                        new AxisStatus(_positions[0][axis], _velocities[0][axis]),
                        new AxisStatus(_positions[1][axis], _velocities[1][axis]),
                        new AxisStatus(_positions[2][axis], _velocities[2][axis]),
                        _masses[axis]
                    );
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
        public PointMass this[TEntityKey entityKey]
        {
            get
            {
                try
                {
                    int i = _indexes[entityKey];
                    return new PointMass
                    (
                        new AxisStatus(_positions[0][i], _velocities[0][i]),
                        new AxisStatus(_positions[1][i], _velocities[1][i]),
                        new AxisStatus(_positions[2][i], _velocities[2][i]),
                        _masses[i]
                    );
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
            if (timePerTick < 0) throw new ArgumentOutOfRangeException("Time per tick can't be < 0", nameof(TimePerTick));
            Capacity = capacity;

            TimePerTick = timePerTick;
            Ticks = 0;
            _indexes = new Dictionary<TEntityKey, int>(capacity);
            _forceEvaluationLaws = new Func<PhysicalContext<TEntityKey>, Force>[capacity][];

            for (int axis = 0; axis < 3; ++axis)
            {
                _forceValues[axis] = new double[capacity];
                _positions[axis] = new double[capacity];
                _velocities[axis] = new double[capacity];
            }
            _masses = new double[capacity];

            _isLocked = false;
            IsFilled = false;
        }

        /// <summary>
        /// Adds <see cref="PointMass"/> entity to this context and binds some force evaluation laws that affect it
        /// </summary>
        public void AddEntity(TEntityKey key, in PointMass entity, params Func<PhysicalContext<TEntityKey>, Force>[] forceEvaluationLaws)
        {
            if (IsFilled) throw new FilledPhysicalContextException<TEntityKey>(this);

            var index = _indexes.Count;

            _forceEvaluationLaws[index] = (Func<PhysicalContext<TEntityKey>, Force>[])forceEvaluationLaws.Clone();

            _positions[0][index] = entity.X.Position;
            _positions[1][index] = entity.Y.Position;
            _positions[2][index] = entity.Z.Position;

            _velocities[0][index] = entity.X.Velocity;
            _velocities[1][index] = entity.Y.Velocity;
            _velocities[2][index] = entity.Z.Velocity;

            _masses[index] = entity.Mass;

            _indexes.Add(key, index);
            if (_indexes.Count == Capacity) IsFilled = true;
        }

        #region Tick methods
        /// <summary>
        /// Updates all entities while result of <paramref name="tickWhilePredicate"/> is true
        /// </summary>
        /// <param name="tickWhilePredicate">
        /// Execution continues only while result of this predicate is true. Calculates after each <see cref="TimePerTick"/> wasted.
        /// </param>
        /// <exception cref="AggregateException"> </exception>
        /// <exception cref="LockedPhysicalContextException{TEntityKey}"> </exception>
        /// <exception cref="UninitializedPhysicalContextException{TEntityKey}"> </exception>
        public void Tick(Func<PhysicalContext<TEntityKey>, bool> tickWhilePredicate, bool parallelEntities = true)
        {
            if (!IsFilled) throw new UninitializedPhysicalContextException<TEntityKey>(this);
            if (_isLocked) throw new LockedPhysicalContextException<TEntityKey>(this);

            _isLocked = true;
            try
            {
                while (tickWhilePredicate(this)) UpdateEntities();
            }
            catch (Exception e) when (!(e is AggregateException)) 
            {
                //Seq loop was used, so we should wrap exception into Aggregate
                throw new AggregateException(e);
            }
            finally
            {
                _isLocked = false;
            }
        }

        /// <summary>
        /// Updates all entities as <paramref name="timeSpan"/> was wasted
        /// </summary>
        /// <exception cref="AggregateException"> </exception>
        /// <exception cref="LockedPhysicalContextException{TEntityKey}"> </exception>
        /// <exception cref="UninitializedPhysicalContextException{TEntityKey}"> </exception>
        public void Tick(double timeSpan)
        {
            if (!IsFilled) throw new UninitializedPhysicalContextException<TEntityKey>(this);
            if (_isLocked) throw new LockedPhysicalContextException<TEntityKey>(this);

            _isLocked = true;
            try
            {
                ulong count = (ulong)Math.Round(timeSpan / TimePerTick);
                for (ulong t = 0; t < count; t++) UpdateEntities();
            }
            catch (Exception e) when (!(e is AggregateException))
            {
                //Seq loop was used, so we should wrap exception into Aggregate
                throw new AggregateException(e);
            }
            finally
            {
                _isLocked = false;
            }
        }

        /// <summary>
        /// Updates all entities as <paramref name="timeSpan"/> was wasted
        /// </summary>
        /// <param name="tickWhilePredicate">
        /// Execution continues only while result of this predicate is true. Calculates after each <see cref="TimePerTick"/> wasted.
        /// </param>
        /// <returns>
        /// Whether <paramref name="tickWhilePredicate"/> was <see langword="true"/> all the <paramref name="timeSpan"/>
        /// </returns>
        /// <exception cref="AggregateException"> </exception>
        /// <exception cref="LockedPhysicalContextException{TEntityKey}"> </exception>
        /// <exception cref="UninitializedPhysicalContextException{TEntityKey}"> </exception>
        public bool Tick(double timeSpan, Func<PhysicalContext<TEntityKey>, bool> tickWhilePredicate)
        {
            if (!IsFilled) throw new UninitializedPhysicalContextException<TEntityKey>(this);
            if (_isLocked) throw new LockedPhysicalContextException<TEntityKey>(this);

            _isLocked = true;
            try
            {
                ulong count = (ulong)Math.Round(timeSpan / TimePerTick);
                for (ulong t = 0; t < count; t++)
                {
                    if (!tickWhilePredicate(this)) return false;
                    UpdateEntities();
                }
            }
            catch (Exception e) when (!(e is AggregateException))
            {
                //Seq loop was used, so we should wrap exception into Aggregate
                throw new AggregateException(e);
            }
            finally
            {
                _isLocked = false;
            }

            return true;
        }

        /// <summary>
        /// Updates all entities as <see cref="TimePerTick"/> was wasted
        /// </summary>
        /// <exception cref="AggregateException"> </exception>
        /// <exception cref="LockedPhysicalContextException{TEntityKey}"> </exception>
        /// <exception cref="UninitializedPhysicalContextException{TEntityKey}"> </exception>
        public void Tick()
        {
            if (!IsFilled) throw new UninitializedPhysicalContextException<TEntityKey>(this);
            if (_isLocked) throw new LockedPhysicalContextException<TEntityKey>(this);

            _isLocked = true;
            try
            {
                UpdateEntities();
            }
            catch (Exception e) when (!(e is AggregateException))
            {
                //Seq loop was used, so we should wrap exception into Aggregate
                throw new AggregateException(e);
            }
            finally
            {
                _isLocked = false;
            }
        }
        #endregion

        [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
        void UpdateEntities()
        {
            var count = Capacity;

            if (EntitiesParallelOptions is null)
            {
                for (int c = 0; c < count; ++c)
                {
                    Force force = Force.Zero;
                    for (int i = 0; i < _forceEvaluationLaws[c].Length; ++i)
                    {
                        force += _forceEvaluationLaws[c][i].Invoke(this);
                    }
                    _forceValues[0][c] = force.XComponent;
                    _forceValues[1][c] = force.YComponent;
                    _forceValues[2][c] = force.ZComponent;
                }

                int lastHandledIndex = 0;

                for (; lastHandledIndex + Vector<double>.Count < count; lastHandledIndex += Vector<double>.Count)
                {
                    for (int axis = 0; axis < 3; ++axis)
                    {
                        Vector<double> accelerations = new Vector<double>(_forceValues[axis], lastHandledIndex);
                        Vector<double> masses = new Vector<double>(_masses, lastHandledIndex);
                        accelerations /= masses;

                        Vector<double> velocities = new Vector<double>(_velocities[axis], lastHandledIndex);
                        Vector<double> positions = new Vector<double>(_positions[axis], lastHandledIndex);

                        positions += (velocities * TimePerTick);
                        velocities += (accelerations * TimePerTick);

                        positions.CopyTo(_positions[axis], lastHandledIndex);
                        velocities.CopyTo(_velocities[axis], lastHandledIndex);
                    }
                }

                for (int c = lastHandledIndex; c < count; ++c)
                {
                    for (int axis = 0; axis < 3; ++axis)
                    {
                        _positions[axis][c] += _velocities[axis][c] * TimePerTick;
                        _velocities[axis][c] += _forceValues[axis][c] / _masses[c] * TimePerTick;
                    }
                }

            }
            else
            {
                Parallel.For
                (
                    0,
                    count,
                    EntitiesParallelOptions,
                    c =>
                    {
                        Force force = Force.Zero;
                        for (int i = 0; i < _forceValues[c].Length; ++i)
                        {
                            force += _forceEvaluationLaws[c][i].Invoke(this);
                        }
                        _forceValues[0][c] = force.XComponent;
                        _forceValues[1][c] = force.YComponent;
                        _forceValues[2][c] = force.ZComponent;
                    }
                );
                //TODO: parallel
                int lastC = 0;
                for (int c = 0; c < count; c += Vector<double>.Count)
                {
                    for (int axis = 0; axis < 3; ++axis)
                    {
                        Vector<double> accelerations = new Vector<double>(_forceValues[axis], c);
                        Vector<double> masses = new Vector<double>(_masses, c);
                        accelerations /= masses;

                        Vector<double> velocities = new Vector<double>(_velocities[axis], c);
                        Vector<double> positions = new Vector<double>(_positions[axis], c);

                        velocities += (accelerations * TimePerTick);
                        positions += (velocities * TimePerTick);

                        positions.CopyTo(_positions[axis], c);
                        velocities.CopyTo(_velocities[axis], c);
                    }
                    lastC = c;
                }
                for (int c = lastC; c < count; ++c)
                {
                    for (int axis = 0; axis < 3; ++axis)
                    {
                        _positions[axis][c] += _velocities[axis][c] * TimePerTick;
                        _velocities[axis][c] += _forceValues[axis][c] / _masses[c] * TimePerTick;
                    }
                }
            }

            Ticks++;

            if (OnTick is null) return;

            if (SubscribersParallelOptions is null)
            {
                OnTick.Invoke(this, EventArgs.Empty);
            }
            else
            {
                //TODO: remove calling of GetInvokationList, because it requires array allocating in heap 
                var subscribers = OnTick.GetInvocationList();
                Parallel.ForEach
                (
                    subscribers,
                    SubscribersParallelOptions,
                    sub => ((EventHandler)sub).Invoke(this, EventArgs.Empty)
                );
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
                yield return new KeyValuePair<TEntityKey, PointMass>(key, this[key]);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
